using System;
using System.Configuration;
using System.Linq;

namespace SimpleAuthentication.Core.Config
{
    public class ConfigService : IConfigService
    {
        public Configuration GetConfiguration()
        {
            return UseAppSettings() ?? UseConfig();
        }

        private static Configuration UseConfig(string sectionName = "authenticationProviders")
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
                    Providers = (from p in configSection.Providers.AllKeys
                        select new Provider
                        {
                            Name = configSection.Providers[p].Name,
                            Key = configSection.Providers[p].Key,
                            Secret = configSection.Providers[p].Secret,
                            Scopes = configSection.Providers[p].Scope
                        }).ToList()
                };
        }

        private static Configuration UseAppSettings()
        {
            return ConfigurationManager.AppSettings.ParseAppSettings();
        }
    }
}