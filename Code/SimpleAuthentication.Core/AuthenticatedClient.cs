using System;

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
        /// <param name="accessToken">The retrieved access token.</param>
        /// <param name="userInformation">The extracted user's information.</param>
        /// <param name="rawUserInformation">The full raw, unmodified user information content.</param>
        /// <remarks>The rawUserInformation content is usually in json or xml format, but could be in any format. It also includes way more information which the UserInformation property extracts.</remarks>
        public AuthenticatedClient(string providerName,
            AccessToken accessToken,
            UserInformation userInformation,
            string rawUserInformation)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                throw new ArgumentNullException("providerName");
            }

            if (accessToken == null)
            {
                throw new ArgumentNullException("accessToken");
            }

            if (userInformation == null)
            {
                throw new ArgumentNullException();
            }

            if (string.IsNullOrWhiteSpace(rawUserInformation))
            {
                throw new ArgumentNullException("rawUserInformation");
            }

            ProviderName = providerName;
            AccessToken = accessToken;
            UserInformation = userInformation;
            RawUserInformation = rawUserInformation;
        }

        #region Implementation of IAuthenticatedClient

        /// <summary>
        /// An Authentication Provider.
        /// </summary>
        public string ProviderName { get; private set; }

        /// <summary>
        /// The access token.
        /// </summary>
        public AccessToken AccessToken { get; private set; }

        public string RawUserInformation { get; private set; }

        /// <summary>
        /// User information retrieved.
        /// </summary>
        public UserInformation UserInformation { get; private set; }

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