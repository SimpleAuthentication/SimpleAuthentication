using System;
using WorldDomination.Web.Authentication.Facebook;
using Xunit;

namespace WorldDomination.UnitTests
{
    // ReSharper disable InconsistentNaming

    public class FacebookProviderFacts
    {
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
                Assert.NotNull(facebookClient.UserInformation.Name);
                Assert.NotNull(facebookClient.UserInformation.UserName);
            }

            [Fact]
            public void GivenSomeInvalidResult_RetrieveAccessToken_ThrowsAnException()
            {
                // Arrange.
                var mockWebClientWrapper = MoqUtilities.MockedIWebClientWrapper(new [] { "asds", null});
                var facebookProvider = new FacebookProvider("a", "b", new Uri("http://www.google.com"),
                                                            mockWebClientWrapper.Object);
                var facebookClient = new FacebookClient
                {
                    Code = "aa",
                    State = "bb"
                };

                // Act.
                var result = Assert.Throws<ArgumentException>(() => facebookProvider.RetrieveAccessToken(facebookClient));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("value should contain 2 elements. value contains currently 1 element.\r\nParameter name: value", result.Message);
            }

            [Fact]
            public void GivenAMissingExpiresParam_RetrieveAccessToken_ThrowsAnException()
            {
                // Arrange.
                var mockWebClientWrapper = MoqUtilities.MockedIWebClientWrapper(new[] { "access_token=foo&hi=ohnoes", null });
                var facebookProvider = new FacebookProvider("a", "b", new Uri("http://www.google.com"),
                                                            mockWebClientWrapper.Object);
                var facebookClient = new FacebookClient
                {
                    Code = "aa",
                    State = "bb"
                };

                // Act.
                var result = Assert.Throws<ArgumentException>(() => facebookProvider.RetrieveAccessToken(facebookClient));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("value should be equal to 2. The actual value is 1.\r\nParameter name: value", result.Message);
            }

            [Fact]
            public void GivenSomethingWeirdHappenedWhileTryingToRetrieveMeData_RetrieveAccessToken_ThrowsAnException()
            {
                // Arrange.
                var mockWebClientWrapper = MoqUtilities.MockedIWebClientWrapper(new[] { "access_token=foo&expires=1", "ohcrap" });
                var facebookProvider = new FacebookProvider("a", "b", new Uri("http://www.google.com"),
                                                            mockWebClientWrapper.Object);
                var facebookClient = new FacebookClient
                {
                    Code = "aa",
                    State = "bb"
                };

                // Act.
                var result = Assert.Throws<InvalidOperationException>(() => facebookProvider.RetrieveAccessToken(facebookClient));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Failed to deserialize the json user information result from Facebook.", result.Message);
            }
        }
    }

    // ReSharper restore InconsistentNaming
}