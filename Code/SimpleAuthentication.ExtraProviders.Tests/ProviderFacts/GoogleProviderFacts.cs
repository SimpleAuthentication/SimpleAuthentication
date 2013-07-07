using System;
using System.Collections.Specialized;
using System.Net;
using Moq;
using RestSharp;
using WorldDomination.Web.Authentication.Providers;
using WorldDomination.Web.Authentication.Providers.Google;
using Xunit;

namespace WorldDomination.Web.Authentication.Tests.ProviderFacts
{
    // ReSharper disable InconsistentNaming

    public class GoogleProviderFacts
    {
        private static Mock<IRestClient> MockRestClient
        {
            get
            {
                var mockRestClient = new Mock<IRestClient>();
                //mockRestClient.Setup(x => x.BaseUrl).Returns("http://www.whatever.com/");
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://www.whatever.pew.pew"));
                mockRestClient.Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                    .Returns(It.IsAny<IRestResponse<AccessTokenResult>>);

                return mockRestClient;
            }
        }

        public class AuthenticateClientFacts
        {
            [Fact]
            public void GivenGoogleReturnedAnError_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var googleProvider = new GoogleProvider(new ProviderParams {Key = "aa", Secret = "bb"});
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {
                        "error",
                        "I dont' always use bayonets. But when I do, I transport them on Aircraft Carriers."
                    },
                    {"state", existingState}
                };
                var googleAuthenticationServiceSettings = new GoogleAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };
                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => googleProvider.AuthenticateClient(googleAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to retrieve an authorization code from Google. The error provided is: I dont' always use bayonets. But when I do, I transport them on Aircraft Carriers.",
                    result.Message);
            }

            [Fact]
            public void GivenNoCodeAndNoErrorWasReturned_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var googleProvider = new GoogleProvider(new ProviderParams { Key = "aa", Secret = "bb" });
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"aaa", "bbb"},
                    {"state", existingState}
                };
                var googleAuthenticationServiceSettings = new GoogleAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => googleProvider.AuthenticateClient(googleAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("No code parameter provided in the response query string from Google.", result.Message);
            }

            [Fact]
            public void GivenANullCallbackUriWhileTryingToRetrieveAnAccessToken_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.BadRequest);
                mockRestResponse.Setup(x => x.StatusDescription).Returns("Bad Request");
                mockRestResponse.Setup(x => x.Content).Returns("{\n  \"error\" : \"invalid_request\"\n}");
                var mockRestClient = MockRestClient;
                mockRestClient
                    .Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                var googleProvider = new GoogleProvider(new ProviderParams {Key = "aa", Secret = "bb"})
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                                                        
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };
                var googleAuthenticationServiceSettings = new GoogleAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => googleProvider.AuthenticateClient(googleAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to obtain an Access Token from Google OR the the response was not an HTTP Status 200 OK. Response Status: BadRequest. Response Description: Bad Request. Error Content: {\n  \"error\" : \"invalid_request\"\n}. Error Message: --no error exception--.",
                    result.Message);
            }

            [Fact]
            public void GivenAnErrorOccuredWhileTryingToRetrieveAnAccessToken_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                const string errorMessage =
                    "If God says he was not created by a creator, does that mean: god is an aetheist?"; 
                var mockRestClient = MockRestClient;
                mockRestClient.Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                              .Throws(new InvalidOperationException(errorMessage));
                var googleProvider = new GoogleProvider(new ProviderParams {Key = "aa", Secret = "bb"})
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };
                var googleAuthenticationServiceSettings = new GoogleAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => googleProvider.AuthenticateClient(googleAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Failed to retrieve an Access Token from Google.", result.Message);
                Assert.NotNull(result.InnerException);
                Assert.Equal(errorMessage, result.InnerException.RecursiveErrorMessages());
            }

            [Fact]
            public void GivenAnInvalidRequestToken_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.Unauthorized);
                mockRestResponse.Setup(x => x.StatusDescription).Returns("Unauthorized");
                var mockRestClient = MockRestClient;
                mockRestClient
                    .Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                var googleProvider = new GoogleProvider(new ProviderParams {Key = "aa", Secret = "bb"})
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };
                var googleAuthenticationServiceSettings = new GoogleAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => googleProvider.AuthenticateClient(googleAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Failed to obtain an Access Token from Google OR the the response was not an HTTP Status 200 OK. Response Status: Unauthorized. Response Description: Unauthorized. Error Content: . Error Message: --no error exception--.", result.Message);
                Assert.Null(result.InnerException);
            }

            [Fact]
            public void GivenAnRequestTokenWithMissingParameters_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponse.Setup(x => x.Data).Returns(new AccessTokenResult());
                var mockRestClient = MockRestClient;
                mockRestClient
                    .Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                var googleProvider = new GoogleProvider(new ProviderParams {Key = "aa", Secret = "bb"})
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };
                var googleAuthenticationServiceSettings = new GoogleAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => googleProvider.AuthenticateClient(googleAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Retrieved a Google Access Token but it doesn't contain one or more of either: access_token, expires_in or token_type.",
                    result.Message);
            }

            [Fact]
            public void GivenExecutingUserInfoThrowsAnException_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponse.Setup(x => x.Data).Returns(new AccessTokenResult
                {
                    AccessToken = "aaa",
                    ExpiresIn = 100,
                    IdToken =
                        "What if that sexy girl in that pop up chat really does want to meet people in my area?",
                    TokenType = "overly attached girlfriend"
                });

                var mockRestResponseUserInfo = new Mock<IRestResponse<UserInfoResult>>();
                mockRestResponseUserInfo.Setup(x => x.StatusCode).Returns(HttpStatusCode.Unauthorized);
                mockRestResponseUserInfo.Setup(x => x.StatusDescription).Returns("Unauthorized");

                var mockRestClient = MockRestClient;
                mockRestClient
                    .Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                mockRestClient.
                    Setup(x => x.Execute<UserInfoResult>(It.IsAny<IRestRequest>()))
                              .Returns(mockRestResponseUserInfo.Object);

                var googleProvider = new GoogleProvider(new ProviderParams {Key = "aa", Secret = "bb"})
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };
                var googleAuthenticationServiceSettings = new GoogleAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => googleProvider.AuthenticateClient(googleAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to obtain some UserInfo data from the Google Api OR the the response was not an HTTP Status 200 OK. Response Status: Unauthorized. Response Description: Unauthorized. Error Message: --no error exception--.",
                    result.Message);
            }

            [Fact]
            public void GivenExecutingUserInfoWorksButIsMissingSomeRequiredData_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponse.Setup(x => x.Data).Returns(new AccessTokenResult
                {
                    AccessToken = "aaa",
                    ExpiresIn = 100,
                    IdToken =
                        "What if that sexy girl in that pop up chat really does want to meet people in my area?",
                    TokenType = "overly attached girlfriend"
                });

                var mockRestResponseUserInfo = new Mock<IRestResponse<UserInfoResult>>();
                mockRestResponseUserInfo.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponseUserInfo.Setup(x => x.Data).Returns(new UserInfoResult()); // Missing required info.

                var mockRestClient = MockRestClient;
                mockRestClient
                    .Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                mockRestClient.
                    Setup(x => x.Execute<UserInfoResult>(It.IsAny<IRestRequest>()))
                              .Returns(mockRestResponseUserInfo.Object);

                var googleProvider = new GoogleProvider(new ProviderParams {Key = "aa", Secret = "bb"})
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };
                var googleAuthenticationServiceSettings = new GoogleAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => googleProvider.AuthenticateClient(googleAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "We were unable to retrieve the User Id from Google API, the user may have denied the authorization.",
                    result.Message);
            }

            [Fact]
            public void GivenExecutingRetrieveSomeUserInfo_AuthenticateClient_ReturnsAnAuthenticatedClient()
            {
                // Arrange.
                const string accessToken = "aaa";
                const int expiresIn = 100;
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponse.Setup(x => x.Data).Returns(new AccessTokenResult
                {
                    AccessToken = accessToken,
                    ExpiresIn = expiresIn,
                    IdToken =
                        "What if that sexy girl in that pop up chat really does want to meet people in my area?",
                    TokenType = "overly attached girlfriend"
                });

                var userInfoResult = new UserInfoResult
                {
                    Email = "aaa",
                    FamilyName = "bbb",
                    Gender = "male",
                    GivenName = "ccc",
                    Id = "ddd",
                    Link = "http://2p1s.com",
                    Locale = "en-au",
                    Name = "eee",
                    Picture = "http://2p1s.com/zomg",
                    VerifiedEmail = true
                };
                var mockRestResponseUserInfo = new Mock<IRestResponse<UserInfoResult>>();
                mockRestResponseUserInfo.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponseUserInfo.Setup(x => x.Data).Returns(userInfoResult);

                var mockRestClient = MockRestClient;
                mockRestClient
                    .Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);

                mockRestClient.
                    Setup(x => x.Execute<UserInfoResult>(It.IsAny<IRestRequest>()))
                              .Returns(mockRestResponseUserInfo.Object);

                var googleProvider = new GoogleProvider(new ProviderParams {Key = "aa", Secret = "bb"})
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";

                var queryStringParameters = new NameValueCollection
                {
                    {"code", accessToken},
                    {"state", existingState}
                };
                var googleAuthenticationServiceSettings = new GoogleAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result = googleProvider.AuthenticateClient(googleAuthenticationServiceSettings, queryStringParameters);

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("google", result.ProviderName);
                Assert.NotNull(result.AccessToken);
                Assert.Equal(accessToken, result.AccessToken.PublicToken);
                Assert.True(DateTime.UtcNow.AddSeconds(expiresIn) >= result.AccessToken.ExpiresOn);
                Assert.NotNull(result.UserInformation);
                Assert.Equal(GenderType.Male, result.UserInformation.Gender);
                Assert.Equal(userInfoResult.Id, result.UserInformation.Id);
                Assert.Equal(userInfoResult.Locale, result.UserInformation.Locale);
                Assert.Equal(userInfoResult.Name, result.UserInformation.Name);
                Assert.Equal(userInfoResult.Picture, result.UserInformation.Picture);
                Assert.Equal(userInfoResult.GivenName, result.UserInformation.UserName);
            }
        }

        public class RedirectToAuthenticateFacts
        {
            [Fact]
            public void GivenSomeState_RedirectToAuthenticate_ReturnsAUri()
            {
                // Arrange.
                var googleProvider = new GoogleProvider(new ProviderParams {Key = "aa", Secret = "bb"});

                // Act.
                var result =
                    googleProvider.RedirectToAuthenticate(new GoogleAuthenticationServiceSettings
                    {
                        State = "bleh",
                        CallBackUri = new Uri("http://2p1s.com")
                    });

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "https://accounts.google.com/o/oauth2/auth?client_id=aa&redirect_uri=http://2p1s.com/&response_type=code&scope=https://www.googleapis.com/auth/userinfo.profile%20https://www.googleapis.com/auth/userinfo.email&state=bleh",
                    result.AbsoluteUri);
            }
        }
    }

    // ReSharper restore InconsistentNaming
}