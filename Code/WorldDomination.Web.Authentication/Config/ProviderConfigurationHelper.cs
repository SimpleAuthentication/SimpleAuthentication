using System;
using System.Collections.Generic;
using System.Configuration;
using CuttingEdge.Conditions;

namespace WorldDomination.Web.Authentication.Config
{
    public static class ProviderConfigHelper
    {
        /// <summary>
        /// Used for testing, gives you the ability to specify a different config file
        /// custom config file needs to be set to copy
        /// </summary>
        /// <param name="file">string: the name of a config file to load and check.</param>
        /// <param name="sectionName">Optional; string: name of the config file section. Defaults to 'authenticationProviders'</param>
        /// <returns>ProviderConfiguration: some configuration data.</returns>
        public static ProviderConfiguration UseConfig(string file, string sectionName = "authenticationProviders")
        {
            Condition.Requires(file).IsNotNullOrEmpty();
            Condition.Requires(sectionName).IsNotNullOrEmpty();

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
        /// <returns>ProviderConfiguration: some configuration data.</returns>
        public static ProviderConfiguration UseConfig(string sectionName = "authenticationProviders")
        {
            Condition.Requires(sectionName).IsNotNullOrEmpty();

            var providerConfig = ConfigurationManager.GetSection(sectionName) as ProviderConfiguration;

            if (providerConfig == null)
            {
                throw new ApplicationException("Missing the config section [" + sectionName + "] from your .config file");
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

        //public static 
    }
}