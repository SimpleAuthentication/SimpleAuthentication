using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using WorldDomination.Web.Authentication.Config;
using WorldDomination.Web.Authentication.Exceptions;
using WorldDomination.Web.Authentication.Providers.Facebook;
using WorldDomination.Web.Authentication.Providers.Google;
using WorldDomination.Web.Authentication.Providers.Twitter;
using WorldDomination.Web.Authentication.Tracing;

namespace WorldDomination.Web.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private static ConcurrentDictionary<string, IAuthenticationProvider> _configuredProviders;

        public AuthenticationService()
        {
            TraceManager = new Lazy<ITraceManager>(() => new TraceManager()).Value;
            _configuredProviders = new ConcurrentDictionary<string, IAuthenticationProvider>();
            
            Initialize();
        }

        #region Implementation of IAuthenticationService

        public IDictionary<string, IAuthenticationProvider> AuthenticationProviders
        {
            get { return _configuredProviders; }
        }

        public ITraceManager TraceManager { set; private get; }

        public void AddProvider(IAuthenticationProvider provider, bool replaceExisting = true)
        {
            Add(provider, replaceExisting);
        }

        public void AddProviders(IEnumerable<IAuthenticationProvider> providers, bool replaceExisting = true)
        {
            Add(providers, replaceExisting);
        }

        public void RemoveProvider(string providerName)
        {
            IAuthenticationProvider provider;
            _configuredProviders.TryRemove(providerName, out provider);
        }

        public Uri RedirectToAuthenticationProvider(string providerKey, Uri callBackUri = null)
        {
            if (string.IsNullOrEmpty(providerKey))
            {
                throw new ArgumentNullException("providerKey");
            }

            // Determine the provider.
            var authenticationProvider = GetAuthenticationProvider(providerKey);
            if (authenticationProvider == null)
            {
                throw new InvalidOperationException("No provider was found for the key: " + providerKey);
            }

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
            if (authenticationServiceSettings == null)
            {
                throw new ArgumentNullException("authenticationServiceSettings");
            }

            if (string.IsNullOrEmpty(authenticationServiceSettings.ProviderName))
            {
                throw new ArgumentException("authenticationServiceSettings.providerName");
            }

            if (authenticationServiceSettings.CallBackUri == null ||
                string.IsNullOrEmpty(authenticationServiceSettings.CallBackUri.AbsoluteUri))
            {
                throw new ArgumentException("authenticationServiceSettings.CallBackUri");
            }

            var authenticationProvider = GetAuthenticationProvider(authenticationServiceSettings.ProviderName);
            if (authenticationProvider == null)
            {
                throw new InvalidOperationException("No Provider found for the Provider Name: " +
                                                    authenticationServiceSettings.ProviderName);
            }

            return authenticationProvider.RedirectToAuthenticate(authenticationServiceSettings);
        }

        public IAuthenticatedClient GetAuthenticatedClient(IAuthenticationServiceSettings authenticationServiceSettings,
                                                           dynamic requestParameters)
        {
            var querystringParameters = new NameValueCollection();
            var keyValuesAsText = new StringBuilder();
            foreach (var item in requestParameters)
            {
                querystringParameters.Add(item, requestParameters[item]);
                keyValuesAsText.Append("Key: {0} / Value: {1}. ", item, requestParameters[item]);
            }

            if (keyValuesAsText.Length > 0)
            {
                TraceSource.TraceVerbose("Request Parameters: " + keyValuesAsText);
            }

            return GetAuthenticatedClient(authenticationServiceSettings, querystringParameters);
        }

        public IAuthenticatedClient GetAuthenticatedClient(IAuthenticationServiceSettings authenticationServiceSettings,
                                                           NameValueCollection queryStringParameters)
        {
            if (authenticationServiceSettings == null)
            {
                throw new ArgumentNullException("authenticationServiceSettings");
            }

            if (queryStringParameters == null)
            {
                throw new ArgumentNullException("queryStringParameters");
            }

            if (queryStringParameters.Count <= 0)
            {
                throw new ArgumentOutOfRangeException("queryStringParameters");
            }

            // Grab the Authentication Client.
            var authenticationProvider = GetAuthenticationProvider(authenticationServiceSettings.ProviderName);

            if (authenticationProvider == null)
            {
                var errorMessage = "Failed to find a registered provider, for the input name parameter: " +
                                   authenticationServiceSettings.ProviderName;
                TraceSource.TraceError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            TraceSource.TraceInformation("Retrieved provider: {0}.", authenticationProvider.Name);
            var authenticatedClient = authenticationProvider.AuthenticateClient(authenticationServiceSettings, queryStringParameters);

            if (authenticatedClient == null)
            {
                TraceSource.TraceWarning("Failed to retrieve an authenticated client instance. Er.. WTF?");
            }
            else
            {
                TraceSource.TraceInformation("Retrieved Authenticated Client data: {0}",
                                             authenticatedClient.UserInformation.ToLongString());
            }

            return authenticatedClient;
        }

        public IAuthenticationServiceSettings GetAuthenticateServiceSettings(string providerKey, Uri requestUrl,
                                                                             string path =
                                                                                 "/authentication/authenticatecallback")
        {
            var name = providerKey.ToLowerInvariant();

            if (string.IsNullOrEmpty(providerKey))
            {
                throw new ArgumentNullException("providerKey");
            }

            var authenticationProvider = GetAuthenticationProvider(name);
            var settings = authenticationProvider.DefaultAuthenticationServiceSettings;
            
            // Setup up some defaults.
            settings.State = Guid.NewGuid().ToString();
            settings.CallBackUri = CreateCallBackUri(providerKey, requestUrl, path);

            return settings;
        }

        #endregion

        public static ICollection<IAuthenticationProvider> RegisteredAuthenticatedProviders
        {
            get
            {
                return _configuredProviders == null
                           ? null
                           : _configuredProviders.Values;
            }
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
            if (providerConfig.Providers == null)
            {
                throw new ArgumentException("providerConfiguration.Providers");
            }

            foreach (ProviderKey provider in providerConfig.Providers)
            {
                var discoverProvider = DiscoverProvider(discoveredProviders, provider);

                Add(discoverProvider, false);
            }

            // Do we also need to add some Fake Providers?
            if (providerConfig.IncludeFakeProviders)
            {
                Add(new List<IAuthenticationProvider>
                {
                    new FakeFacebookProvider(),
                    new FakeGoogleProvider(),
                    new FakeTwitterProvider()
                }, true);
            }
        }

        private IAuthenticationProvider DiscoverProvider(IEnumerable<Type> discoveredProviders, ProviderKey providerKey)
        {
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
            if (provider.GetConstructor(new[] { typeof(ProviderParams) }) != null)
            {
                var parameters = new ProviderParams
                {
                    Key = providerKey.Key,
                    Secret = providerKey.Secret
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

        private static Uri CreateCallBackUri(string providerKey, Uri requestUrl, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            if (string.IsNullOrEmpty(providerKey))
            {
                throw new ArgumentNullException("providerKey");
            }

            if (requestUrl == null)
            {
                throw new ArgumentNullException("requestUrl");
            }

            var builder = new UriBuilder(requestUrl)
            {
                Path = path,
                Query = "providerkey=" + providerKey.ToLowerInvariant()
            };

            // Don't include port 80/443 in the Uri.
            if (builder.Uri.IsDefaultPort)
            {
                builder.Port = -1;
            }

            return builder.Uri;
        }

        private TraceSource TraceSource { get { return TraceManager["WD.Web.Authentication.AuthenticationService"]; } }

        private IAuthenticationProvider GetAuthenticationProvider(string providerKey)
        {
            IAuthenticationProvider authenticationProvider = null;

            if (AuthenticationProviders != null)
            {
                AuthenticationProviders.TryGetValue(providerKey.ToLowerInvariant(), out authenticationProvider);
            }

            if (authenticationProvider == null)
            {
                var errorMessage =
                    string.Format(
                        "No '{0}' provider details have been added/provided. Maybe you forgot to add the name/key/value data into your web.config? Eg. in your web.config configuration/authenticationProviders/providers section add the following (if you want to offer Google authentication): <add name=\"Google\" key=\"someNumber.apps.googleusercontent.com\" secret=\"someSecret\" />",
                        providerKey);
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return authenticationProvider;
        }

        private static void Add(IAuthenticationProvider provider, bool replaceExisting = true)
        {
            _configuredProviders.AddOrUpdate(provider.Name.ToLower(), provider, (key, authenticationProvider) =>
            {
                if (!replaceExisting)
                {
                    throw new WorldDominationConfigurationException(
                        string.Format("The provider '{0}' already exists and cannot be overridden, either set `replaceExisting` to `true`, or remove the provider first.", provider.Name));
                }

                return provider;
            });
        }

        private static void Add(IEnumerable<IAuthenticationProvider> providers, bool replaceExisting)
        {
            foreach (var authenticationProvider in providers)
            {
                Add(authenticationProvider, replaceExisting);
            }
        }
    }
}