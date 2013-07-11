using System.Configuration;

namespace SimpleAuthentication.Core.Config
{
    public class ProviderKey : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string) this["name"]; }
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

        [ConfigurationProperty("scope", IsRequired = false)]
        public string Scope
        {
            get { return this ["scope"] as string; }
        }
    }
}