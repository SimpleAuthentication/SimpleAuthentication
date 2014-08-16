using System;
using System.Collections.Generic;
using System.Linq;
using SimpleAuthentication.Core.Config;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers;
using SimpleAuthentication.Core.Tracing;

namespace SimpleAuthentication.Core
{
    public class AuthenticationProviderFactory
    {
        public static Lazy<Configuration> Configuration;

        private static readonly Lazy<IDictionary<string, IAuthenticationProvider>> Providers =
            new Lazy<IDictionary<string, IAuthenticationProvider>>(Initialize);

        public AuthenticationProviderFactory(IConfigService configService)
        {
            configService.ThrowIfNull("configService");

            TraceManager = new Lazy<ITraceManager>(() => new TraceManager()).Value;
            Configuration = new Lazy<Configuration>(configService.GetConfiguration);
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

        public void RemoveProvider<T>()
        {
            var providerName = typeof (T).Name.ToLowerInvariant();

            RemoveProviderFromDictionary(providerName, AuthenticationProviders);
        }

        public static IAuthenticationProvider AvailableProvider<T>(IList<Type> discoveredProviders,
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

        private static IDictionary<string, IAuthenticationProvider> Initialize()
        {
            var authenticationProviders = new Dictionary<string, IAuthenticationProvider>();
           
            // TODO: Replace this with Phillip Hayden's code for *smart scanning*.
            var availableProviders = new List<Type>
            {
                typeof (GoogleProvider),
                typeof (FacebookProvider),
                typeof (TwitterProvider),
                typeof (WindowsLiveProvider),
                typeof (FakeProvider)
            };

            if (Configuration != null &&
                Configuration.Value.Providers != null &&
                Configuration.Value.Providers.Any())
            {
                // Map the available providers to the config settings provided.
                SetupConfigurationProviders(authenticationProviders,
                    availableProviders,
                    Configuration.Value.Providers);
            }

            if (authenticationProviders.Any())
            {
                return authenticationProviders;
            }

            // No providers where found. Unable to go anywhere!
            const string errorMessage =
                "No Authentication Provider config settings where found. As such, we'll never be able to authenticate successfully against another service. How to fix this: add at least one Authentication Provider configuration data into your config file's <appSettings> section (recommended and easiest answer) [eg. <add key=\"sa.Google\" value=\"key:587140099194.apps.googleusercontent.com;secret:npk1_gx-gqJmLiJRPFooxCEY\"/> or add a custom config section to your .config file (looks a bit more pro, but is also a bit more complex to setup). For more info (especially the convention rules for the appSettings key/value> please refer to: ";
            throw new AuthenticationException(errorMessage);
        }

        private static void SetupConfigurationProviders(
            IDictionary<string, IAuthenticationProvider> authenticationProviders,
            IList<Type> availableProviders,
            IEnumerable<Provider> providers)
        {
            authenticationProviders.ThrowIfNull("authenticationProviders");
            availableProviders.ThrowIfNull("discoveredProviders");
            providers.ThrowIfNull("appSettings");

            foreach (var provider in providers)
            {
                var discoveredProvider = AvailableProvider(availableProviders, provider);

                AddProviderToDictionary(authenticationProviders, discoveredProvider, false);
            }
        }

        private static IAuthenticationProvider AvailableProvider(IList<Type> discoveredProviders,
            Provider provider)
        {
            provider.ThrowIfNull("providerKey");

            var name = provider.Name.ToLowerInvariant();

            return AvailableProvider(discoveredProviders, name, () =>
                new ProviderParams(
                    provider.Key,
                    provider.Secret,
                    provider.Scopes.ScopesToCollection()));
        }

        private static void AddProviderToDictionary(
            IDictionary<string, IAuthenticationProvider> authenticationProviders,
            IAuthenticationProvider provider,
            bool replaceExisting = true)
        {
            provider.ThrowIfNull("provider");
            authenticationProviders.ThrowIfNull("authenticationProviders");

            var key = provider.Name.ToLower();

            if (authenticationProviders.ContainsKey(key) && !replaceExisting)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "The provider '{0}' already exists and cannot be overridden, either set `replaceExisting` to `true`, or remove the provider first.",
                        provider.Name));
            }

            authenticationProviders[key] = provider;
        }

        private static void RemoveProviderFromDictionary(string providerName,
            ICollection<KeyValuePair<string, IAuthenticationProvider>> authenticationProviders)
        {
            providerName.ThrowIfNull("providerName");
            authenticationProviders.ThrowIfNull("authenticationProviders");

            var providersToRemove =
                authenticationProviders.Where(x => providerName.StartsWith(x.Key.ToLowerInvariant()))
                    .ToList();

            foreach (var providerPair in providersToRemove)
            {
                authenticationProviders.Remove(providerPair);
            }
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