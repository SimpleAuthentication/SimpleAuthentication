using System;
using System.Collections.Generic;
using System.Configuration;

namespace SimpleAuthentication.Core.Config
{
    using System.Linq;

    public static class ProviderConfigHelper
    {
        /// <summary>
        /// Retrieves the authentication settings from the applications .config file.
        /// </summary>
        /// <param name="sectionName">Optional; string: name of the config file section. Defaults to 'authenticationProviders'</param>
        /// <returns>Configuration: some configuration data.</returns>
        public static Configuration UseConfig(string sectionName = "authenticationProviders")
        {
            if (string.IsNullOrEmpty(sectionName))
            {
                throw new ArgumentNullException(sectionName);
            }

            var configSection = ConfigurationManager.GetSection(sectionName) as ProviderConfiguration;
            return configSection == null ||
                   configSection.Providers == null ||
                   configSection.Providers.Count <= 0
                       ? null
                       : new Configuration
                       {
                           RedirectRoute = configSection.RedirectRoute,
                           CallBackRoute = configSection.CallbackRoute,
                           Providers = (from p in configSection.Providers.Cast<ProviderKey>().Select(x => x.Name)
                                        select new Provider
                                        {
                                            Name = configSection.Providers[p].Name,
                                            Key = configSection.Providers[p].Key,
                                            Secret = configSection.Providers[p].Secret,
                                            Scopes = configSection.Providers[p].Scope
                                        }).ToList()
                       };
        }

        public static ProviderKey For(this ProviderConfiguration section, string providerKey)
        {
            var provider = section.Providers[providerKey];

            if (provider == null)
            {
                throw new KeyNotFoundException(string.Format("There is no configuration for {0}", providerKey));
            }

            return provider;
        }

        public static Configuration UseAppSettings()
        {
            return ConfigurationManager.AppSettings.ParseAppSettings();
        }
    }
}