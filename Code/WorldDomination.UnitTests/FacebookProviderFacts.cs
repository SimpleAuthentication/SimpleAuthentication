using System;
using System.Net;
using Moq;
using RestSharp;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Facebook;
using Xunit;

namespace WorldDomination.UnitTests
{
    // ReSharper disable InconsistentNaming

    public class FacebookProviderFacts
    {
        public class RetrieveUserInformationFacts
        {
            [Fact]
            public void GivenAnExceptionOccursWhileTryingToRequestAnAccessToken_RetrieveUserInformation_ThrowsAnException()
            {
                // Arrange.
                const string exceptionMessage = "Some mock exception.";
                var mockRestClient = new Mock<IRestClient>();
                mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                    .Throws(new Exception(exceptionMessage));
                var facebookProvider = new FacebookProvider("a", "b", new Uri("http://www.google.com"),
                                                            mockRestClient.Object);
                var facebookClient = new FacebookClient
                {
                    Code = "aa",
                    State = "bb"
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(() => facebookProvider.RetrieveUserInformation(facebookClient));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Failed to retrieve an oauth access token from Facebook.", result.Message);
                Assert.NotNull(result.InnerException);
                Assert.Equal(exceptionMessage, result.InnerException.Message);
            }

            [Fact]
            public void GivenSomeInvalidRequestToken_RetrieveUserInformation_ThrowsAnException()
            {
                // Arrange.
                var mockRestResponse = new Mock<IRestResponse>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.Unauthorized);
                mockRestResponse.Setup(x => x.StatusDescription).Returns("Unauthorised");

                var mockRestClient = new Mock<IRestClient>();
                mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);

                var facebookProvider = new FacebookProvider("a", "b", new Uri("http://www.google.com"),
                                                            mockRestClient.Object);
                var facebookClient = new FacebookClient
                {
                    Code = "aa",
                    State = "bb"
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(() => facebookProvider.RetrieveUserInformation(facebookClient));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Failed to obtain an Access Token from Facebook OR the the response was not an HTTP Status 200 OK. Response Status: Unauthorized. Response Description: Unauthorised", result.Message);
            }

            [Fact]
            public void GivenAMissingExpiresParam_RetrieveUserInformation_ThrowsAnException()
            {
                // Arrange.
                var mockRestResponse = new Mock<IRestResponse>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponse.Setup(x => x.Content).Returns("access_token=foo&omg=pewpew");

                var mockRestClient = new Mock<IRestClient>();
                mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);

                var facebookProvider = new FacebookProvider("a", "b", new Uri("http://www.google.com"),
                                                            mockRestClient.Object);
                var facebookClient = new FacebookClient
                {
                    Code = "aa",
                    State = "bb"
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(() => facebookProvider.RetrieveUserInformation(facebookClient));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Retrieved a Facebook Access Token but it doesn't contain both the access_token and expires_on parameters.", result.Message);
            }

            [Fact]
            public void GivenAValidAccessTokenButApiMeThrowsAnException_RetrieveUserInformation_ThrowsAnException()
            {
                // Arrange.
                const string exceptionMessage = "Some mock exception message.";
                var mockRestResponse = new Mock<IRestResponse>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponse.Setup(x => x.Content).Returns("access_token=foo&expires_on=1000");

                var mockRestClient = new Mock<IRestClient>();
                mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                mockRestClient.Setup(x => x.Execute<MeResult>(It.IsAny<IRestRequest>()))
                    .Throws(new Exception(exceptionMessage));

                var facebookProvider = new FacebookProvider("a", "b", new Uri("http://www.google.com"),
                                                            mockRestClient.Object);
                var facebookClient = new FacebookClient
                {
                    Code = "aa",
                    State = "bb"
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(() => facebookProvider.RetrieveUserInformation(facebookClient));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Failed to retrieve any Me data from the Facebook Api.", result.Message);
                Assert.NotNull(result.InnerException);
                Assert.Equal(exceptionMessage, result.InnerException.Message);
            }

            [Fact]
            public void GivenAnInvalidMeResultThrowsAnException_RetrieveUserInformation_ThrowsAnException()
            {
                // Arrange.
                var mockRestResponse = new Mock<IRestResponse>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponse.Setup(x => x.Content).Returns("access_token=foo&expires_on=1000");

                var mockRestResponseApiMe = new Mock<IRestResponse<MeResult>>();
                mockRestResponseApiMe.Setup(x => x.StatusCode).Returns(HttpStatusCode.Unauthorized);
                mockRestResponseApiMe.Setup(x => x.StatusDescription).Returns("Unauthorized");

                var mockRestClient = new Mock<IRestClient>();
                mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                mockRestClient.Setup(x => x.Execute<MeResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponseApiMe.Object);

                var facebookProvider = new FacebookProvider("a", "b", new Uri("http://www.google.com"),
                                                            mockRestClient.Object);
                var facebookClient = new FacebookClient
                {
                    Code = "aa",
                    State = "bb"
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(() => facebookProvider.RetrieveUserInformation(facebookClient));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Failed to obtain some Me data from the Facebook api OR the the response was not an HTTP Status 200 OK. Response Status: Unauthorized. Response Description: Unauthorized", result.Message);
            }

            [Fact]
            public void GivenValidCredentials_RetrieveUserInformation_ReturnsAnAccessTokenAndUserInformation()
            {
                // Arrange.
                var mockRestResponseAccessToken = new Mock<IRestResponse>();
                mockRestResponseAccessToken.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponseAccessToken.Setup(x => x.Content).Returns("access_token=foo&expires_on=1000");

                var meResult = new MeResult
                               {
                                   Id = 1,
                                   FirstName = "some firstname",
                                   LastName = "some lastname",
                                   Link = "http://whatever",
                                   Locale = "en-au",
                                   Name = "Hi there",
                                   Timezone = 10,
                                   Username = "PewPew",
                                   Verified = true
                               };

                var mockRestResponseApiMe = new Mock<IRestResponse<MeResult>>();
                mockRestResponseApiMe.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponseApiMe.Setup(x => x.Data).Returns(meResult);

                var mockRestClient = new Mock<IRestClient>();
                mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponseAccessToken.Object);
                mockRestClient.Setup(x => x.Execute<MeResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponseApiMe.Object);

                var facebookProvider = new FacebookProvider("a", "b", new Uri("http://www.google.com"),
                                                            mockRestClient.Object);
                var facebookClient = new FacebookClient
                {
                    Code = "aa",
                    State = "bb"
                };

                // Act.
                facebookProvider.RetrieveUserInformation(facebookClient);

                // Assert.
                Assert.NotNull(facebookClient);
                Assert.NotNull(facebookClient.AccessToken);
                Assert.NotNull(facebookClient.Code);
                Assert.NotNull(facebookClient.UserInformation);
                Assert.True(facebookClient.UserInformation.Id > 0);
                Assert.NotNull(facebookClient.UserInformation.Name);
                Assert.NotNull(facebookClient.UserInformation.UserName);
            }

        }
    }

    // ReSharper restore InconsistentNaming
}