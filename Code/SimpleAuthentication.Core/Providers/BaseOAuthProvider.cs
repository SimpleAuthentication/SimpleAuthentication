using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Tracing;

namespace SimpleAuthentication.Core.Providers
{
    public abstract class BaseOAuthProvider<TAccessTokenResult> : BaseProvider, IPublicPrivateKeyProvider, IScopedProvider
        where TAccessTokenResult : class, new()
    {
        protected BaseOAuthProvider(string name, ProviderParams providerParams, string description = null) 
            : base(name, description)
        {
            providerParams.Validate();

            PublicApiKey = providerParams.PublicApiKey;
            SecretApiKey = providerParams.SecretApiKey;
            Scopes = providerParams.Scopes;
        }

        #region IPublicPrivateKeyProvider Implementation

        public string PublicApiKey { get; protected set; }
        public string SecretApiKey { get; protected set; }

        #endregion

        #region IScopedProvider Implementation

        public abstract IEnumerable<string> DefaultScopes { get; }

        public virtual string ScopeSeparator
        {
            get { return " "; }
        }

        public virtual string ScopeKey
        {
            get { return "scope"; }
        }

        public IEnumerable<string> Scopes { get; set; }

        #endregion

        #region IAuthenticationProvider Members

        public override RedirectToAuthenticateSettings GetRedirectToAuthenticateSettings(Uri callbackUrl)
        {
            if (callbackUrl == null)
            {
                throw new ArgumentNullException("callbackUrl");
            }

            // Validations.
            if (AuthenticateRedirectionUrl == null)
            {
                throw new AuthenticationException(
                    "AuthenticationRedirectUrl has no value. Please set the authentication Url location to redirect to.");
            }

            if (string.IsNullOrWhiteSpace(PublicApiKey))
            {
                throw new AuthenticationException("PublicApiKey has no value. Please set this value.");
            }

            // Generate some state which will be used in the redirection uri and used for CSRF checks.
            var state = Guid.NewGuid().ToString();

            // Now the redirection uri.
            var redirectUri = string.Format("{0}?{1}",
                AuthenticateRedirectionUrl.AbsoluteUri,
                CreateRedirectionQuerystringParameters(callbackUrl, state));

            TraceSource.TraceInformation("{0} redirection uri: {1}.",
                Name,
                redirectUri);

            return new RedirectToAuthenticateSettings
            {
                RedirectUri = new Uri(redirectUri),
                State = state
            };
        }

        public override async Task<IAuthenticatedClient> AuthenticateClientAsync(
            NameValueCollection queryStringParameters,
            string state,
            Uri callbackUri)
        {
            #region Parameter checks

            if (queryStringParameters == null ||
                queryStringParameters.Count <= 0)
            {
                throw new ArgumentNullException("queryStringParameters");
            }

            if (string.IsNullOrWhiteSpace(state))
            {
                throw new ArgumentNullException("state");
            }

            if (callbackUri == null)
            {
                throw new ArgumentNullException("callbackUri");
            }

            #endregion

            TraceSource.TraceInformation("Callback parameters: " +
                                         string.Join("&",
                                             queryStringParameters.AllKeys.Select(
                                                 key => key + "=" + queryStringParameters[key]).ToArray()));

            #region Cross Site Request Forgery checks -> state == state?

            // Start with the Cross Site Request Forgery check.
            var callbackState = queryStringParameters[StateKey];
            if (string.IsNullOrWhiteSpace(callbackState))
            {
                var errorMessage =
                    "The callback querystring doesn't include a state key/value parameter. We need one of these so we can to a CSRF check. Please check why the request url from the provider is missing the parameter: " +
                    StateKey + ". eg. &state=something...";
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            #endregion

            TraceSource.TraceVerbose("Retrieving the Authorization Code.");
            var authorizationCode = GetAuthorizationCodeFromQueryString(queryStringParameters);
            TraceSource.TraceVerbose("Authorization Code retrieved.");

            TraceSource.TraceVerbose("Retrieving the Access Token.");
            var accessToken = await GetAccessTokenAsync(authorizationCode, callbackUri);
            TraceSource.TraceVerbose("Access Token retrieved.");

            if (accessToken == null)
            {
                const string errorMessage = "No access token retrieved from provider. Unable to continue.";
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            TraceSource.TraceInformation("Authorization Code: {0}. {1}",
                string.IsNullOrEmpty(authorizationCode)
                    ? "-no authorization code-"
                    : authorizationCode,
                accessToken.ToString());

            TraceSource.TraceVerbose("Retrieving user information.");
            var userInformation = await RetrieveUserInformationAsync(accessToken);
            TraceSource.TraceVerbose("User information retrieved.");

            var authenticatedClient = new AuthenticatedClient(Name.ToLowerInvariant())
            {
                AccessToken = accessToken,
                UserInformation = userInformation
            };

            TraceSource.TraceInformation(authenticatedClient.ToString());

            return authenticatedClient;
        }

        #endregion

        private string ScopesJoined
        {
            get
            {
                return string.Join(ScopeSeparator, Scopes == null ||
                                                   !Scopes.Any()
                    ? DefaultScopes
                    : Scopes);
            }
        }

        /// <summary>
        /// Create the provider authentication parameters which make up the end part of the redirection url. eg. state=aaa&foo=bar, etc.
        /// </summary>
        protected virtual string CreateRedirectionQuerystringParameters(Uri callbackUri, string state)
        {
            if (callbackUri == null)
            {
                throw new ArgumentNullException("callbackUri");
            }

            if (string.IsNullOrEmpty(state))
            {
                throw new ArgumentNullException("state");
            }

            var queryString = new StringBuilder();
            queryString.AppendFormat("client_id={0}&redirect_uri={1}&response_type=code",
                PublicApiKey, 
                callbackUri.AbsoluteUri);

            var scopesParameter = GetScopes();
            if (!string.IsNullOrWhiteSpace(scopesParameter))
            {
                queryString.AppendFormat("&{0}", scopesParameter);
            }

            var stateParameter = GetQuerystringState(state);
            if (!string.IsNullOrWhiteSpace(stateParameter))
            {
                queryString.AppendFormat("&{0}", stateParameter);
            }

            return queryString.ToString();
        }

        protected virtual string GetAuthorizationCodeFromQueryString(NameValueCollection queryStringParameters)
        {
            if (queryStringParameters == null)
            {
                throw new ArgumentNullException("queryStringParameters");
            }

            if (queryStringParameters.Count <= 0)
            {
                throw new ArgumentOutOfRangeException("queryStringParameters");
            }

            /* Documentation:
               Providers returns an authorization code to your application if the user grants your application the permissions it requested. 
               The authorization code is returned to your application in the query string parameter code. If the state parameter was included in the request,
               then it is also included in the response. */
            var code = queryStringParameters["code"];
            var error = queryStringParameters["error"];

            // First check for any errors.
            if (!string.IsNullOrWhiteSpace(error))
            {
                var errorMessage =
                    string.Format("Failed to retrieve an authorization code from {0}. The error provided is: {1}",
                        Name,
                        error);
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            // Otherwise, we need a code.
            if (string.IsNullOrWhiteSpace(code))
            {
                string errorMessage = string.Format(
                    "No code parameter provided in the response query string from {0}.", Name);
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return code;
        }

        protected abstract Task<TAccessTokenResult> GetAccessTokenFromProviderAsync(string authorizationCode,
            Uri redirectUrl);

        protected abstract AccessToken MapAccessTokenResultToAccessToken(TAccessTokenResult accessTokenResult);

        protected async Task<AccessToken> GetAccessTokenAsync(string authorizationCode, Uri redirectUrl)
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

            TAccessTokenResult accessTokenResult;

            try
            {
                accessTokenResult = await GetAccessTokenFromProviderAsync(authorizationCode, redirectUrl);
            }
            catch (Exception exception)
            {
                var authentictionException =
                    new AuthenticationException(string.Format("Failed to retrieve an Access Token from {0}.",
                        Name), exception);
                var errorMessage = string.Format("{0}", authentictionException.RecursiveErrorMessages());
                TraceSource.TraceError(errorMessage);
                throw authentictionException;
            }

            return MapAccessTokenResultToAccessToken(accessTokenResult);
        }

        protected abstract Task<UserInformation> RetrieveUserInformationAsync(AccessToken accessToken);

        protected string GetScopes()
        {
            return string.Format("{0}={1}", ScopeKey, ScopesJoined);
        }

        protected KeyValuePair<string, string> GetScopeAsKeyValuePair()
        {
            return new KeyValuePair<string, string>(ScopeKey, ScopesJoined);
        }
    }
}