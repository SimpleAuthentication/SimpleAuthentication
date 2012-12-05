using System;
using System.Collections.Generic;
using System.Configuration;

namespace WorldDomination.Web.Authentication.Config
{
    public static class ProviderConfigHelper
    {
        /// <summary>
        /// Used for testing, gives you the ability to specify a different config file
        /// custom config file needs to be set to copy
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static ProviderConfiguration UseConfig(string file)
        {
            var currentConfig = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            var path = currentConfig.Substring(0, currentConfig.LastIndexOf(@"\", StringComparison.Ordinal));

            var configMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = string.Concat(path, @"\", file)
            };

            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

            if (config == null)
            {
                throw new ApplicationException(string.Format("the config file {0} does not exist or isn't set to 'copy' on build", file));
            }

            var providerConfig = config.GetSection("authProviders") as ProviderConfiguration;

            if (providerConfig == null)
            {
                throw new ApplicationException("authProviders config section is missing from your .config file");
            }

            return providerConfig;
        }

        public static ProviderConfiguration UseConfig()
        {
            var providerConfig = ConfigurationManager.GetSection("authProviders") as ProviderConfiguration;

            if (providerConfig == null)
            {
                throw new ApplicationException("authProviders config section is missing from your .config file");
            }

            return providerConfig;
        }

        public static ProviderKey For(this ProviderConfiguration section, ProviderType providerEnumKey)
        {

            var provider = section.Providers[providerEnumKey];

            if (provider == null)
            {
                throw new KeyNotFoundException(string.Format("There is no configuration for {0}", providerEnumKey));
            }

            return provider;
        }
    }
}