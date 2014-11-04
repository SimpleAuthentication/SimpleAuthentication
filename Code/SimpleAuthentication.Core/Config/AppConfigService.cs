using System;
using System.Configuration;
using System.Linq;
using SimpleAuthentication.Core.Exceptions;

namespace SimpleAuthentication.Core.Config
{
    public class AppConfigService : IConfigService
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
            var configuration = ConfigurationManager.AppSettings.ParseAppSettings();

            if (configuration == null ||
                configuration.Providers == null ||
                !configuration.Providers.Any())
            {
                throw new AuthenticationException("AppSettings section parsed and -no- provider's were found. At least one key/value is required in the <appSettings> section so we can authenticate against a provider. A sample key/value is: <add key=\"sa.Google\" value=\"key:blahblahblah.apps.googleusercontent.com;secret:pew-pew\" />");
            }

            return configuration;
        }
    }
}