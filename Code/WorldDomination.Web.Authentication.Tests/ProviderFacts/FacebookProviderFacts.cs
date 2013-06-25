using System;
using System.Collections.Specialized;
using System.Net;
using Moq;
using RestSharp;
using WorldDomination.Web.Authentication.Providers;
using WorldDomination.Web.Authentication.Providers.Facebook;
using Xunit;

namespace WorldDomination.Web.Authentication.Tests.ProviderFacts
{
    // ReSharper disable InconsistentNaming

    public class FacebookProviderFacts
    {
        public class AuthenticateClientFacts
        {
            [Fact]
            public void GivenSomeErrorOccuredWhileTryingToRetrieveAccessToken_AuthenticateClient_ThrowsAnAuthenticationException()
            {
                // Arrange.
                var mockRestClient = new Mock<IRestClient>();
                mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>())).Returns(It.IsAny<IRestResponse>);
                var facebookProvider = new FacebookProvider(new ProviderParams { Key = "a", Secret = "b" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"state", existingState},
                    {"error_reason", "aaa"},
                    {"error", "bbb"},
                    {"error_description", "ccc"}
                };
                var facebookAuthenticationSettings = new FacebookAuthenticationServiceSettings
                {
                    State = existingState
                };

                // Act.
                var result =
                    Assert.Throws<AuthenticationException>(
                        () => facebookProvider.AuthenticateClient(facebookAuthenticationSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Reason: aaa. Error: bbb. Description: ccc.", result.Message);
            }

            [Fact]
            public void GivenNoValidAccessTokenParams_AuthenticateClient_ThrowsAnAuthenticationException()
            {
                // Arrange.
                var mockRestClient = new Mock<IRestClient>();
                mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>())).Returns(It.IsAny<IRestResponse>);
                var facebookProvider = new FacebookProvider(new ProviderParams { Key = "a", Secret = "b" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"state", existingState} // No code param.
                };
                var facebookAuthenticationSettings = new FacebookAuthenticationServiceSettings
                {
                    State = existingState
                };

