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
        UserInformation UserInformation { get; }

        /// <summary>
        /// The access token.
        /// </summary>
        AccessToken AccessToken { get; }

        /// <summary>
        /// The full raw data for the autneticated User's Information.
        /// </summary>
        /// <remarks>This is provided in case you wish to extract other custom data.</remarks>
        string RawUserInformation { get; }
    }
}