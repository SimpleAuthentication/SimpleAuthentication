using System.Configuration;

namespace WorldDomination.Web.Authentication.Config
{
    public class ProviderKey : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public ProviderType Name
        {
            get { return (ProviderType)this["name"]; }
        }

        [ConfigurationProperty("key", IsRequired = true)]
        public string Key
        {
            get { return this["key"] as string; }
        }

        [ConfigurationProperty("secret", IsRequired = true)]
        public string Secret
        {
            get { return this["secret"] as string; }
        }
    }
}