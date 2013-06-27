using System;
using System.Collections.Specialized;
using System.Diagnostics;

namespace WorldDomination.Web.Authentication.Providers.Google
{
    public class FakeGoogleProvider : BaseProvider, IFakeAuthenticationProvider
    {
        #region Implementation of IAuthenticationProvider

        protected override TraceSource TraceSource
        {
            get { return TraceManager["WD.Web.Authentication.Providers." + Name]; }
        }

        public string Name
        {
            get { return "FakeGoogle"; }
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

            return new AuthenticatedClient("google")
            {
                AccessToken = new AccessToken
                {
                    PublicToken = "SomethingWonderfulHasHappened.AniSkywalkerIAmPregnant",
                    ExpiresOn = DateTime.UtcNow.AddDays(30),
                },
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
            get { return new GoogleAuthenticationServiceSettings(true); }
        }

        #endregion

        #region Implementation of IFakeAuthenticationProvider

        public string RedirectToAuthenticateExceptionMessage { set; private get; }
        public UserInformation UserInformation { set; private get; }
        public string AuthenticateClientExceptionMessage { set; private get; }

        #endregion
    }
}