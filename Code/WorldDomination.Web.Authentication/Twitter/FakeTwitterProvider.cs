using System;
using System.Collections.Specialized;
using CuttingEdge.Conditions;

namespace WorldDomination.Web.Authentication.Twitter
{
    public class FakeTwitterProvider : IFakeAuthenticationProvider
    {
        private readonly Uri _redirectToAuthenticateUri;

        public FakeTwitterProvider(Uri redirectToAuthenticateUri)
        {
            Condition.Requires(redirectToAuthenticateUri);
            Condition.Requires(redirectToAuthenticateUri.AbsoluteUri).IsNotNull();

            _redirectToAuthenticateUri = redirectToAuthenticateUri;
        }

        #region Implementation of IAuthenticationProvider

        public string Name
        {
            get { return "Twitter"; }
        }

        public Uri RedirectToAuthenticate(IAuthenticationServiceSettings authenticationServiceSettings)
        {
            if (!string.IsNullOrEmpty(RedirectToAuthenticateExceptionMessage))
            {
                throw new AuthenticationException(RedirectToAuthenticateExceptionMessage);
            }

            return _redirectToAuthenticateUri ?? new Uri("bitly.com/Ttw62r");
        }

        public IAuthenticatedClient AuthenticateClient(NameValueCollection parameters, string existingState)
        {
            if (!string.IsNullOrEmpty(AuthenticateClientExceptionMessage))
            {
                throw new AuthenticationException(AuthenticateClientExceptionMessage);
            }

            return new AuthenticatedClient(ProviderType.Facebook)
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

        public IAuthenticationServiceSettings DefaultAuthenticationServiceSettings { get; private set; }

        #endregion

        #region Implementation of IFakeAuthenticationProvider

        public string RedirectToAuthenticateExceptionMessage { set; private get; }
        public UserInformation UserInformation { set; private get; }
        public string AuthenticateClientExceptionMessage { set; private get; }

        #endregion
    }
}