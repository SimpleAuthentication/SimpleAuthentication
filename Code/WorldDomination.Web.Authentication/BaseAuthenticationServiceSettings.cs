using System;
using CuttingEdge.Conditions;

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
            Condition.Requires(providerKey).IsNotNullOrEmpty();

            ProviderKey = providerKey;
        }

        #region Implementation of IAuthenticationServiceSettings

        /// <summary>
        /// The Provider's unique name.
        /// </summary>
        public string ProviderKey { get; private set; }

        /// <summary>
        /// The type of provider.
        /// </summary>
        public ProviderType ProviderType
        {
            get
            {
                if (string.IsNullOrEmpty(ProviderKey))
                {
                    return ProviderType.Unknown;
                }

                ProviderType providerType;
                return Enum.TryParse(ProviderKey, out providerType) ? providerType : ProviderType.Other;
            }
        }

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