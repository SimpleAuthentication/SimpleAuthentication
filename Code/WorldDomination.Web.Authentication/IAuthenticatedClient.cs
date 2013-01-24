using System;

namespace WorldDomination.Web.Authentication
{
    /// <summary>
    /// Defines the contract that authenticated clients from Authenticated providers must impliment.
    /// </summary>
    public interface IAuthenticatedClient
    {
        /// <summary>
        /// An Authentication Provider.
        /// </summary>
        ProviderType ProviderType { get; }

        /// <summary>
        /// User information retrieved.
        /// </summary>
        UserInformation UserInformation { get; set; }

        /// <summary>
        /// An access token.
        /// </summary>
        string AccessToken { get; set; }

        /// <summary>
        /// When the access token expires. Always in UTC.
        /// </summary>
        DateTime AccessTokenExpiresOn { get; set; }
    }
}