using System;
using System.Collections.Specialized;

namespace WorldDomination.Web.Authentication.ExtraProviders.Amazon
{
    public class FakeAmazonProvider : IFakeAuthenticationProvider
    {
        private readonly Uri _redirectToAuthenticateUri;

        public FakeAmazonProvider(Uri redirectToAuthenticateUri)
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
            get { return "Amazon"; }
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

            return _redirectToAuthenticateUri ?? new Uri("http://a.fake.amazon/uri");
        }

        public IAuthenticatedClient AuthenticateClient(IAuthenticationServiceSettings authenticationServiceSettings,
                                                       NameValueCollection queryStringParameters)
        {
            return new AuthenticatedClient("amazon")
            {
                AccessToken = "EstSularusOthMithas-MyHonorIsMyLife",
                AccessTokenExpiresOn = DateTime.UtcNow.AddDays(30),
                UserInformation = UserInformation ?? new UserInformation
                {
                    Id = "FakeId-" + Guid.NewGuid().ToString(),
                    Name = "Sturm Brightblade",
                    Email = "fakeee@sturm.com"
                }
            };
        }

        public IAuthenticationServiceSettings DefaultAuthenticationServiceSettings
        {
            get { return new AmazonAuthenticationServiceSettings(); }
        }

        #endregion

        #region Implementation of IFakeAuthenticationProvider

        public string RedirectToAuthenticateExceptionMessage { set; private get; }
        public UserInformation UserInformation { set; private get; }
        public string AuthenticateClientExceptionMessage { set; private get; }

        #endregion
    }
}