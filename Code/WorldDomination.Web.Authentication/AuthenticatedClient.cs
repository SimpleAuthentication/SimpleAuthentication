using System;

namespace WorldDomination.Web.Authentication
{
    public class AuthenticatedClient : IAuthenticatedClient
    {
        public AuthenticatedClient(ProviderType providerType)
        {
            ProviderType = providerType;
        }

        #region Implementation of IAuthenticatedClient

        public ProviderType ProviderType { get; private set; }
        public UserInformation UserInformation { get; set; }
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpiresOn { get; set; }

        #endregion
    }
}