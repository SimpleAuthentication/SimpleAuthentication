using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SimpleAuthentication.Config;
using SimpleAuthentication.Providers;
using SimpleAuthentication.Tracing;

namespace SimpleAuthentication
{
    public class AuthenticationProviderFactory
    {
        public AuthenticationProviderFactory()
        {
            TraceManager = new Lazy<ITraceManager>(() => new TraceManager()).Value;

            AuthenticationProviders =
                new Lazy<IDictionary<string, IAuthenticationProvider>>(
                    () => new Dictionary<string, IAuthenticationProvider>()).Value;

            Initialize();
        }

        #region IAuthenticationFactory Implementation

        public IDictionary<string, IAuthenticationProvider> AuthenticationProviders { get; private set; }

        public ITraceManager TraceManager { set; private get; }

        public void AddProvider(IAuthenticationProvider provider, bool replaceExisting = true)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            var key = provider.Name.ToLower();

            if (AuthenticationProviders.ContainsKey(key) &&
                !replaceExisting)
            {
                throw new InvalidOperationException(
                        string.Format(
                            "The provider '{0}' already exists and cannot be overridden, either set `replaceExisting` to `true`, or remove the provider first.",
                            provider.Name));
            }

            AuthenticationProviders[key] = provider;
        }

        public void RemoveProvider(string providerName)
        {
            AuthenticationProviders.Remove(providerName);
        }

        #endregion

        private TraceSource TraceSource
        {
            get { return TraceManager["WD.Web.Authentication.AuthenticationProviderFactory"]; }
        }

        private void Initialize()
        {
            var discoveredProviders = MefHelpers.GetExportedTypes<IAuthenticationProvider>();

            //TODO: Try configure from appSettings

            //Try configure from custom config section.
            var providerConfig = ProviderConfigHelper.UseConfig();
            if (providerConfig != null)
            {
                SetupCustomConfigProviders(discoveredProviders, providerConfig);
            }
        }

        private void SetupCustomConfigProviders(IList<Type> discoveredProviders, ProviderConfiguration providerConfig)
        {
            if (providerConfig == null)
            {
                throw new ArgumentNullException("providerConfig");
            }

            if (providerConfig.Providers == null)
            {
                throw new ArgumentException("providerConfiguration.Providers");
            }

            foreach (ProviderKey provider in providerConfig.Providers)
            {
                var discoveredProvider = DiscoverProvider(discoveredProviders, provider);

                AddProvider(discoveredProvider, false);
            }
        }

        private IAuthenticationProvider DiscoverProvider(IEnumerable<Type> discoveredProviders, ProviderKey providerKey)
        {
            if (discoveredProviders == null)
            {
                throw new ArgumentNullException("discoveredProviders");
            }

            if (providerKey == null)
            {
                throw new ArgumentNullException("providerKey");
            }

            var name = providerKey.Name.ToLowerInvariant();

            var provider = discoveredProviders.SingleOrDefault(x => x.Name.ToLowerInvariant().StartsWith(name));

            if (provider == null)
            {
                var errorMessage =
                    string.Format(
                        "Unable to find the provider [{0}]. Is there a provider dll available? Is there a typo in the provider name? Solution suggestions: Check to make sure the correct dll's are in the 'bin' directory and/or check the name to make sure there's no typo's in there. Example: If you're trying include the GitHub provider, make sure the name is 'github' (any case) and that the ExtraProviders dll exists in the 'bin' directory.",
                        name);
                TraceSource.TraceError(errorMessage);
                throw new ApplicationException(errorMessage);
            }

            IAuthenticationProvider authenticationProvider = null;

            // Make sure we have a provider with the correct constructor parameters.
            // How? If a person creates their own provider and doesn't offer a constructor
            // that has a sigle ProviderParams, then we're stuffed. So we need to help them.
            if (provider.GetConstructor(new[] {typeof (ProviderParams)}) != null)
            {
                var parameters = new ProviderParams
                {
                    PublicApiKey = providerKey.Key,
                    SecretApiKey = providerKey.Secret,
                    Scopes = string.IsNullOrEmpty(providerKey.Scope)
                                 ? null
                                 : providerKey.Scope.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                };
                authenticationProvider = Activator.CreateInstance(provider, parameters) as IAuthenticationProvider;
            }

            if (authenticationProvider == null)
            {
                // We didn't find a proper constructor for the provider class we wish to instantiate.
                var errorMessage =
                    string.Format(
                        "The type {0} doesn't have the proper constructor. It requires a constructor that only accepts 1 argument of type ProviderParams. Eg. public MyProvider(ProviderParams providerParams){{ .. }}.",
                        provider.FullName);
                TraceSource.TraceError(errorMessage);
                throw new ApplicationException(errorMessage);
            }

            return authenticationProvider;
        }
    }
}