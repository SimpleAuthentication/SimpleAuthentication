using System;
using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Tab.Assist;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Providers;

namespace Glimpse.WorldDomination.Web.Authentication
{
    public class RegisteredProvidersTab : ITab
    {
        #region ITab Implementation

        public object GetData(ITabContext context)
        {
            var tabSection = Plugin.Create("", "Name", "Public Key", "Private Key", "Scopes");

            // Grab the registered authentication providers.
            var registeredProviders = AuthenticationService.RegisteredAuthenticatedProviders;

            if (registeredProviders != null &&
                registeredProviders.Any())
            {
                int count = 1;
                foreach (var registeredProvider in registeredProviders)
                {
                    string key = "-", secret = "-", scopes = "-";
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
                        key = publicPrivateKeyProvider.Key;
                        secret = publicPrivateKeyProvider.Secret;
                    }

                    tabSection.AddRow()
                              .Column(count++)
                              .Column(registeredProvider.Name)
                              .Column(key)
                              .Column(secret)
                              .Column(scopes);
                }
            }

            return tabSection;
        }

        public string Name
        {
            get { return "Registered Providers"; }
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