using System;

namespace WorldDomination.Web.Authentication
{
    /// <summary>
    /// Some common (abstract) authentication service settings.
    /// </summary>
    public abstract class BaseAuthenticationServiceSettings : IAuthenticationServiceSettings
    {
        /// <summary>
        /// Initializes a new instance of a BaseAuthenticationServiceSettings.
        /// </summary>
        /// <param name="providerKey">The Provider key.</param>
        protected BaseAuthenticationServiceSettings(string providerKey)
        {
            if (string.IsNullOrEmpty(providerKey))
            {
                throw new ArgumentNullException("providerKey");
            }

            ProviderName = providerKey;
        }

        #region Implementation of IAuthenticationServiceSettings

        /// <summary>
        /// The Provider's unique name.
        /// </summary>
        public string ProviderName { get; private set; }

        /// <summary>
        /// Optional: The default callBack uri.
        /// </summary>
        public Uri CallBackUri { get; set; }

        /// <summary>
        /// Some optional state for this authentication process.
        /// </summary>
        public string State { get; set; }

        #endregion
    }
}