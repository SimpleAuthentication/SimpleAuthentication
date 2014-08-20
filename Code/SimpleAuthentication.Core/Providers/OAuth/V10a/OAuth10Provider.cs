using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using OAuth;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers.Twitter;

namespace SimpleAuthentication.Core.Providers.OAuth.V10a
{
    public abstract class OAuth10Provider : IAuthenticationProvider
    {
        private string _stateKey;

        protected OAuth10Provider(ProviderParams providerParams)
        {
            if (providerParams == null)
            {
                throw new ArgumentNullException("providerParams");
            }

            PublicApiKey = providerParams.PublicApiKey;
            SecretApiKey = providerParams.SecretApiKey;

            // NOTE: Scopes are currently ignored.

            StateKey = "state";
        }

        public string PublicApiKey { get; private set; }
        public string SecretApiKey { get; private set; }
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
        
        #region IAuthenticationProvider Implementation

        public abstract string Name { get; }
        public abstract string Description { get; }

        public abstract Task<RedirectToAuthenticateSettings> GetRedirectToAuthenticateSettingsAsync(Uri callbackUrl);

        public async Task<IAuthenticatedClient> AuthenticateClientAsync(IDictionary<string, string> querystring,
            string state,
            Uri callbackUrl)
        {
            if (querystring == null ||
                querystring.Count <= 0)
            {
                throw new ArgumentNullException("querystring");
            }

            if (string.IsNullOrWhiteSpace(state))
            {
                throw new ArgumentNullException("state");
            }

            // NOTE: CallbackUrl is not used in OAuth 1.0a.

            //TraceSource.TraceInformation("Callback parameters: " +
            //                             string.Join("&",
            //                                 queryString.Keys.Select(
            //                                     key => key + "=" + queryString[key]).ToArray()));

            SystemHelpers.CrossSiteRequestForgeryCheck(querystring,
                state,
                StateKey);

            var oAuthVerifier = GetVerifierTokenFromQuerystring(querystring);
            
            var accessToken = await GetAccessTokenAsync(oAuthVerifier);

            var userInformationContent = await GetUserInformationAsync(accessToken);
            var userInformation = GetUserInformationFromContent(userInformationContent);

            return new AuthenticatedClient(Name,
                accessToken,
                userInformation,
                userInformationContent);
        }

        #endregion

        protected abstract Uri GetRedirectToProviderUri(RequestToken requestToken);

        protected async Task<RedirectToAuthenticateSettings> GetRedirectToAuthenticateSettingsAsync(Uri callbackUrl,
            Uri providerAuthenticationUrl)
        {
            if (callbackUrl == null)
            {
                throw new ArgumentNullException("callbackUrl");
            }

            if (providerAuthenticationUrl == null)
            {
                throw new ArgumentNullException("providerAuthenticationUrl");
            }

            var requestToken = await GetRequestToken(callbackUrl, providerAuthenticationUrl);
            var redirectUrl = GetRedirectToProviderUri(requestToken);

            return new RedirectToAuthenticateSettings
            {
                RedirectUri = redirectUrl
            };
        }

        protected abstract AccessToken GetAccessTokenFromResponseContent(string content);

        protected abstract UserInformation GetUserInformationFromContent(string content);

