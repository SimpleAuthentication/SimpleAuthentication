using System;
using System.Collections.Specialized;

namespace WorldDomination.Web.Authentication.Facebook
{
    public class FakeFacebookProvider : IFakeOAuthAuthenticationProvider
    {
        private readonly Uri _redirectToAuthenticateUri;

        public FakeFacebookProvider(Uri redirectToAuthenticateUri)
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
            get { return "Facebook"; }
        }

        public Uri CallBackUri { get; private set; }

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

            CallBackUri = authenticationServiceSettings.CallBackUri;

            return _redirectToAuthenticateUri ?? new Uri("http://some.fake.uri/with/lots/of/pewpew");
        }

        public IAuthenticatedClient AuthenticateClient(NameValueCollection parameters, string existingState)
        {
            if (!string.IsNullOrEmpty(AuthenticateClientExceptionMessage))
            {
                throw new AuthenticationException(AuthenticateClientExceptionMessage);
            }

            return new AuthenticatedClient("facebook")
            {
                AccessToken = "IAmALittleTeaPotShortAndStout",
                AccessTokenExpiresOn = DateTime.UtcNow.AddDays(30),
                UserInformation = UserInformation ?? new UserInformation
                {
                    Gender = GenderType.Female,
                    Id = "FakeId-" + Guid.NewGuid().ToString(),
                    Locale = "en-au",
                    Name = "Leah Culver",
                    Picture = "http://i.imgur.com/f4mIx.png",
                    UserName = "Leah.Culver"
                }
            };
        }

        public IAuthenticationServiceSettings DefaultAuthenticationServiceSettings
        {
            get
            {
                return new FacebookAuthenticationServiceSettings
                {
                    Display = DisplayType.Unknown,
                    IsMobile = false
                };
            }
        }

        #endregion

        #region Implementation of IFakeAuthenticationProvider

        public string RedirectToAuthenticateExceptionMessage { private get; set; }
        public UserInformation UserInformation { private get; set; }
        public string AuthenticateClientExceptionMessage { private get; set; }

        #endregion
    }
}