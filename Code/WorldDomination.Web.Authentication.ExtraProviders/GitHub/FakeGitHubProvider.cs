using System;
using System.Collections.Specialized;
using System.Diagnostics;
using WorldDomination.Web.Authentication.Tracing;

namespace WorldDomination.Web.Authentication.ExtraProviders.GitHub
{
    public class FakeGitHubProvider : IFakeAuthenticationProvider
    {
        private readonly Uri _redirectToAuthenticateUri;

        public FakeGitHubProvider(Uri redirectToAuthenticateUri)
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
            get { return "GitHub"; }
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

            return _redirectToAuthenticateUri ?? new Uri("http://a.fake.git.hub/uri");
        }

        public IAuthenticatedClient AuthenticateClient(IAuthenticationServiceSettings authenticationServiceSettings,
                                                       NameValueCollection queryStringParameters)
        {
            return new AuthenticatedClient("github")
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

        public IAuthenticationServiceSettings DefaultAuthenticationServiceSettings
        {
            get { return new GitHubAuthenticationServiceSettings(); }
        }

        public ITraceManager TraceManager { set; private get; }

        protected TraceSource TraceSource
        {
            get { return TraceManager["WD.Web.Authentication.Providers." + Name]; }
        }

        #endregion

        #region Implementation of IFakeAuthenticationProvider

        public string RedirectToAuthenticateExceptionMessage { set; private get; }
        public UserInformation UserInformation { set; private get; }
        public string AuthenticateClientExceptionMessage { set; private get; }

        #endregion
    }
}