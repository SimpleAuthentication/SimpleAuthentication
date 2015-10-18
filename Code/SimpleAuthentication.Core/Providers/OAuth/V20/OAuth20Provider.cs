using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Tracing;

namespace SimpleAuthentication.Core.Providers.OAuth.V20
{
    public abstract class OAuth20Provider : IAuthenticationProvider
    {
        private string _scopeKey;
        private string _scopeSeparator;
        private string _stateKey;

        protected OAuth20Provider(ProviderParams providerParams)
        {
            if (providerParams == null)
            {
                throw new ArgumentNullException("providerParams");
            }

            PublicApiKey = providerParams.PublicApiKey;
            SecretApiKey = providerParams.SecretApiKey;

            // Optional.
            Scopes = providerParams.Scopes;

            // Defaults.
            ScopeKey = "scope";
            ScopeSeparator = ",";
            StateKey = "state";
        }

        #region IAuthenticationProvider Implementation

        public abstract string Name { get; }

        public string Description
        {
            get { return "OAuth 2.0"; }
        }

        public abstract Task<RedirectToAuthenticateSettings> GetRedirectToAuthenticateSettingsAsync(Uri callbackUrl);

        public async Task<IAuthenticatedClient> AuthenticateClientAsync(IDictionary<string, string> queryString,
            string state,
            Uri callbackUri)
        {
            if (queryString == null ||
                queryString.Count <= 0)
            {
                throw new ArgumentNullException("queryString");
            }

            if (string.IsNullOrWhiteSpace(state))
            {
                throw new ArgumentNullException("state");
            }

            if (callbackUri == null)
            {
                throw new ArgumentNullException("callbackUri");
            }

            //TraceSource.TraceInformation("Callback parameters: " +
            //                             string.Join("&",
            //                                 queryString.Keys.Select(
            //                                     key => key + "=" + queryString[key]).ToArray()));

            SystemHelpers.CrossSiteRequestForgeryCheck(queryString,
                state,
                StateKey);

            EnsureResultIsNotAnError(queryString);

            //TraceSource.TraceVerbose("Retrieving the Authorization Code.");
            var authorizationCode = GetAuthorizationCodeFromQueryString(queryString);
            //TraceSource.TraceVerbose("Authorization Code retrieved.");

            //TraceSource.TraceInformation("Authorization Code: {0}. {1}",
            //    string.IsNullOrEmpty(authorizationCode)
            //        ? "-no authorization code-"
            //        : authorizationCode,
            //    accessToken.ToString());

            //TraceSource.TraceVerbose("Retrieving the Access Token.");
            var accessToken = await GetAccessTokenAsync(authorizationCode, callbackUri);
            //TraceSource.TraceVerbose("Access Token retrieved.");

            //TraceSource.TraceVerbose("Retrieving user information.");
            var userInformationContent = await GetUserInformationAsync(accessToken);

            var userInformation = GetUserInformationFromContent(userInformationContent);

            //TraceSource.TraceVerbose("User information retrieved.");

            var authenticatedClient = new AuthenticatedClient(Name,
                accessToken,
                userInformation,
                userInformationContent);

            //TraceSource.TraceInformation(authenticatedClient.ToString());

            return authenticatedClient;
        }

        #endregion

        public string PublicApiKey { get; private set; }
        public string SecretApiKey { get; private set; }

        public string ScopeKey
        {
            get { return _scopeKey; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException("value");
                }
                _scopeKey = value;
            }
        }

