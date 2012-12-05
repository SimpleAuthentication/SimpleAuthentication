using System.Configuration;

namespace WorldDomination.Web.Authentication.Config
{
    public class ProviderConfiguration : ConfigurationSection
    {
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