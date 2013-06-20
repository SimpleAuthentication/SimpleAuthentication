using System;
using System.Collections.Specialized;
using System.Net;
using Moq;
using RestSharp;
using WorldDomination.Web.Authentication.Providers;
using WorldDomination.Web.Authentication.Providers.Twitter;
using Xunit;

namespace WorldDomination.Web.Authentication.Tests.ProviderFacts
{
    // ReSharper disable InconsistentNaming

    public class TwitterProviderFacts
    {
        public class AuthenticateClientFacts
        {
            [Fact]
            public void GivenAUserDeniesAcceptingTheAppAuthorization_AuthenticateClient_ThrowsAnAuthenticationException()
            {
                // Arrange.
                var twitterProvider = new TwitterProvider(new ProviderParams { Key = "a", Secret = "b"});
                var nameValueCollection = new NameValueCollection
                {
                    {"denied", "JpQ7ZTt1nMeAhIypiOxLrkS3LreHwihKjsJcIJDf4To"}
                };
                // Act.
                var result =
                    Assert.Throws<AuthenticationException>(
                        () => twitterProvider.AuthenticateClient(new TwitterAuthenticationServiceSettings(), nameValueCollection));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to accept the Twitter App Authorization. Therefore, authentication didn't proceed.",
                    result.Message);
            }

            [Fact]
            public void GivenTheCallbackParamtersAreInvalid_AuthenticateClient_ThrowsAnAuthenticationException()
            {
                // Arrange.
                var mockRestClient = new Mock<IRestClient>();
                mockRestClient
                    .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                    .Throws<Exception>();
                var twitterProvider = new TwitterProvider(new ProviderParams {Key = "a", Secret = "b"})
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                var nameValueCollection = new NameValueCollection(); // Missing 2x required params.

                // Act.
                var result =
                    Assert.Throws<ArgumentOutOfRangeException>(
                        () => twitterProvider.AuthenticateClient(new TwitterAuthenticationServiceSettings(), nameValueCollection));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Specified argument was out of the range of valid values.\r\nParameter name: queryStringParameters",
                    result.Message);
            }

