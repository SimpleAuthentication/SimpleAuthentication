using System;
using System.Configuration;

namespace WorldDomination.Web.Authentication.Config
{
    public class ProviderConfiguration : ConfigurationSection
    {
        //[ConfigurationProperty("callbackUri")]
        //public string CallbackUri
        //{
        //    get { return this["callbackUri"] as string; }
        //}

        //[ConfigurationProperty("callbackQuerystringKey", DefaultValue = "providerKey")]
        //public string CallbackQuerystringKey
        //{
        //    get { return this["callbackQuerystringKey"] as string; }
        //}

        /// <summary>
        /// Auto wire up / include some fake providers.
        /// </summary>
        [ConfigurationProperty("includeFakeProviders")]
        public bool IncludeFakeProviders
        {
            get
            {
                bool includeFakeProviders;
                bool.TryParse(this["includeFakeProviders"].ToString(), out includeFakeProviders);

                return includeFakeProviders;
            }
        }

        /// <summary>
        /// Collection of providers to be offered.
        /// </summary>
        [ConfigurationProperty("providers")]
        public ProviderKeyCollection Providers
        {
            get { return (ProviderKeyCollection)this["providers"]; }
        }
    }
}