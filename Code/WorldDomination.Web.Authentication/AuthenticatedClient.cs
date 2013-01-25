using System;

namespace WorldDomination.Web.Authentication
{
    /// <summary>
    /// An authenticated client containing data from the Authentication Provider.
    /// </summary>
    public class AuthenticatedClient : IAuthenticatedClient
    {
        /// <summary>
        /// Initializes a new instance of an AuthenticatedClient.
        /// </summary>
        /// <param name="providerType">A Provider.</param>
        public AuthenticatedClient(string providerType)
        {
            ProviderName = providerType;
        }

        #region Implementation of IAuthenticatedClient

        /// <summary>
        /// An Authentication Provider.
        /// </summary>
        public string ProviderName { get; private set; }

        /// <summary>
        /// User information retrieved.
        /// </summary>
        public UserInformation UserInformation { get; set; }

        /// <summary>
        /// An access token.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// When the access token expires. Always in UTC.
        /// </summary>
        public DateTime AccessTokenExpiresOn { get; set; }

        #endregion
    }
}