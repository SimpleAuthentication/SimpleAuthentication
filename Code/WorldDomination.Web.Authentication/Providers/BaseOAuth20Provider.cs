using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using WorldDomination.Web.Authentication.Tracing;

namespace WorldDomination.Web.Authentication.Providers
{
    public abstract class BaseOAuth20Provider : BaseRestFactoryProvider, IAuthenticationProvider
    {
        #region IAuthenticationProvider Members

        public abstract string Name { get; }
        public abstract IAuthenticationServiceSettings DefaultAuthenticationServiceSettings { get; }
        public abstract Uri RedirectToAuthenticate(IAuthenticationServiceSettings authenticationServiceSettings);

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

            TraceSource.TraceInformation("Authorization Code: {0}. Access Token: {1}.",
                                         string.IsNullOrEmpty(authorizationCode)
                                             ? "-no authorization code-"
                                             : authorizationCode,
                                         string.IsNullOrEmpty(accessToken) ? "-no access token-" : accessToken);

            TraceSource.TraceVerbose("Retrieving user information.");
            var userInformation = RetrieveUserInformation(accessToken);
            TraceSource.TraceVerbose("User information retrieved.");

            var authenticatedClient = new AuthenticatedClient(Name.ToLowerInvariant())
            {
                AccessToken = accessToken,
                AccessTokenExpiresOn = DateTime.UtcNow,
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

        protected abstract string RetrieveAccessToken(string code, Uri redirectUri);

        protected abstract UserInformation RetrieveUserInformation(string accessToken);
    }
}
