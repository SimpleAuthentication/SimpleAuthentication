using System.Configuration;

namespace WorldDomination.Web.Authentication.Config
{
    public class ProviderKey : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public ProviderEnum Name
        {
            get { return (ProviderEnum)this["name"]; }
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