        private async Task<RequestToken> GetRequestToken(Uri callbackUrl,
            Uri providerAuthenticationUrl)
        {
            if (callbackUrl == null)
            {
                throw new ArgumentNullException("callbackUrl");
            }

            if (providerAuthenticationUrl == null)
            {
                throw new ArgumentNullException("providerAuthenticationUrl");
            }

            OAuthRequest oAuthRequest = OAuthRequest.ForRequestToken(PublicApiKey, 
                SecretApiKey, 
                callbackUrl.AbsoluteUri);
            oAuthRequest.RequestUrl = "https://twitter.com/oauth/request_token";

            var authorizationHeader = oAuthRequest.GetAuthorizationHeader();

            HttpResponseMessage response;
            using (var client = HttpClientFactory.GetHttpClient())
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationHeader);
                response = await client.GetAsync(oAuthRequest.RequestUrl);
            }

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorMessage =
                    string.Format(
                        "Failed to retrieve an HttpStatus-OK while attempting to get a Request Token from Twitter. Status: {0}. Content: {1}.",
                        response.StatusCode,
                        content);
                throw new AuthenticationException(errorMessage);
            }

            return GetRequestTokenFromContet(content);
        }

        private static RequestToken GetRequestTokenFromContet(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException("content");
            }

            const string tokenKey = "oauth_token";
            const string secretKey = "oauth_token_secret";
            var keyValues = SystemHelpers.ConvertKeyValueContentToDictionary(content);

            var accessToken = new RequestToken
            {
                OAuthToken = keyValues.ContainsKey(tokenKey)
                    ? keyValues[tokenKey]
                    : null,
                OAuthTokenSecret = keyValues.ContainsKey(secretKey)
                    ? keyValues[secretKey]
                    : null
            };

            if (string.IsNullOrWhiteSpace(accessToken.OAuthToken) ||
                string.IsNullOrWhiteSpace(accessToken.OAuthTokenSecret))
            {
                var errorMessage =
                    string.Format(
                        "Twitter returned an Access Token but it requires both an '{0}' and '{1}' key/values in the response body. Body content returned: {2}",
                        tokenKey, 
                        secretKey,
                        string.IsNullOrWhiteSpace(content)
                            ? "-- no content --"
                            : content);
                throw new AuthenticationException(errorMessage);
            }

            return accessToken;
        }

        private OAuthVerifier GetVerifierTokenFromQuerystring(IDictionary<string, string> querystring)
        {
            if (querystring == null)
            {
                throw new ArgumentNullException("querystring");
            }

            const string tokenKey = "oauth_token";
            const string verifierKey = "oauth_verifier";

            foreach (var key in new[] { tokenKey, verifierKey })
            {
                if (!querystring.ContainsKey(key))
                {
                    var errorMessage =
                        string.Format(
                            "Verifier token response content from '{0}' expected the key/value '{1}' but none was retrieved.",
                            Name,
                            key);
                    throw new AuthenticationException(errorMessage);
                }
            }

            return new OAuthVerifier
            {
                Token = querystring[tokenKey],
                Verifier = querystring[verifierKey]
            };
        }

        private async Task<AccessToken> GetAccessTokenAsync(OAuthVerifier oAuthVerifier)
        {
            if (oAuthVerifier == null)
            {
                throw new ArgumentNullException("oAuthVerifier");
            }

            var oAuthRequest = OAuthRequest.ForAccessToken(PublicApiKey,
                SecretApiKey,
                oAuthVerifier.Token,
                null,
                oAuthVerifier.Verifier);
            oAuthRequest.RequestUrl = "https://api.twitter.com/oauth/access_token";
            var authorizationHeader = oAuthRequest.GetAuthorizationHeader();

            HttpResponseMessage response;
            using (var client = HttpClientFactory.GetHttpClient())
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationHeader);
                response = await client.GetAsync(oAuthRequest.RequestUrl);
            }

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorMessage =
                    string.Format(
                        "Failed to retrieve an HttpStatus-OK while attempting to get a Request Token from Twitter. Status: {0}. Content: {1}.",
                        response.StatusCode,
                        content);
                throw new AuthenticationException(errorMessage);
            }

            return GetAccessTokenFromResponseContent(content);
        }

        private async Task<string> GetUserInformationAsync(AccessToken accessToken)
        {
            if (accessToken == null)
            {
                throw new ArgumentNullException("accessToken");
            }

            var oAuthRequest = OAuthRequest.ForProtectedResource("GET",
                PublicApiKey,
                SecretApiKey,
                accessToken.Token,
                accessToken.Secret);
            oAuthRequest.RequestUrl = "https://api.twitter.com/1.1/account/verify_credentials.json";
            var authorizationHeader = oAuthRequest.GetAuthorizationHeader();

            HttpResponseMessage response;
            using (var client = HttpClientFactory.GetHttpClient())
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationHeader);
                response = await client.GetAsync(oAuthRequest.RequestUrl);
            }

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorMessage =
                    string.Format(
                        "Failed to retrieve an HttpStatus-OK while attempting to get a Request Token from Twitter. Status: {0}. Content: {1}.",
                        response.StatusCode,
                        content);
                throw new AuthenticationException(errorMessage);
            }

            return content;
        }
    }
}