using System;

namespace WorldDomination.Web.Authentication
{
    public class AuthenticatedClient : IAuthenticatedClient
    {
        private ErrorInformation _errorInformation;
        private UserInformation _userInformation;

        public AuthenticatedClient(ProviderType providerType)
        {
            ProviderType = providerType;
        }

        #region Implementation of IAuthenticatedClient

        public Status Status { get; private set; }
        public ProviderType ProviderType { get; private set; }

        public UserInformation UserInformation
        {
            get { return _userInformation; }
            set
            {
                _userInformation = value;
                SetStatus();
            }
        }

        public ErrorInformation ErrorInformation
        {
            get { return _errorInformation; }
            set
            {
                _errorInformation = value;
                SetStatus();
            }
        }

        public string AccessToken { get; set; }
        public DateTime AccessTokenExpiresOn { get; set; }

        #endregion

        private void SetStatus()
        {
            if (_userInformation != null)
            {
                Status = Status.Authenticated;
            }
            else if (_errorInformation != null)
            {
                Status = Status.Denied;
            }
            else
            {
                Status = Status.Unknown;
            }
        }
    }
}