                // Act.
                var result =
                    Assert.Throws<AuthenticationException>(
                        () => facebookProvider.AuthenticateClient(facebookAuthenticationSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("No code parameter provided in the response query string from Facebook.", result.Message);
            }

            [Fact]
            public void
                GivenAnExceptionOccursWhileTryingToRequestAnAccessToken_AuthenticateClient_ThrowsAnAuthenticationException()
            {
                // Arrange.
                const string exceptionMessage =
                    "1st World Problems: Too many rooms in my house. Can't decide where to sleep.";
                var mockRestClient = new Mock<IRestClient>();
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://www.whatever.pew.pew"));
                mockRestClient.Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                              .Throws(new Exception(exceptionMessage));
                var facebookProvider = new FacebookProvider(new ProviderParams { Key = "a", Secret = "b" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"state", existingState},
                    {"code", "whatever"}
                };
                var facebookAuthenticationSettings = new FacebookAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result =
                    Assert.Throws<AuthenticationException>(
                        () => facebookProvider.AuthenticateClient(facebookAuthenticationSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Failed to retrieve an Access Token from Facebook. 1st World Problems: Too many rooms in my house. Can't decide where to sleep.", result.RecursiveErrorMessages());
                Assert.NotNull(result.InnerException);
                Assert.Equal(exceptionMessage, result.InnerException.Message);
            }

            [Fact]
            public void GivenSomeInvalidRequestToken_AuthenticateClient_ThrowsAnAuthenticationException()
            {
                // Arrange.
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.Unauthorized);
                mockRestResponse.Setup(x => x.StatusDescription).Returns("Unauthorised");
                mockRestResponse.Setup(x => x.Content).Returns("{error:hi there asshat}");

                var mockRestClient = new Mock<IRestClient>();
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://www.whatever.pew.pew"));
                mockRestClient.Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                              .Returns(mockRestResponse.Object);

                var facebookProvider = new FacebookProvider(new ProviderParams { Key = "a", Secret = "b" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"state", existingState},
                    {"code", "whatever"}
                };
                var facebookAuthenticationSettings = new FacebookAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result =
                    Assert.Throws<AuthenticationException>(
                        () => facebookProvider.AuthenticateClient(facebookAuthenticationSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to obtain an Access Token from Facebook OR the the response was not an HTTP Status 200 OK. Response Status: Unauthorized. Response Description: Unauthorised. Error Content: {error:hi there asshat}. Error Message: --no error exception--.",
                    result.Message);
            }

            [Fact]
            public void GivenAMissingCallBackUriParam_AuthenticateClient_ThrowsAnAuthenticationException()
            {
                // Arrange.
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.BadRequest);
                mockRestResponse.Setup(x => x.StatusDescription).Returns("Bad Request");
                mockRestResponse.Setup(x => x.Content)
                                .Returns(
                                    "{\"error\":{\"message\":\"Missing redirect_uri parameter.\",\"type\":\"OAuthException\",\"code\":191}}");

                var mockRestClient = new Mock<IRestClient>();
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://www.whatever.pew.pew"));
                mockRestClient.Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                              .Returns(mockRestResponse.Object);

                var facebookProvider = new FacebookProvider(new ProviderParams { Key = "a", Secret = "b" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"state", existingState},
                    {"code", "whatever"}
                };
                var facebookAuthenticationSettings = new FacebookAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result =
                    Assert.Throws<AuthenticationException>(
                        () => facebookProvider.AuthenticateClient(facebookAuthenticationSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to obtain an Access Token from Facebook OR the the response was not an HTTP Status 200 OK. Response Status: BadRequest. Response Description: Bad Request. Error Content: {\"error\":{\"message\":\"Missing redirect_uri parameter.\",\"type\":\"OAuthException\",\"code\":191}}. Error Message: --no error exception--.",
                    result.Message);
            }

            [Fact]
            public void GivenAMissingExpiresParam_AuthenticateClient_ThrowsAnAuthenticationException()
            {
                // Arrange.
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponse.Setup(x => x.Data).Returns(new AccessTokenResult { access_token = "foo" });

                var mockRestClient = new Mock<IRestClient>();
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://www.fatchicksinpartyhats.com"));
                mockRestClient.Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()) )
                              .Returns(mockRestResponse.Object);

                var facebookProvider = new FacebookProvider(new ProviderParams { Key = "a", Secret = "b" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"state", existingState},
                    {"code", "whatever"}
                };
                var facebookAuthenticationSettings = new FacebookAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result =
                    Assert.Throws<AuthenticationException>(
                        () => facebookProvider.AuthenticateClient(facebookAuthenticationSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Retrieved a Facebook Access Token but there's an error with either the access_token and/or expires_on parameters. Access Token: foo. Expires In: 0.",
                    result.Message);
            }

            [Fact]
            public void
                GivenAValidAccessTokenButApiMeThrowsAnException_AuthenticateClient_ThrowsAnAuthenticationException()
            {
                // Arrange.
                const string exceptionMessage =
                    "1st World Problems: The Pizza guy arrived. Before I finished downloading the movie.";
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponse.Setup(x => x.Data).Returns(new AccessTokenResult
                {
                    access_token = "foo",
                    expires = 1000
                });

                var mockRestClient = new Mock<IRestClient>();
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://imgur.com/gallery/787lCwb"));
                mockRestClient.Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                              .Returns(mockRestResponse.Object);
                mockRestClient.Setup(x => x.Execute<MeResult>(It.IsAny<IRestRequest>()))
                              .Throws(new Exception(exceptionMessage));

                var facebookProvider = new FacebookProvider(new ProviderParams { Key = "a", Secret = "b" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"state", existingState},
                    {"code", "whatever"}
                };
                var facebookAuthenticationSettings = new FacebookAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result =
                    Assert.Throws<AuthenticationException>(
                        () => facebookProvider.AuthenticateClient(facebookAuthenticationSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Failed to retrieve any Me data from the Facebook Api. 1st World Problems: The Pizza guy arrived. Before I finished downloading the movie.", result.Message);
            }

            [Fact]
            public void GivenAnInvalidMeResultThrowsAnException_AuthenticateClient_ThrowsAnAuthenticationException()
            {
                // Arrange.
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponse.Setup(x => x.Data).Returns(new AccessTokenResult
                {
                    access_token = "foo",
                    expires = 1000
                });

                var mockRestResponseApiMe = new Mock<IRestResponse<MeResult>>();
                mockRestResponseApiMe.Setup(x => x.StatusCode).Returns(HttpStatusCode.Unauthorized);
                mockRestResponseApiMe.Setup(x => x.StatusDescription).Returns("Unauthorized");

                var mockRestClient = new Mock<IRestClient>();
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://imgur.com/gallery/787lCwb"));
                mockRestClient.Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                              .Returns(mockRestResponse.Object);
                mockRestClient.Setup(x => x.Execute<MeResult>(It.IsAny<IRestRequest>()))
                              .Returns(mockRestResponseApiMe.Object);

                var facebookProvider = new FacebookProvider(new ProviderParams { Key = "a", Secret = "b" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"state", existingState},
                    {"code", "whatever"}
                };
                var facebookAuthenticationSettings = new FacebookAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result =
                    Assert.Throws<AuthenticationException>(
                        () => facebookProvider.AuthenticateClient(facebookAuthenticationSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to obtain some 'Me' data from the Facebook api OR the the response was not an HTTP Status 200 OK. Response Status: Unauthorized. Response Description: Unauthorized. Error Message: --no error exception--.",
                    result.Message);
            }

            [Fact]
            public void GivenValidCredentials_AuthenticateClient_ReturnsAnAuthenticatedClientWithUserInformation()
            {
                // Arrange.
                var mockRestResponseAccessToken = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponseAccessToken.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponseAccessToken.Setup(x => x.Data).Returns(
                    new AccessTokenResult
                {
                    access_token = "foo",
                    expires = 1000
                });

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
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://imgur.com/gallery/787lCwb"));
                mockRestClient.Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                              .Returns(mockRestResponseAccessToken.Object);
                mockRestClient.Setup(x => x.Execute<MeResult>(It.IsAny<IRestRequest>()))
                              .Returns(mockRestResponseApiMe.Object);

                var facebookProvider = new FacebookProvider(new ProviderParams {Key = "a", Secret = "b"})
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"state", existingState},
                    {"code", "whatever"}
                };
                var facebookAuthenticationSettings = new FacebookAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result =
                    facebookProvider.AuthenticateClient(facebookAuthenticationSettings, queryStringParameters);

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("facebook", result.ProviderName);
                Assert.NotNull(result.AccessToken);
                Assert.NotNull(result.UserInformation);
                Assert.False(string.IsNullOrEmpty(result.UserInformation.Id));
                Assert.NotNull(result.UserInformation.Name);
                Assert.NotNull(result.UserInformation.UserName);
            }
        }

        public class RedirectToAuthenticateFacts
        {
            [Fact]
            public void GivenDefaultSettingsRequested_RedirectToAuthenticate_ReturnsAUri()
            {
                // Arrange.
                var facebookProvider = new FacebookProvider(new ProviderParams { Key = "aa", Secret = "bb" });
                var facebookAuthenticationServiceSettings = new FacebookAuthenticationServiceSettings
                {
                    CallBackUri = new Uri("http://www.2p1s.com")
                };

                // Act.
                var result = facebookProvider.RedirectToAuthenticate(facebookAuthenticationServiceSettings);

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "https://www.facebook.com/dialog/oauth?client_id=aa&scope=email&redirect_uri=http://www.2p1s.com/",
                    result.AbsoluteUri);
            }

            [Fact]
            public void GivenMobileAndDisplayAreRequested_RedirectToAuthenticate_ReturnsAUri()
            {
                // Arrange.
                var facebookProvider = new FacebookProvider(new ProviderParams { Key = "aa", Secret = "bb" });
                var facebookAuthenticationServiceSettings = new FacebookAuthenticationServiceSettings
                {
                    CallBackUri = new Uri("http://www.2p1s.com"),
                    IsMobile = true,
                    Display = DisplayType.Touch
                };
                // Act.
                var result = facebookProvider.RedirectToAuthenticate(facebookAuthenticationServiceSettings);

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "https://m.facebook.com/dialog/oauth?client_id=aa&scope=email&display=touch&redirect_uri=http://www.2p1s.com/",
                    result.AbsoluteUri);
            }
        }
    }

    // ReSharper restore InconsistentNaming
}