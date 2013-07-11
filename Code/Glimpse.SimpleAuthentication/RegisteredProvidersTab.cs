using System;
using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Tab.Assist;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Providers;

namespace Glimpse.SimpleAuthentication
{
    public class RegisteredProvidersTab : ITab
    {
        #region ITab Implementation

        public object GetData(ITabContext context)
        {
            var tabSection = Plugin.Create("", "Name", "Type", "Public Key", "Private Key", "Scopes");

            // Grab the registered authentication providers.
            var authenticationService = new AuthenticationProviderFactory();
            var registeredProviders = authenticationService.AuthenticationProviders.Values;

            if (registeredProviders.Any())
            {
                int count = 1;
                foreach (var registeredProvider in registeredProviders)
                {
                    string publicApiKey = "-", secretApiKey = "-", scopes = "-";
                    var scopedProvider = registeredProvider as IScopedProvider;
                    if (scopedProvider != null)
                    {
                        scopes = String.Join(scopedProvider.ScopeSeparator,
                                             scopedProvider.Scopes == null ||
                                             !scopedProvider.Scopes.Any()
                                                 ? scopedProvider.DefaultScopes
                                                 : scopedProvider.Scopes);
                    }

                    var publicPrivateKeyProvider = registeredProvider as IPublicPrivateKeyProvider;
                    if (publicPrivateKeyProvider != null)
                    {
                        publicApiKey = publicPrivateKeyProvider.PublicApiKey;
                        secretApiKey = publicPrivateKeyProvider.SecretApiKey;
                    }

                    tabSection.AddRow()
                              .Column(count++)
                              .Column(registeredProvider.Name)
                              .Column(registeredProvider.AuthenticationType)
                              .Column(publicApiKey)
                              .Column(secretApiKey)
                              .Column(scopes);
                }
            }

            return tabSection;
        }

        public string Name
        {
            get { return "Authentication Providers"; }
        }

        public RuntimeEvent ExecuteOn
        {
            get { return RuntimeEvent.EndRequest; }
        }

        public Type RequestContextType
        {
            get { return null; }
        }

        #endregion
    }
}