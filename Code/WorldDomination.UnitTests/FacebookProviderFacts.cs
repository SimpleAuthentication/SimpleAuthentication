using System;
using WorldDomination.Web.Authentication.Facebook;
using Xunit;

namespace WorldDomination.UnitTests
{
    // ReSharper disable InconsistentNaming

    public class FacebookProviderFacts
    {
        #region Nested type: RetrieveAccessTokenFacts

        public class RetrieveAccessTokenFacts
        {
            [Fact]
            public void GivenValidCredentials_RetrieveAccessToken_ReturnsAnAccessTokenAndUserInformation()
            {
                // Arrange.
                var mockWebClientWrapper = MoqUtilities.MockedIWebClientWrapper();
                var facebookProvider = new FacebookProvider("a", "b", new Uri("http://www.google.com"),
                                                            mockWebClientWrapper.Object);
                var facebookClient = new FacebookClient
                                         {
                                             Code = "aa",
                                             State = "bb"
                                         };

                // Act.
                facebookProvider.RetrieveAccessToken(facebookClient);

                // Assert.
                Assert.NotNull(facebookClient);
                Assert.NotNull(facebookClient.AccessToken);
                Assert.NotNull(facebookClient.Code);
                Assert.NotNull(facebookClient.UserInformation);
                Assert.True(facebookClient.UserInformation.Id > 0);
                Assert.NotNull(facebookClient.UserInformation.FirstName);
                Assert.NotNull(facebookClient.UserInformation.LastName);
            }
        }

        #endregion
    }

    // ReSharper restore InconsistentNaming
}