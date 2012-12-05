using System.Configuration;

namespace WorldDomination.Web.Authentication.Config
{
    public class ProviderKeyCollection : KeyValueConfigurationCollection
    {
        public ProviderKey this[ProviderType provider]
        {
            get
            {
                return base.BaseGet(provider) as ProviderKey;
            }
            set
            {
                if (base.BaseGet(provider) != null)
                {
                    var index = base.BaseIndexOf(base.BaseGet(provider));
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(0, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ProviderKey();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ProviderKey)element).Name;
        } 
    }
}