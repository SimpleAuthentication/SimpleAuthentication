using System;
using System.Collections.Generic;
using System.Configuration;

namespace SimpleAuthentication.Core.Config
{
    using System.Linq;

    public static class ProviderConfigHelper
    {
        /// <summary>
        /// Used for testing, gives you the ability to specify a different config file
        /// custom config file needs to be set to copy
        /// </summary>
        /// <param name="file">string: the name of a config file to load and check.</param>
        /// <param name="sectionName">Optional; string: name of the config file section. Defaults to 'authenticationProviders'</param>
        /// <returns>ProviderConfiguration: some configuration data.</returns>
        public static ProviderConfiguration UseConfig(string file, string sectionName)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException("file");
            }

            if (string.IsNullOrEmpty(sectionName))
            {
                throw new ArgumentNullException("sectionName");
            }

            var currentConfig = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            var path = currentConfig.Substring(0, currentConfig.LastIndexOf(@"\", StringComparison.Ordinal));

            var configMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = string.Concat(path, @"\", file)
            };

            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

            if (config == null)
            {
                throw new ApplicationException(
                    string.Format("the config file {0} does not exist or isn't set to 'copy' on build", file));
            }

            var providerConfig = config.GetSection(sectionName) as ProviderConfiguration;

            if (providerConfig == null)
            {
                throw new ApplicationException("Missing the config section [" + sectionName + "] from your .config file");
            }

            return providerConfig;
        }

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