using System;
using System.Collections.Specialized;
using CuttingEdge.Conditions;

namespace WorldDomination.Web.Authentication.Google
{
    public class FakeGoogleProvider : IFakeAuthenticationProvider
    {
        private readonly Uri _redirectToAuthenticateUri;

        public FakeGoogleProvider(Uri redirectToAuthenticateUri)
        {
            Condition.Requires(redirectToAuthenticateUri);
            Condition.Requires(redirectToAuthenticateUri.AbsoluteUri).IsNotNull();

            _redirectToAuthenticateUri = redirectToAuthenticateUri;
        }

        #region Implementation of IAuthenticationProvider

        public string Name
        {
            get { return "Google"; }
        }

        public Uri CallBackUri { get; private set; }

        public Uri RedirectToAuthenticate(IAuthenticationServiceSettings authenticationServiceSettings)
        {
            Condition.WithExceptionOnFailure<AuthenticationException>()
                     .Requires(RedirectToAuthenticateExceptionMessage).IsNotNullOrEmpty();

            CallBackUri = authenticationServiceSettings.CallBackUri;

            return _redirectToAuthenticateUri ?? new Uri("http://bit.ly/RD3lQT");
        }

        public IAuthenticatedClient AuthenticateClient(NameValueCollection parameters, string existingState)
        {
            if (!string.IsNullOrEmpty(AuthenticateClientExceptionMessage))
            {
                throw new AuthenticationException(AuthenticateClientExceptionMessage);
            }

            return new AuthenticatedClient(ProviderType.Facebook)
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