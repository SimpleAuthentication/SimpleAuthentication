﻿using System;
using System.Collections.Specialized;
using CuttingEdge.Conditions;

namespace WorldDomination.Web.Authentication.Facebook
{
    public class FakeFacebookProvider : IFakeAuthenticationProvider
    {
        private readonly Uri _redirectToAuthenticateUri;

        public FakeFacebookProvider(Uri redirectToAuthenticateUri)
        {
            Condition.Requires(redirectToAuthenticateUri);
            Condition.Requires(redirectToAuthenticateUri.AbsoluteUri).IsNotNull();

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
            Condition.WithExceptionOnFailure<AuthenticationException>()
                     .Requires(RedirectToAuthenticateExceptionMessage).IsNotNullOrEmpty();

            CallBackUri = authenticationServiceSettings.CallBackUri;

            return _redirectToAuthenticateUri ?? new Uri("http://some.fake.uri/with/lots/of/pewpew");
        }

        public IAuthenticatedClient AuthenticateClient(NameValueCollection parameters, string existingState)
        {
            if (!string.IsNullOrEmpty(AuthenticateClientExceptionMessage))
            {
                throw new AuthenticationException(AuthenticateClientExceptionMessage);
            }

            return new AuthenticatedClient(ProviderType.Facebook)
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