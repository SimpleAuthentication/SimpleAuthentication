namespace SimpleAuthentication.Core
{
    /// <summary>
    /// Defines the contract that authenticated clients from Authenticated providers must impliment.
    /// </summary>
    public interface IAuthenticatedClient
    {
        /// <summary>
        /// An Authentication Provider.
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// User information retrieved.
        /// </summary>
        UserInformation UserInformation { get; set; }

        /// <summary>
        /// The access token.
        /// </summary>
        AccessToken AccessToken { get; set; }
    }
}