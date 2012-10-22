using System;
using System.Collections.Generic;
using System.Web;
using CuttingEdge.Conditions;

namespace WorldDomination.Web.Authentication
{
    public class AuthenticationService
    {
        private IDictionary<string, IAuthenticationProvider> _authenticationProviders;

        public IDictionary<string, IAuthenticationProvider> AuthenticationProviders
        {
            get { return _authenticationProviders; }
        }

        public void AddProvider(IAuthenticationProvider authenticationProvider)
        {
            if (_authenticationProviders == null)
            {
                _authenticationProviders = new Dictionary<string, IAuthenticationProvider>();
            }

            // Does this provider already exist?
            if (_authenticationProviders.ContainsKey(authenticationProvider.Name))
            {
                throw new InvalidOperationException("Trying to add a " + authenticationProvider.GetType() +
                                                    " provider, but one already exists.");
            }

            _authenticationProviders.Add(authenticationProvider.Name, authenticationProvider);
        }

        public Uri RedirectToAuthenticationProvider(string providerKey, string state)
        {
            Condition.Requires(providerKey).IsNotNullOrEmpty();
            Condition.Requires(state).IsNotNullOrEmpty();

            IAuthenticationProvider authenticationProvider = null;
            if (_authenticationProviders != null)
            {
                _authenticationProviders.TryGetValue(providerKey, out authenticationProvider);
            }

            if (authenticationProvider == null)
            {
                throw new InvalidOperationException("No '" + providerKey + "' provider has been added.");
            }

            return authenticationProvider.RedirectToAuthenticate(state);
        }

        public IAuthenticatedClient CheckCallback(HttpRequestBase httpRequestBase, string state)
        {
            Condition.Requires(httpRequestBase).IsNotNull();
            Condition.Requires(state).IsNotNull();

            if (httpRequestBase.Params == null ||
                !httpRequestBase.Params.HasKeys())
            {
                return new AuthenticatedClient(ProviderType.Unknown)
                       {
                           ErrorInformation =
                               new ErrorInformation(
                               "No request params found - unable to determine from where we authenticated with/against.")
                       };
            }


            IAuthenticatedClient authenticatedClient = null;
            foreach (var provider in _authenticationProviders.Values)
            {
                authenticatedClient = provider.AuthenticateClient(httpRequestBase.Params, state);
                if (authenticatedClient != null)
                {
                    break;
                }
            }

            return authenticatedClient;
        }
    }
}