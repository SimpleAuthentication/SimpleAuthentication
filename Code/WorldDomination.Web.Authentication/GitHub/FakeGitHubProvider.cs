namespace WorldDomination.Web.Authentication.GitHub
{
    using System;
    using System.Collections.Specialized;

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

        public string Name { get { return "GitHub"; } }
        
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

            return _redirectToAuthenticateUri ?? new Uri("http://a.fake.git.hub/uri");
        }

        public IAuthenticatedClient AuthenticateClient(NameValueCollection parameters, string existingState)
        {
            return null;
        }

        public IAuthenticationServiceSettings DefaultAuthenticationServiceSettings
        {
            get { return new GitHubAuthenticationServiceSettings(); }
        }

        #endregion


        #region Implementation of IFakeAuthenticationProvider

        public string RedirectToAuthenticateExceptionMessage { set; private get; }
        public UserInformation UserInformation { set; private get; }
        public string AuthenticateClientExceptionMessage { set; private get; }

        #endregion
    }
}