using System;
using System.Collections.Specialized;

namespace WorldDomination.Web.Authentication.Providers.Twitter
{
    public class FakeTwitterProvider : IFakeAuthenticationProvider
    {
        private readonly Uri _redirectToAuthenticateUri;

        public FakeTwitterProvider(Uri redirectToAuthenticateUri)
        {
            if (redirectToAuthenticateUri == null)
            {
                throw new ArgumentNullException("redirectToAuthenticateUri");
            }

            if (string.IsNullOrEmpty(redirectToAuthenticateUri.AbsoluteUri))
            {
                throw new ArgumentException("redirectToAuthenticateUri.AbsoluteUri");
            }

            _redirectToAuthenticateUri = redirectToAuthenticateUri;
        }

        #region Implementation of IAuthenticationProvider

        public string Name
        {
            get { return "Twitter"; }
        }

        public Uri RedirectToAuthenticate(IAuthenticationServiceSettings authenticationServiceSettings)
        {
            if (authenticationServiceSettings == null)
            {
                throw new ArgumentNullException("authenticationServiceSettings");
            }

            if (authenticationServiceSettings.CallBackUri == null)
            {
                throw new ArgumentException("authenticationServiceSettings.CallBackUri");
            }

            return _redirectToAuthenticateUri ?? new Uri("bitly.com/Ttw62r");
        }

        public IAuthenticatedClient AuthenticateClient(IAuthenticationServiceSettings authenticationServiceSettings,
                                                       NameValueCollection queryStringParameters)
        {
            if (authenticationServiceSettings == null)
            {
                throw new ArgumentNullException("authenticationServiceSettings");
            }

            if (!string.IsNullOrEmpty(AuthenticateClientExceptionMessage))
            {
                throw new AuthenticationException(AuthenticateClientExceptionMessage);
            }

            return new AuthenticatedClient("facebook")
            {
                AccessToken = "EstSularusOthMithas-MyHonorIsMyLife",
                AccessTokenExpiresOn = DateTime.UtcNow.AddDays(30),
                UserInformation = UserInformation ?? new UserInformation
                {
                    Gender = GenderType.Male,
                    Id = "FakeId-" + Guid.NewGuid().ToString(),
                    Locale = "en-au",
                    Name = "Sturm Brightblade",
                    Picture = "http://i.imgur.com/jtoOF.jpg",
                    UserName = "Sturm.Brightblade"
                }
            };
        }

        public IAuthenticationServiceSettings DefaultAuthenticationServiceSettings { get { return new TwitterAuthenticationServiceSettings(); } }

        #endregion

        #region Implementation of IFakeAuthenticationProvider

        public string RedirectToAuthenticateExceptionMessage { set; private get; }
        public UserInformation UserInformation { set; private get; }
        public string AuthenticateClientExceptionMessage { set; private get; }

        #endregion
    }
}