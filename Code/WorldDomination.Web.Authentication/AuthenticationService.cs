using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using CuttingEdge.Conditions;

namespace WorldDomination.Web.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        #region Implementation of IAuthenticationService

        public IDictionary<string, IAuthenticationProvider> AuthenticationProviders { get; private set; }

        public void AddProvider(IAuthenticationProvider authenticationProvider)
        {
            if (AuthenticationProviders == null)
            {
                AuthenticationProviders = new Dictionary<string, IAuthenticationProvider>();
            }

            // Does this provider already exist?
            if (AuthenticationProviders.ContainsKey(authenticationProvider.Name))
            {
                throw new AuthenticationException("Trying to add a " + authenticationProvider.Name +
                                                  " provider, but one already exists.");
            }

            AuthenticationProviders.Add(authenticationProvider.Name, authenticationProvider);
        }

        public Uri RedirectToAuthenticationProvider(string providerKey, string state)
        {
            Condition.Requires(providerKey).IsNotNullOrEmpty();
            Condition.Requires(state).IsNotNullOrEmpty();

            var authenticationProvider = GetAuthenticationProvider(providerKey);

            return authenticationProvider.RedirectToAuthenticate(state);
        }

        public IAuthenticatedClient CheckCallback(string providerKey, NameValueCollection requestParameters,
                                                  string state)
        {
            Condition.Requires(providerKey).IsNotNullOrEmpty();
            Condition.Requires(requestParameters).IsNotNull();
            Condition.Requires(state).IsNotNullOrEmpty();

            var authenticationProvider = GetAuthenticationProvider(providerKey);
            return authenticationProvider.AuthenticateClient(requestParameters, state);
        }

        #endregion

        private IAuthenticationProvider GetAuthenticationProvider(string providerKey)
        {
            IAuthenticationProvider authenticationProvider = null;
            if (AuthenticationProviders != null)
            {
                AuthenticationProviders.TryGetValue(providerKey, out authenticationProvider);
            }

            if (authenticationProvider == null)
            {
                throw new AuthenticationException("No '" + providerKey + "' provider has been added.");
            }
            return authenticationProvider;
        }
    }
}