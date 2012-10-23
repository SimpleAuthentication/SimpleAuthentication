using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Moq;
using RestSharp;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Google;
using Xunit;

namespace WorldDomination.UnitTests
{
    // ReSharper disable InconsistentNaming

    public class GoogleProviderFacts
    {
        public class RedirectToAuthenticateFacts
        {
            [Fact]
            public void GivenSomeState_RedirectToAuthenticate_ReturnsAUri()
            {
                // Arrange.
                var googleProvider = new GoogleProvider("aa", "bb", new Uri("http://wwww.google.com"));

                // Act.
                var result = googleProvider.RedirectToAuthenticate("bleh");

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("https://accounts.google.com/o/oauth2/auth?client_id=aa&redirect_uri=http://wwww.google.com/&response_type=code&state=bleh&scope=https://www.googleapis.com/auth/userinfo.profile%20https://www.googleapis.com/auth/userinfo.email", result.AbsoluteUri);
            }
        }

        public class AuthenticateClientFacts
        {
            [Fact]
            public void GivenADifferentStateValue_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var googleProvider = new GoogleProvider("aa", "bb", new Uri("http://wwww.google.com"));
                var queryStringParameters = new NameValueCollection
                                            {
                                                {"code", "a"},
                                                {"state", "b"}
                                            };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => googleProvider.AuthenticateClient(queryStringParameters, "foo"));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("The states do not match. It's possible that you may be a victim of a CSRF.", result.Message);
            }

            [Fact]
            public void GivenGoogleReturnedAnError_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var googleProvider = new GoogleProvider("aa", "bb", new Uri("http://wwww.google.com"));
                const string existingState = "http://2p1s.com";
                var queryStringParameters = new NameValueCollection
                                            {
                                                {"error", "I dont' always use bayonets. But when I do, I transport them on Aircraft Carriers."},
                                                {"state", existingState}
                                            };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => googleProvider.AuthenticateClient(queryStringParameters, existingState));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Failed to retrieve an authorization code from Google. The error provided is: I dont' always use bayonets. But when I do, I transport them on Aircraft Carriers.", result.Message);
            }

            [Fact]
            public void GivenNoCodeAndNoErrorWasReturned_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var googleProvider = new GoogleProvider("aa", "bb", new Uri("http://wwww.google.com"));
                const string existingState = "http://2p1s.com";
                var queryStringParameters = new NameValueCollection
                                            {
                                                {"aaa", "bbb"},
                                                {"state", existingState}
                                            };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => googleProvider.AuthenticateClient(queryStringParameters, existingState));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("No code parameter provided in the response query string from Google.", result.Message);
            }

            [Fact]
            public void GivenAnErrorOccuredWhileTryingToRetrieveAnAccessToken_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var mockRestClient = new Mock<IRestClient>();
                const string errorMessage =
                    "If God says he was not created by a creator, does that mean: god is an aetheist?";
                mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                    .Throws(new InvalidOperationException(errorMessage));
                var googleProvider = new GoogleProvider("aa", "bb", new Uri("http://wwww.google.com"), null, mockRestClient.Object);
                const string existingState = "http://2p1s.com";
                var queryStringParameters = new NameValueCollection
                                            {
                                                {"code", "aaa"},
                                                {"state", existingState}
                                            };
                

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => googleProvider.AuthenticateClient(queryStringParameters, existingState));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Failed to obtain an Access Token from Google.", result.Message);
                Assert.NotNull(result.InnerException);
                Assert.Equal(errorMessage, result.InnerException.Message);
            }

            [Fact]
            public void GivenAnInvalidRequestToken_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var mockRestClient = new Mock<IRestClient>();
                var mockRestResponse = new Mock<IRestResponse>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.Unauthorized);
                mockRestResponse.Setup(x => x.StatusDescription).Returns("Unauthorized");
                mockRestClient
                    .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                var googleProvider = new GoogleProvider("aa", "bb", new Uri("http://wwww.google.com"), null, mockRestClient.Object);
                const string existingState = "http://2p1s.com";
                var queryStringParameters = new NameValueCollection
                                            {
                                                {"code", "aaa"},
                                                {"state", existingState}
                                            };


                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => googleProvider.AuthenticateClient(queryStringParameters, existingState));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Failed to obtain an Access Token from Google OR the the response was not an HTTP Status 200 OK. Response Status: Unauthorized. Response Description: Unauthorized", result.Message);
            }

            [Fact]
            public void GivenAnRequestTokenWithMissingParameters_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var mockRestClient = new Mock<IRestClient>();
                var mockRestResponse = new Mock<IRestResponse>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                //mockRestResponse.Setup(x => x.Content).Returns("{\"access_token\":\"1/fFAGRNJru1FTz70BzhT3Zg\",\"expires_in\":3920,\"token_type\":\"Bearer\"}");
                mockRestResponse.Setup(x => x.Content).Returns("{\"aaa\":\"bbb\",\"ccc\":ddd,\"eee\":\"ffff\"}");
                mockRestClient
                    .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                var googleProvider = new GoogleProvider("aa", "bb", new Uri("http://wwww.google.com"), null, mockRestClient.Object);
                const string existingState = "http://2p1s.com";
                var queryStringParameters = new NameValueCollection
                                            {
                                                {"code", "aaa"},
                                                {"state", existingState}
                                            };


                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => googleProvider.AuthenticateClient(queryStringParameters, existingState));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Retrieved a Google Access Token but it doesn't contain one or more of either: access_token, expires_in or token_type", result.Message);
            }

            [Fact]
            public void GivenExecutingUserInfoThrowsAnException_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var mockRestClient = new Mock<IRestClient>();
                var mockRestResponse = new Mock<IRestResponse>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponse.Setup(x => x.Content).Returns("\"access_token\":\"1/fFAGRNJru1FTz70BzhT3Zg\",\"expires_in\":3920,\"token_type\":\"Bearer\"");
                mockRestClient
                    .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                var googleProvider = new GoogleProvider("aa", "bb", new Uri("http://wwww.google.com"), null, mockRestClient.Object);
                const string existingState = "http://2p1s.com";
                var queryStringParameters = new NameValueCollection
                                            {
                                                {"code", "aaa"},
                                                {"state", existingState}
                                            };


                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => googleProvider.AuthenticateClient(queryStringParameters, existingState));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Retrieved a Google Access Token but it doesn't contain one or more of either: access_token, expires_in or token_type", result.Message);
            }
        }
    }

    // ReSharper restore InconsistentNaming
}
