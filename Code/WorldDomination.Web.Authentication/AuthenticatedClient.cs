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
        /// <param name="providerName">A Provider's name.</param>
        public AuthenticatedClient(string providerName)
        {
            ProviderName = providerName;
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

        public override string ToString()
        {
            return string.Format("Provider Name: {0}. Access Token: {1}. Access Token Expires on: {2}. User Info: {3}",
                string.IsNullOrEmpty(ProviderName) ? "-no provider name-": ProviderName,
                string.IsNullOrEmpty(AccessToken) ? "-no provider name-": AccessToken,
                (string.Format("{0} {1}", AccessTokenExpiresOn.ToLongDateString(), AccessTokenExpiresOn.ToLongTimeString())),
                UserInformation == null ? "-no user information-" : UserInformation.ToLongString());
        }
    }
}