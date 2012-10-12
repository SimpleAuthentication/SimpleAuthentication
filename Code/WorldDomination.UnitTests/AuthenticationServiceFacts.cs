using System;
using System.Collections.Specialized;
using System.Web;
using Moq;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Facebook;
using Xunit;

namespace WorldDomination.UnitTests
{
    // ReSharper disable InconsistentNaming

    public class AuthenticationServiceFacts
    {
        #region Nested type: CheckCallbackFacts

        public class CheckCallbackFacts
        {
            [Fact]
            public void GivenAFacebookCallback_CheckCallback_RetrievesAnAccessTokenAnUserData()
            {
                // Arrange.
                const string state = "blah";
                var mockWebClientWrapper = MoqUtilities.MockedIWebClientWrapper();
                var facebookProvider = new FacebookProvider("a", "b", new Uri("http://www.google.com"),
                                                            mockWebClientWrapper.Object);
                var authenticationService = new AuthenticationService(facebookProvider);
                var mockRequestBase = new Mock<HttpRequestBase>();
                mockRequestBase.Setup(x => x.Params).Returns(new NameValueCollection
                                                                 {
                                                                     {"code", "aaa"},
                                                                     {"state", state}
                                                                 });

                // Act.
                var facebookClient =
                    authenticationService.CheckCallback(mockRequestBase.Object, state) as FacebookClient;

                // Assert.
                Assert.NotNull(facebookClient);
                Assert.NotNull(facebookClient.AccessToken);
                Assert.NotNull(facebookClient.UserInformation);
                Assert.NotNull(facebookClient.UserInformation.Id);
            }
        }

        #endregion
    }

    // ReSharper restore InconsistentNaming
}