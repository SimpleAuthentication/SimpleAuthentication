using System;
using System.Collections.Specialized;

namespace WorldDomination.Web.Authentication.Google
{
    public class FakeGoogleProvider : IFakeAuthenticationProvider
    {
        private readonly Uri _redirectToAuthenticateUri;

        public FakeGoogleProvider(Uri redirectToAuthenticateUri)
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
            get { return "Google"; }
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

            return _redirectToAuthenticateUri ?? new Uri("http://bit.ly/RD3lQT");
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

            return new AuthenticatedClient("google")
            {
                AccessToken = "SomethingWonderfulHasHappened.AniImPregnant",
                AccessTokenExpiresOn = DateTime.UtcNow.AddDays(30),
                UserInformation = UserInformation ?? new UserInformation
                {
                    Gender = GenderType.Female,
                    Id = "FakeId-" + Guid.NewGuid().ToString(),
                    Locale = "en-au",
                    Name = "Natalie Portman",
                    Picture = "http://i.imgur.com/B9es0.jpg",
                    UserName = "Natalie.Portman"
                }
            };
        }

        public IAuthenticationServiceSettings DefaultAuthenticationServiceSettings
        {
            get { return new GoogleAuthenticationServiceSettings(); }
        }

        #endregion

        #region Implementation of IFakeAuthenticationProvider

        public string RedirectToAuthenticateExceptionMessage { set; private get; }
        public UserInformation UserInformation { set; private get; }
        public string AuthenticateClientExceptionMessage { set; private get; }

        #endregion
    }
}