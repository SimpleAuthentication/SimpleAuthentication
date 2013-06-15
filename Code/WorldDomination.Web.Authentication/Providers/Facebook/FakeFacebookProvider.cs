using System;
using System.Collections.Specialized;
using System.Diagnostics;
using WorldDomination.Web.Authentication.Tracing;

namespace WorldDomination.Web.Authentication.Providers.Facebook
{
    public class FakeFacebookProvider : BaseProvider, IFakeAuthenticationProvider
    {
        #region Implementation of IAuthenticationProvider

        public string Name
        {
            get { return "FakeFacebook"; }
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

            var redirectUrl = string.Format("{0}&state={1}", authenticationServiceSettings.CallBackUri.AbsoluteUri,
                authenticationServiceSettings.State);
            return new Uri(redirectUrl);
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
                AccessToken = new AccessToken
                              {
                                  PublicToken = "IAmALittleTeaPotShortAndStout",
                                  ExpiresOn = DateTime.UtcNow.AddDays(30),
                              },
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

        protected override TraceSource TraceSource
        {
            get { return TraceManager["WD.Web.Authentication.Providers." + Name]; }
        }

        public IAuthenticationServiceSettings DefaultAuthenticationServiceSettings
        {
            get
            {
                return new FacebookAuthenticationServiceSettings(true)
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