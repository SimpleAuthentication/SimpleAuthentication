namespace SimpleAuthentication.Core
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
        /// The access token.
        /// </summary>
        public AccessToken AccessToken { get; set; }

        /// <summary>
        /// User information retrieved.
        /// </summary>
        public UserInformation UserInformation { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("Provider Name: {0}. {1} User Info: {2}",
                                 string.IsNullOrEmpty(ProviderName) ? "--no provider name--" : ProviderName,
                                 AccessToken == null ? "--no access token--" : AccessToken.ToString(),
                                 UserInformation == null ? "-no user information-" : UserInformation.ToLongString());
        }
    }
}