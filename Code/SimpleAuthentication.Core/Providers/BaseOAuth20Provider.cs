using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using RestSharp;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Tracing;

namespace SimpleAuthentication.Core.Providers
{
    public abstract class BaseOAuth20Provider<TAccessTokenResult>
        : BaseProvider, IPublicPrivateKeyProvider, IScopedProvider where TAccessTokenResult : class, new()
    {
        protected BaseOAuth20Provider(string name, ProviderParams providerParams)
            : base(name, "OAuth 2.0")
        {
            providerParams.Validate();

            PublicApiKey = providerParams.PublicApiKey;
            SecretApiKey = providerParams.SecretApiKey;
            Scopes = providerParams.Scopes;

            RestClientFactory = new RestClientFactory();
        }

        public IRestClientFactory RestClientFactory { get; set; }

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

        public override RedirectToAuthenticateSettings RedirectToAuthenticate(Uri callbackUri)
        {
            if (callbackUri == null)
            {
                throw new ArgumentNullException("callbackUri");
            }

            // Validations.
            if (AuthenticateRedirectionUrl == null)
            {
                throw new AuthenticationException(
                    "AuthenticationRedirectUrl has no value. Please set the authentication Url location to redirect to.");
            }

            if (string.IsNullOrEmpty(PublicApiKey))
            {
                throw new AuthenticationException("PublicApiKey has no value. Please set this value.");
            }

            // Generate some state which will be used in the redirection uri and used for CSRF checks.
            var state = Guid.NewGuid().ToString();

            // Now the redirection uri.
            var redirectUri = string.Format("{0}?{1}", AuthenticateRedirectionUrl.AbsoluteUri,
                                            CreateRedirectionQuerystringParameters(callbackUri, state));

            TraceSource.TraceInformation("Google redirection uri: {0}.", redirectUri);

            return new RedirectToAuthenticateSettings
            {
                RedirectUri = new Uri(redirectUri),
                State = state
            };
        }

        public override IAuthenticatedClient AuthenticateClient(NameValueCollection queryStringParameters,
                                                                string state,
                                                                Uri callbackUri)
        {
            #region Parameter checks

            if (queryStringParameters == null ||
                queryStringParameters.Count <= 0)
            {
                throw new ArgumentNullException("queryStringParameters");
            }

            if (string.IsNullOrEmpty(state))
            {
                throw new ArgumentNullException("state");
            }

            if (callbackUri == null)
            {
                throw new ArgumentNullException("callbackUri");
            }

            #endregion

            TraceSource.TraceInformation("Callback parameters: " +
                string.Join("&", queryStringParameters.AllKeys.Select(key => key + "=" + queryStringParameters[key]).ToArray()));

            #region Cross Site Request Forgery checks -> state == state?

            // Start with the Cross Site Request Forgery check.
            var callbackState = queryStringParameters[StateKey];
            if (string.IsNullOrEmpty(callbackState))
            {
                var errorMessage =
                    "The callback querystring doesn't include a state key/value parameter. We need one of these so we can to a CSRF check. Please check why the request url from the provider is missing the parameter: " +
                    StateKey + ". eg. &state=something...";
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            #endregion

            TraceSource.TraceVerbose("Retrieving the Authorization Code.");
            var authorizationCode = RetrieveAuthorizationCode(queryStringParameters);
            TraceSource.TraceVerbose("Authorization Code retrieved.");

            TraceSource.TraceVerbose("Retrieving the Access Token.");
            var accessToken = RetrieveAccessToken(authorizationCode, callbackUri);
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
            var userInformation = RetrieveUserInformation(accessToken);
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

            return string.Format("client_id={0}&redirect_uri={1}&response_type=code{2}{3}",
                    PublicApiKey, callbackUri.AbsoluteUri, GetScope(), GetQuerystringState(state));
        }

        protected virtual string RetrieveAuthorizationCode(NameValueCollection queryStringParameters)
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
               Google returns an authorization code to your application if the user grants your application the permissions it requested. 
               The authorization code is returned to your application in the query string parameter code. If the state parameter was included in the request,
               then it is also included in the response. */
            var code = queryStringParameters["code"];
            var error = queryStringParameters["error"];

            // First check for any errors.
            if (!string.IsNullOrEmpty(error))
            {
                var errorMessage =
                    string.Format("Failed to retrieve an authorization code from {0}. The error provided is: {1}" +
                                  Name, error);
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            // Otherwise, we need a code.
            if (string.IsNullOrEmpty(code))
            {
                string errorMessage = string.Format(
                    "No code parameter provided in the response query string from {0}.", Name);
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return code;
        }

        protected abstract IRestResponse<TAccessTokenResult> ExecuteRetrieveAccessToken(string authorizationCode,
                                                                                        Uri redirectUri);

        protected abstract AccessToken MapAccessTokenResultToAccessToken(TAccessTokenResult accessTokenResult);

        protected AccessToken RetrieveAccessToken(string authorizationCode, Uri redirectUri)
        {
            if (string.IsNullOrEmpty(authorizationCode))
            {
                throw new ArgumentNullException("authorizationCode");
            }

            if (redirectUri == null ||
                string.IsNullOrEmpty(redirectUri.AbsoluteUri))
            {
                throw new ArgumentNullException("redirectUri");
            }

            IRestResponse<TAccessTokenResult> response;

            try
            {
                response = ExecuteRetrieveAccessToken(authorizationCode, redirectUri);
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

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK ||
                response.ErrorException != null)
            {
                var errorMessage = string.Format(
                    "Failed to obtain an Access Token from {0} OR the the response was not an HTTP Status 200 OK. Response Status: {1}. Response Description: {2}. Error Content: {3}. Error Message: {4}.",
                    string.IsNullOrEmpty(Name) ? "--no name--" : Name,
                    response == null ? "-- null response --" : response.StatusCode.ToString(),
                    response == null ? string.Empty : response.StatusDescription,
                    response == null ? string.Empty : response.Content,
                    response == null
                        ? string.Empty
                        : response.ErrorException == null
                              ? "--no error exception--"
                              : response.ErrorException.RecursiveErrorMessages());

                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return MapAccessTokenResultToAccessToken(response.Data);
        }

        protected abstract UserInformation RetrieveUserInformation(AccessToken accessToken);

        protected string GetScope()
        {
            return string.Format("&{0}={1}",
                                 ScopeKey,
                                 String.Join(ScopeSeparator, Scopes == null ||
                                                             !Scopes.Any()
                                                                 ? DefaultScopes
                                                                 : Scopes));
        }
    }
}