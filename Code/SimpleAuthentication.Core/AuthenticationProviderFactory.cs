namespace SimpleAuthentication.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Config;
    using Tracing;

    public class AuthenticationProviderFactory
    {
        private static readonly Lazy<IDictionary<string, IAuthenticationProvider>> Providers =
            new Lazy<IDictionary<string, IAuthenticationProvider>>(
                () =>
                {
                    var authenticationProviders = new Dictionary<string, IAuthenticationProvider>();
                    Initialize(authenticationProviders);
                    return authenticationProviders;
                });

        public AuthenticationProviderFactory()
        {
            TraceManager = new Lazy<ITraceManager>(() => new TraceManager()).Value;
        }

        public IDictionary<string, IAuthenticationProvider> AuthenticationProviders
        {
            get { return Providers.Value; }
        }

        public ITraceManager TraceManager { set; private get; }

        public void AddProvider(IAuthenticationProvider provider, bool replaceExisting = true)
        {
            AddProviderToDictionary(AuthenticationProviders, provider, replaceExisting);
        }

        public void RemoveProvider(string providerName)
        {
            RemoveProviderFromDictionary(providerName, AuthenticationProviders);
        }

        private static void Initialize(IDictionary<string, IAuthenticationProvider> authenticationProviders)
        {
            authenticationProviders.ThrowIfNull("authenticationProviders");

            var discoveredProviders = ReflectionHelpers.FindAllTypesOf<IAuthenticationProvider>();
            if (discoveredProviders == null)
            {
                return;
            }

            // Try configure from custom config section.
            var providerConfig = ProviderConfigHelper.UseConfig();
            if (providerConfig != null)
            {
                SetupCustomConfigProviders(authenticationProviders, discoveredProviders, providerConfig);
            }

            var appSettings = ProviderConfigHelper.UseAppSettings();
            if (appSettings != null && appSettings.Any())
            {
                SetupAppSettingsConfigProviders(authenticationProviders, discoveredProviders, appSettings);
            }
        }

        private static void SetupAppSettingsConfigProviders(
            IDictionary<string, IAuthenticationProvider> authenticationProviders,
            IList<Type> discoveredProviders,
            IList<AppSettingsParser.ProviderKey> appSettings)
        {
            authenticationProviders.ThrowIfNull("authenticationProviders");
            discoveredProviders.ThrowIfNull("discoveredProviders");
            appSettings.ThrowIfNull("appSettings");

            foreach (var provider in appSettings)
            {
                var discoveredProvider = DiscoverProvider(discoveredProviders, provider);

                AddProviderToDictionary(authenticationProviders, discoveredProvider, false);
            }
        }

        private static void SetupCustomConfigProviders(
            IDictionary<string, IAuthenticationProvider> authenticationProviders,
            IList<Type> discoveredProviders,
            ProviderConfiguration providerConfig)
        {
            authenticationProviders.ThrowIfNull("authenticationProviders");
            discoveredProviders.ThrowIfNull("discoveredProviders");
            providerConfig.ThrowIfNull("providerConfig");
            providerConfig.Providers.ThrowIfNull("providerConfig.Providers");

            foreach (ProviderKey provider in providerConfig.Providers)
            {
                var discoveredProvider = DiscoverProvider(discoveredProviders, provider);

                AddProviderToDictionary(authenticationProviders, discoveredProvider, false);
            }
        }

        public static IAuthenticationProvider DiscoverProvider<T>(IList<Type> discoveredProviders,
            string name,
            Func<T> providerParamsFunc) where T : ProviderParams
        {
            discoveredProviders.ThrowIfNull("discoveredProviders");

            var provider = discoveredProviders.SingleOrDefault(x => x.Name.ToLowerInvariant().StartsWith(name));

            if (provider == null)
            {
                var errorMessage =
                    string.Format(
                        "Unable to find the provider [{0}]. Is there a provider dll available? Is there a typo in the provider name? Solution suggestions: Check to make sure the correct dll's are in the 'bin' directory and/or check the name to make sure there's no typo's in there. Example: If you're trying include the GitHub provider, make sure the name is 'github' (any case) and that the ExtraProviders dll exists in the 'bin' directory or make sure you've downloaded the package via NuGet -> install-package SimpleAuthentication.ExtraProviders.",
                        name);
                
                throw new ApplicationException(errorMessage);
            }

            IAuthenticationProvider authenticationProvider = null;

            // Make sure we have a provider with the correct constructor parameters.
            // How? If a person creates their own provider and doesn't offer a constructor
            // that has a sigle ProviderParams, then we're stuffed. So we need to help them.
            if (provider.GetConstructor(new[] {typeof (ProviderParams)}) != null)
            {
                var parameters = providerParamsFunc();

                authenticationProvider = Activator.CreateInstance(provider, parameters) as IAuthenticationProvider;
            }

            if (authenticationProvider == null)
            {
                // We didn't find a proper constructor for the provider class we wish to instantiate.
                var errorMessage =
                    string.Format(
                        "The type {0} doesn't have the proper constructor. It requires a constructor that only accepts 1 argument of type ProviderParams. Eg. public MyProvider(ProviderParams providerParams){{ .. }}.",
                        provider.FullName);

                throw new ApplicationException(errorMessage);
            }

            return authenticationProvider;
        }

        /// <summary>
        /// Method used when using appSettings section
        /// </summary>
        private static IAuthenticationProvider DiscoverProvider(IList<Type> discoveredProviders,
            AppSettingsParser.ProviderKey providerKey)
        {
            providerKey.ThrowIfNull("providerKey");

            var name = providerKey.ProviderName.ToLowerInvariant();

            return DiscoverProvider(discoveredProviders, name, () => new ProviderParams
            {
                PublicApiKey = providerKey.Key,
                SecretApiKey = providerKey.Secret,
                Scopes = providerKey.Scope.ScopesToCollection()
            });
        }

        /// <summary>
        /// Method used when using custom config section
        /// </summary>
        private static IAuthenticationProvider DiscoverProvider(IList<Type> discoveredProviders,
            ProviderKey providerKey)
        {
            providerKey.ThrowIfNull("providerKey");

            var name = providerKey.Name.ToLowerInvariant();

            return DiscoverProvider(discoveredProviders, name, () => new ProviderParams
            {
                PublicApiKey = providerKey.Key,
                SecretApiKey = providerKey.Secret,
                Scopes = providerKey.Scope.ScopesToCollection()
            });
        }

        private static void AddProviderToDictionary(
            IDictionary<string, IAuthenticationProvider> authenticationProviders,
            IAuthenticationProvider provider,
            bool replaceExisting = true)
        {
            provider.ThrowIfNull("provider");
            authenticationProviders.ThrowIfNull("authenticationProviders");

            var key = provider.Name.ToLower();

            if (authenticationProviders.ContainsKey(key) &&
                !replaceExisting)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "The provider '{0}' already exists and cannot be overridden, either set `replaceExisting` to `true`, or remove the provider first.",
                        provider.Name));
            }

            authenticationProviders[key] = provider;
        }

        private static void RemoveProviderFromDictionary(string providerName,
            IDictionary<string, IAuthenticationProvider> authenticationProviders)
        {
            providerName.ThrowIfNull("providerName");
            authenticationProviders.ThrowIfNull("authenticationProviders");

            authenticationProviders.Remove(providerName);
        }
    }

    public static class Extensions
    {
        public static void ThrowIfNull(this object thing, string parameter)
        {
            if (thing == null)
            {
                throw new ArgumentNullException(parameter);
            }
        }

        public static ICollection<string> ScopesToCollection(this string scopes)
        {
            if (string.IsNullOrEmpty(scopes))
            {
                return null;
            }

            return scopes.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}