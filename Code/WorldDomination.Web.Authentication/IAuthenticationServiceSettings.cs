using System;

namespace WorldDomination.Web.Authentication
{
    /// <summary>
    /// Defines the contract for some authentication service settings.
    /// </summary>
    public interface IAuthenticationServiceSettings
    {
        /// <summary>
        /// The Provider's unique name.
        /// </summary>
        string ProviderKey { get; }

        /// <summary>
        /// The type of provider.
        /// </summary>
        ProviderType ProviderType { get; }

        /// <summary>
        /// Optional: The default callBack uri.
        /// </summary>
        Uri CallBackUri { get; set; }

        /// <summary>
        /// Some optional state for this authentication process.
        /// </summary>
        string State { get; set; }
    }
}