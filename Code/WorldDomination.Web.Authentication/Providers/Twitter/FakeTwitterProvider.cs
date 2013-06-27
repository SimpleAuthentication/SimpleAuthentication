using System;
using System.Collections.Specialized;
using System.Diagnostics;

namespace WorldDomination.Web.Authentication.Providers.Twitter
{
    public class FakeTwitterProvider : BaseProvider, IFakeAuthenticationProvider
    {
        #region Implementation of IAuthenticationProvider

        protected override TraceSource TraceSource
        {
            get { return TraceManager["WD.Web.Authentication.Providers." + Name]; }
        }

        public string Name
        {
            get { return "FakeTwitter"; }
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
                                         PublicToken = "EstSularusOthMithas",
                                         ExpiresOn = DateTime.UtcNow.AddDays(30),
                                     },
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

        public IAuthenticationServiceSettings DefaultAuthenticationServiceSettings
        {
            get { return new TwitterAuthenticationServiceSettings(true); }
        }

        #endregion

        #region Implementation of IFakeAuthenticationProvider

        public string RedirectToAuthenticateExceptionMessage { set; private get; }
        public UserInformation UserInformation { set; private get; }
        public string AuthenticateClientExceptionMessage { set; private get; }

        #endregion
    }
}