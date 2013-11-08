using System.Configuration;

namespace SimpleAuthentication.Core.Config
{
    public class ProviderConfiguration : ConfigurationSection
    {
        /// <summary>
        /// The redirect route segment. Eg. what route (on your website) which prepares the data for authentication and redirects off to the particular Provider.
        /// </summary>
        /// <remarks>Leave this empty/blank to use the default.</remarks>
        [ConfigurationProperty("redirectRoute", IsRequired = false)]
        public string RedirectRoute
        {
            get { return (string)this["redirectRoute"]; }
        }

        /// <summary>
        /// The callback route segment. Eg. the route (on your website) which we use to return back to, from the Provider.
        /// </summary>
        /// <remarks>Leave this empty/blank to use the default.</remarks>
        [ConfigurationProperty("callbackRoute", IsRequired = false)]
        public string CallbackRoute
        {
            get { return (string)this["callbackRoute"]; }
        }

        /// <summary>
        /// Collection of providers to be offered.
        /// </summary>
        [ConfigurationProperty("providers")]
        public ProviderKeyCollection Providers
        {
            get { return (ProviderKeyCollection) this["providers"]; }
        }
    }
}