        public string ScopeSeparator
        {
            get { return _scopeSeparator; }
            set
            {
                // NOTE: can be a 'space'.
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }
                _scopeSeparator = value;
            }
        }

        public IEnumerable<string> Scopes { get; set; }

        public string StateKey
        {
            get { return _stateKey; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException("value");
                }
                _stateKey = value;
            }
        }

        public ITraceManager TraceManager { set; private get; }

        protected abstract IEnumerable<string> DefaultScopes { get; }

        protected abstract Uri AccessTokenUri { get; }

        protected abstract Uri UserInformationUri(AccessToken accessToken);

        protected virtual string UserAgent
        {
            get { return null; }
        }

        protected async Task<AccessToken> GetAccessTokenAsync(string authorizationCode,
            Uri redirectUrl)
        {
            if (string.IsNullOrWhiteSpace(authorizationCode))
            {
                throw new ArgumentNullException("authorizationCode");
            }

            if (redirectUrl == null ||
                string.IsNullOrWhiteSpace(redirectUrl.AbsoluteUri))
            {
                throw new ArgumentNullException("redirectUrl");
            }

            HttpResponseMessage response;

            using (var client = HttpClientFactory.GetHttpClient())
            {
                var postData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", PublicApiKey),
                    new KeyValuePair<string, string>("client_secret", SecretApiKey),
                    new KeyValuePair<string, string>("redirect_uri", redirectUrl.AbsoluteUri),
                    new KeyValuePair<string, string>("code", authorizationCode),
                    new KeyValuePair<string, string>("grant_type", "authorization_code")
                };

                var encodedContent = new FormUrlEncodedContent(postData);

                var requestUri = AccessTokenUri;

                //TraceSource.TraceVerbose("Retrieving Access Token endpoint: {0}",
                //    requestUri.AbsoluteUri);

                response = await client.PostAsync(requestUri, encodedContent);
            }

            // RANT: Facebook send back all their data as Json except this f'ing endpoint.
            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorMessage =
                    string.Format(
                        "Failed to retrieve an Access Token from {0}. Status Code: {1}. Error Message: {2}",
                        Name,
                        response.StatusCode,
                        content);

                throw new AuthenticationException(errorMessage);
            }

            return MapAccessTokenContentToAccessToken(content);
        }

        protected virtual async Task<string> GetUserInformationAsync(AccessToken accessToken)
        {
            if (accessToken == null)
            {
                throw new ArgumentNullException("accessToken");
            }

            if (string.IsNullOrWhiteSpace(accessToken.Token))
            {
                throw new ArgumentException("accessToken.Token");
            }

            try
            {
                string content;

                using (var client = HttpClientFactory.GetHttpClient())
                {
                    var requestUri = UserInformationUri(accessToken);

                    // Some API's (like GitHub) require a UserAgent header.
                    if (!string.IsNullOrWhiteSpace(UserAgent))
                    {
                        client.DefaultRequestHeaders.Add("user-agent", UserAgent);
                    }

                    //TraceSource.TraceVerbose("Retrieving user information. Google Endpoint: {0}",
                    //    requestUri.AbsoluteUri);
                    content = await client.GetStringAsync(requestUri);
                }

                return content;
            }
            catch (Exception exception)
            {
                var errorMessage =
                    string.Format("Failed to retrieve UserInfo data from the {0} Api. Error Messages: {1}",
                        Name,
                        exception.RecursiveErrorMessages());
                //TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage, exception);
            }
        }

        protected abstract UserInformation GetUserInformationFromContent(string content);

        public RedirectToAuthenticateSettings GetRedirectToAuthenticateSettings(Uri callbackUrl,
            Uri providerAuthenticationUrl)
        {
            if (callbackUrl == null)
            {
                throw new ArgumentNullException("callbackUrl");
            }

            // Validations.
            if (providerAuthenticationUrl == null)
            {
                throw new AuthenticationException(
                    "ProviderAuthenticationUrl has no value. Please set the authentication Url location to redirect to. eg. http://www.google.com/some/oauth/authenticate/url");
            }

            // Generate some state which will be used in the redirection uri and used for CSRF checks.
            var state = Guid.NewGuid().ToString();

            // Now the redirection uri.
            var redirectUri = string.Format("{0}?{1}",
                providerAuthenticationUrl.AbsoluteUri,
                CreateRedirectionQuerystringParameters(callbackUrl, state));

            return new RedirectToAuthenticateSettings
            {
                RedirectUri = new Uri(redirectUri),
                State = state
            };
        }

        #region GetRedirectToAuthenticateSettings related methods

        protected virtual string CreateRedirectionQuerystringParameters(Uri callbackUri, string state)
        {
            if (callbackUri == null)
            {
                throw new ArgumentNullException("callbackUri");
            }

            if (string.IsNullOrWhiteSpace(state))
            {
                throw new ArgumentNullException("state");
            }

            var queryString = new StringBuilder();
            queryString.AppendFormat("client_id={0}&redirect_uri={1}&response_type=code",
                Uri.EscapeDataString(PublicApiKey),
                Uri.EscapeDataString(callbackUri.AbsoluteUri));

            var scopesParameter = GetScopes();
            if (!string.IsNullOrWhiteSpace(scopesParameter))
            {
                queryString.AppendFormat("&{0}", scopesParameter);
            }

            if (!string.IsNullOrWhiteSpace(state))
            {
                var stateKey = string.IsNullOrWhiteSpace(StateKey)
                    ? "state"
                    : StateKey;
                queryString.AppendFormat("&{0}={1}",
                    Uri.EscapeDataString(stateKey),
                    Uri.EscapeDataString(state));
            }

            return queryString.ToString();
        }

        private string GetScopes()
        {
            var scopes = Scopes != null &&
                         Scopes.Any()
                ? Scopes
                : DefaultScopes != null &&
                  DefaultScopes.Any()
                    ? DefaultScopes
                    : null;

            if (scopes == null)
            {
                return string.Empty;
            }

            // NOTE: sepatator can be a space.
            var separator = string.IsNullOrEmpty(ScopeSeparator)
                ? ","
                : ScopeSeparator;
            var scopesJoined = string.Join(separator, scopes).Trim();

            var key = string.IsNullOrWhiteSpace(ScopeKey)
                ? "scope"
                : ScopeKey;

            return string.IsNullOrWhiteSpace(scopesJoined)
                ? string.Empty
                : string.Format("{0}={1}",
                    Uri.EscapeDataString(key),
                    Uri.EscapeDataString(scopesJoined));
        }

        #endregion

        #region AuthenticateClientAsync related methods

        protected virtual void EnsureResultIsNotAnError(IDictionary<string, string> querystring)
        {
            if (querystring == null)
            {
                throw new ArgumentNullException("querystring");
            }

            if (querystring.Count <= 0)
            {
                throw new ArgumentOutOfRangeException("querystring");
            }

            const string errorKey = "error";
            if (querystring.ContainsKey(errorKey))
            {
                var error = querystring[errorKey];
                var errorMessage =
                    string.Format("Failed to retrieve an authorization code from {0}. The error provided is: {1}",
                        Name,
                        error);
                throw new AuthenticationException(errorMessage);
            }
        }

        protected virtual string GetAuthorizationCodeFromQueryString(IDictionary<string, string> queryString)
        {
            if (queryString == null)
            {
                throw new ArgumentNullException("queryString");
            }

            if (queryString.Count <= 0)
            {
                throw new ArgumentOutOfRangeException("queryString");
            }

            /* Documentation:
               Providers returns an authorization code to your application if the user grants your application the permissions it requested. 
               The authorization code is returned to your application in the query string parameter code. If the state parameter was included in the request,
               then it is also included in the response. */
            const string codeKey = "code";
            var code = queryString.ContainsKey(codeKey)
                ? queryString["code"]
                : null;

            if (string.IsNullOrWhiteSpace(code))
            {
                string errorMessage = string.Format(
                    "No code parameter provided in the response querystring from {0}.", Name);
                //TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return code;
        }

        protected virtual AccessToken MapAccessTokenContentToAccessToken(string content)
        {
            return MapAccessTokenContentToAccessTokenForSomeJson(content);
        }

        protected AccessToken MapAccessTokenContentToAccessTokenForSomeJson(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException("content");
            }

            var accessToken = JsonConvert.DeserializeObject<dynamic>(content);

            string token = accessToken.access_token;
            if (string.IsNullOrWhiteSpace(token))
            {
                var errorMessage =
                    string.Format(
                        "No 'access_token' key/value found in the {0} access token response. Content retrieved: {1}.",
                        Name,
                        content);
                throw new AuthenticationException(errorMessage);
            }

            string expiresInValue = accessToken.expires_in;
            if (string.IsNullOrWhiteSpace(expiresInValue))
            {
                var errorMessage =
                    string.Format(
                        "No 'expires_in' key/value found in the {0} access token response Content retrieved: {1}.",
                        Name,
                        content);
                throw new AuthenticationException(errorMessage);
            }

            var expiresIn = Convert.ToDouble(accessToken.expires_in, CultureInfo.InvariantCulture);
            return new AccessToken
            {
                Token = accessToken.access_token,
                ExpiresOn = expiresIn > 0
                    ? DateTime.UtcNow.AddSeconds(expiresIn)
                    : DateTime.MaxValue
            };
        }

        protected AccessToken MapAccessTokenContentToAccessTokenForSomeKeyValues(string content,
            string tokenKey = "access_token",
            string expiresOnKey = "expires")
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException("content");
            }

            if (string.IsNullOrWhiteSpace(tokenKey))
            {
                throw new ArgumentNullException("tokenKey");
            }

            if (string.IsNullOrWhiteSpace(expiresOnKey))
            {
                throw new ArgumentNullException("expiresOnKey");
            }

            // The data is in a key/value format. So lets split this data out.
            var keyValues = SystemHelpers.ConvertKeyValueContentToDictionary(content);
            if (keyValues == null ||
                !keyValues.Any())
            {
                var errorMessage =
                    string.Format("Failed to extract the access token from the body content. Content returned: {0}",
                        content);
                throw new AuthenticationException(errorMessage);
            }

            // NOTE: It's possible that we don't always have an expires key/value.
            //       Some places don't care about expiring an acess token, I guess.
            if (!keyValues.ContainsKey(tokenKey))
            {
                var errorMessage =
                    string.Format(
                        "Failed to extract the access token from the body content. Content needs both the '{0}' and '{1}' keys.",
                        tokenKey,
                        expiresOnKey);
                throw new AuthenticationException(errorMessage);
            }

            var expiresIn = keyValues.ContainsKey(expiresOnKey)
                ? Convert.ToDouble(keyValues[expiresOnKey], CultureInfo.InvariantCulture)
                : 0;

            return new AccessToken
            {
                Token = keyValues[tokenKey],
                ExpiresOn = expiresIn > 0
                    ? DateTime.UtcNow.AddSeconds(expiresIn)
                    : DateTime.MaxValue
            };
        }

        #endregion
    }
}