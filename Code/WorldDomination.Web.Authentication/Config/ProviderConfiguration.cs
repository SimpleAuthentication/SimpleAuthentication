using System.Configuration;

namespace WorldDomination.Web.Authentication.Config
{
    public class ProviderConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("callbackUri", IsRequired = true)]
        public string CallbackUri
        {
            get { return this["callbackUri"] as string; }
        }

        [ConfigurationProperty("callbackQuerystringKey", DefaultValue = "providerKey")]
        public string CallbackQuerystringKey
        {
            get { return this["callbackQuerystringKey"] as string; }
        }

        [ConfigurationProperty("providers")]
        public ProviderKeyCollection Providers
        {
            get
            {
                return this["providers"] as ProviderKeyCollection;
            }
        }
    }
}