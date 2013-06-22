using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using RestSharp;
using WorldDomination.Web.Authentication.Tracing;

namespace WorldDomination.Web.Authentication.Providers
{
    public abstract class BaseOAuth20Provider<TAccessTokenResult>
        : BaseRestFactoryProvider, IAuthenticationProvider where TAccessTokenResult : class, new()
    {
        #region IAuthenticationProvider Members

        public abstract string Name { get; }
        public abstract IAuthenticationServiceSettings DefaultAuthenticationServiceSettings { get; }
        public abstract Uri RedirectToAuthenticate(IAuthenticationServiceSettings authenticationServiceSettings);

        protected abstract string DefaultScope { get; }
        protected abstract string ScopeSeparator { get; }
        protected abstract string ScopeKey { get; }

        protected string Scope { get; set; }
        protected string ClientKey { get; set; }
        protected string ClientSecret { get; set; }

        protected BaseOAuth20Provider(ProviderParams providerParams)
        {
            providerParams.Validate();

            ClientKey = providerParams.Key;
            ClientSecret = providerParams.Secret;

            //Set the scope to default to avoid an else statement
            Scope = DefaultScope;

            //If a scope was defined, get the scopes and join them based on the provider specific separator. 
            if (providerParams.GetScopes().Length > 0)
            {
                Scope = string.Join(ScopeSeparator, providerParams.GetScopes());
            }
        }

        public IAuthenticatedClient AuthenticateClient(IAuthenticationServiceSettings authenticationServiceSettings,
                                                       NameValueCollection queryStringParameters)
        {
            if (authenticationServiceSettings == null)
            {
                throw new ArgumentNullException("authenticationServiceSettings");
            }

            TraceSource.TraceVerbose("Retrieving the Authorization Code.");
            var authorizationCode = RetrieveAuthorizationCode(queryStringParameters, authenticationServiceSettings.State);
            TraceSource.TraceVerbose("Authorization Code retrieved.");

            TraceSource.TraceVerbose("Retrieving the Access Token.");
            var accessToken = RetrieveAccessToken(authorizationCode, authenticationServiceSettings.CallBackUri);
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

        protected override TraceSource TraceSource
        {
            get { return TraceManager["WD.Web.Authentication.Providers." + Name]; }
        }

        protected abstract string RetrieveAuthorizationCode(NameValueCollection queryStringParameters,
                                                            string existingState = null);

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
                response.StatusCode != HttpStatusCode.OK)
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

        public string GetScope()
        {
            if (string.IsNullOrWhiteSpace(Scope))
            {
                return string.Empty;
            }

            return ScopeKey + Scope;
        }
    }
}