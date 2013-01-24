using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using CuttingEdge.Conditions;
using RestSharp;
using WorldDomination.Web.Authentication.Config;
using WorldDomination.Web.Authentication.Facebook;
using WorldDomination.Web.Authentication.Google;
using WorldDomination.Web.Authentication.Twitter;

namespace WorldDomination.Web.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        public AuthenticationService()
        {
        }

        public AuthenticationService(ProviderConfiguration providerConfiguration,
                                     IList<string> scope = null, IRestClient restClient = null)
        {
            Condition.Requires(providerConfiguration).IsNotNull();
            Condition.Requires(providerConfiguration.Providers).IsNotNull();

            foreach (ProviderKey provider in providerConfiguration.Providers)
            {
                IAuthenticationProvider authenticationProvider;
                switch (provider.Name)
                {
                    case ProviderType.Facebook:
                        authenticationProvider = new FacebookProvider(provider, scope, restClient);
                        break;
                    case ProviderType.Google:
                        authenticationProvider = new GoogleProvider(provider, scope, restClient);
                        break;
                    case ProviderType.Twitter:
                        authenticationProvider = new TwitterProvider(provider, restClient);
                        break;
                    default:
                        throw new ApplicationException(
                            "Unhandled ProviderType found - unable to know which Provider Type to create.");
                }

                AddProvider(authenticationProvider);
            }
        }

        #region Implementation of IAuthenticationService

        public IDictionary<string, IAuthenticationProvider> AuthenticationProviders { get; private set; }

        public IEnumerable<IAuthenticationProvider> Providers
        {
            get
            {
                return AuthenticationProviders != null && AuthenticationProviders.Count > 0
                           ? AuthenticationProviders.Values
                           : null;
            }
        }

        public void AddProvider(IAuthenticationProvider authenticationProvider)
        {
            if (AuthenticationProviders == null)
            {
                AuthenticationProviders = new Dictionary<string, IAuthenticationProvider>();
            }

            var providerName = authenticationProvider.Name.ToLowerInvariant();

            // Does this provider already exist?
            if (AuthenticationProviders.ContainsKey(providerName))
            {
                throw new AuthenticationException("Trying to add a " + providerName +
                                                  " provider, but one already exists.");
            }

            AuthenticationProviders.Add(providerName, authenticationProvider);
        }

        public Uri RedirectToAuthenticationProvider(string providerKey, Uri callBackUri = null)
        {
            Condition.Requires(providerKey).IsNotNullOrEmpty();

            // Determine the provider.
            var authenticationProvider = GetAuthenticationProvider(providerKey);

            // Retrieve the default settings for this provider.
            var authenticationServiceSettings = authenticationProvider.DefaultAuthenticationServiceSettings;

            // Have we provided an specific callBack uri?
            if (callBackUri != null)
            {
                authenticationServiceSettings.CallBackUri = callBackUri;
            }

            return authenticationProvider.RedirectToAuthenticate(authenticationServiceSettings);
        }

        public Uri RedirectToAuthenticationProvider(IAuthenticationServiceSettings authenticationServiceSettings)
        {
            Condition.Requires(authenticationServiceSettings).IsNotNull();
            Condition.Requires(authenticationServiceSettings.ProviderKey).IsNotNullOrEmpty();
            Condition.Requires(authenticationServiceSettings.ProviderType).IsNotEqualTo(ProviderType.Unknown);
            Condition.Requires(authenticationServiceSettings.CallBackUri).IsNotNull();
            Condition.Requires(authenticationServiceSettings.CallBackUri.AbsoluteUri).IsNotNullOrEmpty();

            var authenticationProvider = GetAuthenticationProvider(authenticationServiceSettings.ProviderKey);

            return authenticationProvider.RedirectToAuthenticate(authenticationServiceSettings);
        }

        public IAuthenticatedClient GetAuthenticatedClient(string providerKey, NameValueCollection requestParameters,
                                                  string state = null)
        {
            Condition.Requires(providerKey).IsNotNullOrEmpty();
            Condition.Requires(requestParameters).IsNotNull();

            var authenticationProvider = GetAuthenticationProvider(providerKey);
            return authenticationProvider.AuthenticateClient(requestParameters, state);
        }

        public IAuthenticationServiceSettings GetAuthenticateServiceSettings(string providerKey)
        {
            Condition.Requires(providerKey).IsNotNullOrEmpty();

            // Convert the string to an enumeration.
            ProviderType providerType;
            if (!Enum.TryParse(providerKey, true, out providerType))
            {
                return null;
            }

            switch (providerType)
            {
                case ProviderType.Facebook:
                    return new FacebookAuthenticationServiceSettings();
                case ProviderType.Google:
                    return new GoogleAuthenticationServiceSettings();
                case ProviderType.Twitter:
                    return new TwitterAuthenticationServiceSettings();
                default:
                    throw new AuthenticationException(
                        "Unhandled provider type while trying to determine which AuthenticationServiceSettings to instanciate.");
            }
        }

        #endregion

        private IAuthenticationProvider GetAuthenticationProvider(string providerKey)
        {
            IAuthenticationProvider authenticationProvider = null;
            if (AuthenticationProviders != null)
            {
                AuthenticationProviders.TryGetValue(providerKey.ToLowerInvariant(), out authenticationProvider);
            }

            if (authenticationProvider == null)
            {
                throw new AuthenticationException("No '" + providerKey + "' provider has been added.");
            }
            return authenticationProvider;
        }
    }
}