            [Fact]
            public void
                GivenExecutingARequestToRetrieveARequestTokenThrowsAnException_AuthenticateClient_ThrowsAnAuthenticationException
                ()
            {
                // Arrange.
                var mockRestClient = new Mock<IRestClient>();
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://i.dont.care.about.this.uri"));
                mockRestClient
                    .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                    .Throws(new Exception("one does not simply 'get a job'."));
                var twitterProvider = new TwitterProvider(new ProviderParams { Key = "a", Secret = "b" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                var nameValueCollection = new NameValueCollection
                {
                    {"oauth_token", "aaa"},
                    {"oauth_verifier", "bbbb"}
                };
                // Act.
                var result =
                    Assert.Throws<AuthenticationException>(
                        () => twitterProvider.AuthenticateClient(new TwitterAuthenticationServiceSettings(), nameValueCollection));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Failed to retrieve an oauth access token from Twitter. Error Messages: one does not simply 'get a job'.", result.Message);
                Assert.NotNull(result.InnerException);
                Assert.Equal("one does not simply 'get a job'.", result.InnerException.Message);
            }

            [Fact]
            public void GivenAnInvalidRequestToken_AuthenticateClient_ThrowsAnAuthenticationException()
            {
                // Arrange.
                var mockRestResponse = new Mock<IRestResponse>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.Unauthorized);
                mockRestResponse.Setup(x => x.StatusDescription).Returns("Unauthorized");
                
                var mockRestClient = new Mock<IRestClient>();
                mockRestClient
                    .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://i.dont.care.about.this.uri"));
                var twitterProvider = new TwitterProvider(new ProviderParams { Key = "a", Secret = "b" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                var nameValueCollection = new NameValueCollection
                {
                    {"oauth_token", "aaa"},
                    {"oauth_verifier", "bbbb"}
                };
                // Act.
                var result =
                    Assert.Throws<AuthenticationException>(
                        () => twitterProvider.AuthenticateClient(new TwitterAuthenticationServiceSettings(), nameValueCollection));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to obtain an Access Token from Twitter OR the the response was not an HTTP Status 200 OK. Response Status: Unauthorized. Response Description: Unauthorized. Error Content: . Error Message: --no error exception--.",
                    result.Message);
            }

            [Fact]
            public void GivenAnRequestTokenWithMissingParameters_AuthenticateClient_ThrowsAnAuthenticationException()
            {
                // Arrange.
                var mockRestResponse = new Mock<IRestResponse>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                
                var mockRestClient = new Mock<IRestClient>();
                mockRestClient
                    .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                var twitterProvider = new TwitterProvider(new ProviderParams { Key = "a", Secret = "b" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                var nameValueCollection = new NameValueCollection
                {
                    {"oauth_token", "aaa"}
                }; // Missing oauth_secret.
                // Act.
                var result =
                    Assert.Throws<AuthenticationException>(
                        () => twitterProvider.AuthenticateClient(new TwitterAuthenticationServiceSettings(), nameValueCollection));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to retrieve an oauth_token and an oauth_token_secret after the client has signed and approved via Twitter.",
                    result.Message);
            }

            [Fact]
            public void
                GivenExecutingVerifyCredentialsThrowsAnException_AuthenticateClient_ThrowsAnAuthenticationException()
            {
                // Arrange.
                const string exceptionMessage = "some mock exception.";
                var mockRestClient = new Mock<IRestClient>();
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://i.dont.care.about.this.uri"));

                var mockRestResponseRetrieveRequestToken = new Mock<IRestResponse>();
                mockRestResponseRetrieveRequestToken.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponseRetrieveRequestToken.Setup(x => x.Content)
                                                    .Returns("oauth_token=aaa&oauth_token_secret=ccc");

                var verifyCredentialsResult = new VerifyCredentialsResult
                {
                    Name = "Some Name",
                    Id = 1234,
                    Lang = "en-au",
                    ScreenName = "Some-Screen-Name"
                };
                var mockRestResponseVerifyCredentials = new Mock<IRestResponse<VerifyCredentialsResult>>();
                mockRestResponseVerifyCredentials.Setup(x => x.Data).Returns(verifyCredentialsResult);

                mockRestClient
                    .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponseRetrieveRequestToken.Object);
                mockRestClient
                    .Setup(x => x.Execute<VerifyCredentialsResult>(It.IsAny<IRestRequest>()))
                    .Throws(new Exception(exceptionMessage));

                var twitterProvider = new TwitterProvider(new ProviderParams { Key = "a", Secret = "b" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                var nameValueCollection = new NameValueCollection
                {
                    {"oauth_token", "aaa"},
                    {"oauth_verifier", "bbb"}
                };
                // Act.
                var result =
                    Assert.Throws<AuthenticationException>(
                        () => twitterProvider.AuthenticateClient(new TwitterAuthenticationServiceSettings(), nameValueCollection));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Failed to retrieve VerifyCredentials json data from the Twitter Api. Error Messages: some mock exception.", result.Message);
            }

            [Fact]
            public void GivenAnInvalidVerifyCredentials_RetrieveUserInformation_ThrowsAnAuthenticationException()
            {
                // Arrange.
                var mockRestResponseRetrieveRequestToken = new Mock<IRestResponse>();
                mockRestResponseRetrieveRequestToken.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponseRetrieveRequestToken.Setup(x => x.Content)
                                                    .Returns("oauth_token=aaa&oauth_token_secret=ccc");

                var mockRestResponseVerifyCredentials = new Mock<IRestResponse<VerifyCredentialsResult>>();
                mockRestResponseVerifyCredentials.Setup(x => x.StatusCode).Returns(HttpStatusCode.Unauthorized);
                mockRestResponseVerifyCredentials.Setup(x => x.StatusDescription).Returns("Unauthorized");

                var mockRestClient = new Mock<IRestClient>();
                mockRestClient
                    .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponseRetrieveRequestToken.Object);
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://i.dont.care.about.this.uri"));
                mockRestClient
                    .Setup(x => x.Execute<VerifyCredentialsResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponseVerifyCredentials.Object);

                var twitterProvider = new TwitterProvider(new ProviderParams { Key = "a", Secret = "b" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                var nameValueCollection = new NameValueCollection
                {
                    {"oauth_token", "aaa"},
                    {"oauth_verifier", "bbb"}
                };
                // Act.
                var result =
                    Assert.Throws<AuthenticationException>(
                        () => twitterProvider.AuthenticateClient(new TwitterAuthenticationServiceSettings(), nameValueCollection));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to obtain some VerifyCredentials json data from the Facebook api OR the the response was not an HTTP Status 200 OK. Response Status: Unauthorized. Response Description: Unauthorized. Error Message: --no error exception--.",
                    result.Message);
            }

            [Fact]
            public void GivenSomeValidVerifyCredentials_RetrieveUserInformation_ReturnsAnAuthenticatedClient()
            {
                // Arrange.
                var mockRestResponseRetrieveRequestToken = new Mock<IRestResponse>();
                mockRestResponseRetrieveRequestToken.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponseRetrieveRequestToken.Setup(x => x.Content)
                                                    .Returns("oauth_token=aaa&oauth_token_secret=ccc");

                var verifyCredentialsResult = new VerifyCredentialsResult
                {
                    Name = "Some Name",
                    Id = 1234,
                    Lang = "en-au",
                    ScreenName = "Some-Screen-Name"
                };
                var mockRestResponseVerifyCredentials = new Mock<IRestResponse<VerifyCredentialsResult>>();
                mockRestResponseVerifyCredentials.Setup(x => x.Data).Returns(verifyCredentialsResult);
                mockRestResponseVerifyCredentials.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);

                var mockRestClient = new Mock<IRestClient>();
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://i.dont.care.about.this.uri"));
                mockRestClient
                    .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponseRetrieveRequestToken.Object);
                mockRestClient
                    .Setup(x => x.Execute<VerifyCredentialsResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponseVerifyCredentials.Object);

                var twitterProvider = new TwitterProvider(new ProviderParams { Key = "a", Secret = "b" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                var nameValueCollection = new NameValueCollection
                {
                    {"oauth_token", "aaa"},
                    {"oauth_verifier", "bbb"}
                };

                // Act.
                var result = twitterProvider.AuthenticateClient(new TwitterAuthenticationServiceSettings(), nameValueCollection);

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("twitter", result.ProviderName);
                Assert.NotNull(result.UserInformation);
                Assert.NotNull(result.UserInformation.Id);
                Assert.NotNull(result.UserInformation.Locale);
                Assert.NotNull(result.UserInformation.Name);
                Assert.NotNull(result.UserInformation.UserName);
            }
        }

        public class RedirectToAuthenticateFacts
        {
            [Fact]
            public void GivenAValidRequestToken_RedirectToAuthenticate_ReturnsARedirectResult()
            {
                // Arrange.
                var redirectUri =
                    new Uri(
                        "http://api.twitter.com/oauth/authorize?oauth_token=5rlgo7AwGJt67vSoHmF227QMSHXhvmOmhN5YJpRlEo");
                var mockRestResponse = new Mock<IRestResponse>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponse
                    .Setup(x => x.Content)
                    .Returns("oauth_token=aaaaaaa&oauth_token_secret=asdasd");
                var mockRestClient = new Mock<IRestClient>();
                mockRestClient
                    .Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                    .Returns(redirectUri);
                mockRestClient
                    .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                var twitterProvider = new TwitterProvider(new ProviderParams { Key = "a", Secret = "b" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                var twitterAuthenticationServiceSettings = new TwitterAuthenticationServiceSettings
                {
                    CallBackUri = new Uri("http://www.pewpew.com"),
                    State = "I don't always test me code changes. But when I do, it's in production."
                };

                // Act.
                var result = twitterProvider.RedirectToAuthenticate(twitterAuthenticationServiceSettings);

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(result.AbsoluteUri, redirectUri.AbsoluteUri);
            }

            [Fact]
            public void GivenAnExceptionWhileTryingToRetrieveRequestToken_RedirectToAuthenticate_ThrowsAnAuthenticationException()
            {
                // Arrange.
                var mockRestClient = new Mock<IRestClient>();
                mockRestClient
                    .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                    .Throws(new Exception("some mock exception"));
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://i.dont.care.about.this.uri"));
                var twitterProvider = new TwitterProvider(new ProviderParams { Key = "a", Secret = "b" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                var twitterAuthenticationServiceSettings = new TwitterAuthenticationServiceSettings
                {
                    CallBackUri = new Uri("http://www.pewpew.com"),
                    State = "I don't always test me code changes. But when I do, it's in production."
                };
                // Act.
                var result =
                    Assert.Throws<AuthenticationException>(
                        () => twitterProvider.RedirectToAuthenticate(twitterAuthenticationServiceSettings));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Failed to obtain a Request Token from Twitter.", result.Message);
                Assert.NotNull(result.InnerException);
                Assert.Equal("some mock exception", result.InnerException.Message);
            }

            [Fact]
            public void GivenARequestTokenError_RedirectToAuthenticate_ThrowsAnAuthenticationException()
            {
                // Arrange.
                var mockRestResponse = new Mock<IRestResponse>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.Unauthorized);
                mockRestResponse.Setup(x => x.StatusDescription).Returns("Unauthorized");
                var mockRestClient = new Mock<IRestClient>();
                mockRestClient
                    .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://i.dont.care.about.this.uri"));
                var twitterProvider = new TwitterProvider(new ProviderParams { Key = "a", Secret = "b" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                var twitterAuthenticationServiceSettings = new TwitterAuthenticationServiceSettings
                {
                    CallBackUri = new Uri("http://www.pewpew.com"),
                    State = "I don't always test me code changes. But when I do, it's in production."
                };

                // Act.
                var result =
                    Assert.Throws<AuthenticationException>(
                        () => twitterProvider.RedirectToAuthenticate(twitterAuthenticationServiceSettings));

                // Assert.
                Assert.NotNull(result);
                Assert.Null(result.InnerException);
                Assert.Equal(
                    "Failed to obtain a request token from the Twitter api OR the the response was not an HTTP Status 200 OK. Response Status: Unauthorized. Response Description: Unauthorized. Error Message: --no error exception--.",
                    result.Message);
            }

            [Fact]
            public void GivenAInvalidRequestToken_RedirectToAuthenticate_ThrowsAnAuthenticationException()
            {
                // Arrange.
                var mockRestResponse = new Mock<IRestResponse>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponse.Setup(x => x.Content)
                                .Returns("oauth_token=aaaaaaa&missing_an_oauth_token_secret=asdasd");
                var mockRestClient = new Mock<IRestClient>();
                mockRestClient
                    .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://i.dont.care.about.this.uri"));
                var twitterProvider = new TwitterProvider(new ProviderParams { Key = "a", Secret = "b" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                var twitterAuthenticationServiceSettings = new TwitterAuthenticationServiceSettings
                {
                    CallBackUri = new Uri("http://www.pewpew.com"),
                    State = "I don't always test me code changes. But when I do, it's in production."
                };

                // Act.
                var result =
                    Assert.Throws<AuthenticationException>(
                        () => twitterProvider.RedirectToAuthenticate(twitterAuthenticationServiceSettings));

                // Assert.
                Assert.NotNull(result);
                Assert.Null(result.InnerException);
                Assert.Equal(
                    "Retrieved a Twitter Request Token but it doesn't contain both the oauth_token and oauth_token_secret parameters.",
                    result.Message);
            }
        }
    }

    // ReSharper restore InconsistentNaming
}