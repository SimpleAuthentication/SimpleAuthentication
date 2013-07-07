using System;
using System.Collections.Specialized;
using System.Net;
using Moq;
using RestSharp;
using WorldDomination.Web.Authentication.ExtraProviders;
using WorldDomination.Web.Authentication.ExtraProviders.LinkedIn;
using Xunit;

namespace WorldDomination.Web.Authentication.Tests.ProviderFacts
{
    // ReSharper disable InconsistentNaming

    public class LinkedInProviderFacts
    {
        public class AuthenticateClientFacts
        {
            [Fact]
            public void GivenLinkedInReturnedAnError_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var linkedinProvider = new LinkedInProvider(new ProviderParams {Key = "aa", Secret = "bb"});
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {
                        "error",
                        "I dont' always use bayonets. But when I do, I transport them on Aircraft Carriers."
                    },
                    {"state", existingState}
                };
                var linkedInAuthenticationServiceSettings = new LinkedInAuthenticationServiceSettings
                {
                    State = existingState
                };
                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () =>
                    linkedinProvider.AuthenticateClient(linkedInAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to retrieve an authorization code from LinkedIn. The error provided is: I dont' always use bayonets. But when I do, I transport them on Aircraft Carriers.",
                    result.Message);
            }

            [Fact]
            public void GivenNoCodeAndNoErrorWasReturned_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var linkedinProvider = new LinkedInProvider(new ProviderParams {Key = "aa", Secret = "bb"});
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"aaa", "bbb"},
                    {"state", existingState}
                };
                var linkedInAuthenticationServiceSettings = new LinkedInAuthenticationServiceSettings
                {
                    State = existingState
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () =>
                    linkedinProvider.AuthenticateClient(linkedInAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("No code parameter provided in the response query string from LinkedIn.", result.Message);
            }

            [Fact]
            public void GivenANullCallbackUriWhileTryingToRetrieveAnAccessToken_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var mockRestClient = new Mock<IRestClient>();
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.BadRequest);
                mockRestResponse.Setup(x => x.StatusDescription).Returns("Bad Request");
                mockRestResponse.Setup(x => x.Content).Returns("{\n  \"error\" : \"invalid_request\"\n}");
                mockRestClient
                    .Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://www.windowsazure.com"));
                var linkedinProvider = new LinkedInProvider(new ProviderParams {Key = "aa", Secret = "bb"})
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };
                var linkedInAuthenticationServiceSettings = new LinkedInAuthenticationServiceSettings
                {
                    CallBackUri = new Uri("http://2p1s.com"),
                    State = existingState
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () =>
                    linkedinProvider.AuthenticateClient(linkedInAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to obtain an Access Token from LinkedIn OR the the response was not an HTTP Status 200 OK. Response Status: BadRequest. Response Description: Bad Request. Error Content: {\n  \"error\" : \"invalid_request\"\n}. Error Message: --no error exception--.",
                    result.Message);
            }

            [Fact]
            public void GivenAnErrorOccuredWhileTryingToRetrieveAnAccessToken_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var mockRestClient = new Mock<IRestClient>();
                const string errorMessage =
                    "If God says he was not created by a creator, does that mean: god is an aetheist?";
                mockRestClient.Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                              .Throws(new InvalidOperationException(errorMessage));
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://www.windowsazure.com"));
                var linkedinProvider = new LinkedInProvider(new ProviderParams {Key = "aa", Secret = "bb"})
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };
                var linkedInAuthenticationServiceSettings = new LinkedInAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () =>
                    linkedinProvider.AuthenticateClient(linkedInAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to retrieve an Access Token from LinkedIn.",
                    result.Message);
                Assert.NotNull(result.InnerException);
                Assert.Equal(errorMessage, result.InnerException.Message);
            }

            [Fact]
            public void GivenAnInvalidRequestToken_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var mockRestClient = new Mock<IRestClient>();
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.Unauthorized);
                mockRestResponse.Setup(x => x.StatusDescription).Returns("Unauthorized");
                mockRestClient
                    .Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://www.windowsazure.com"));
                var linkedinProvider = new LinkedInProvider(new ProviderParams {Key = "aa", Secret = "bb"})
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };
                var linkedInAuthenticationServiceSettings = new LinkedInAuthenticationServiceSettings
                {
                    CallBackUri = new Uri("http://2p1s.com"),
                    State = existingState
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () =>
                    linkedinProvider.AuthenticateClient(linkedInAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to obtain an Access Token from LinkedIn OR the the response was not an HTTP Status 200 OK. Response Status: Unauthorized. Response Description: Unauthorized. Error Content: . Error Message: --no error exception--.",
                    result.Message);
            }

            [Fact]
            public void GivenAnRequestTokenWithMissingParameters_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var mockRestClient = new Mock<IRestClient>();
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponse.Setup(x => x.Data).Returns(new AccessTokenResult());
                mockRestClient
                    .Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://www.windowsazure.com"));
                var linkedinProvider = new LinkedInProvider(new ProviderParams {Key = "aa", Secret = "bb"})
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };
                var linkedInAuthenticationServiceSettings = new LinkedInAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () =>
                    linkedinProvider.AuthenticateClient(linkedInAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Retrieved a LinkedIn Access Token but it doesn't contain one or more of either: oauth2_access_token or expires_in.",
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
                    ExpiresIn = 10
                });

                var mockRestResponseUserInfo = new Mock<IRestResponse<UserInfoResult>>();
                mockRestResponseUserInfo.Setup(x => x.StatusCode).Returns(HttpStatusCode.Unauthorized);
                mockRestResponseUserInfo.Setup(x => x.StatusDescription).Returns("Unauthorized");

                var mockRestClient = new Mock<IRestClient>();
                mockRestClient
                    .Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                mockRestClient.
                    Setup(x => x.Execute<UserInfoResult>(It.IsAny<IRestRequest>()))
                              .Returns(mockRestResponseUserInfo.Object);
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://www.windowsazure.com"));

                var linkedinProvider = new LinkedInProvider(new ProviderParams {Key = "aa", Secret = "bb"})
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };
                var linkedInAuthenticationServiceSettings = new LinkedInAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () =>
                    linkedinProvider.AuthenticateClient(linkedInAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to obtain some UserInfo data from the LinkedIn Api OR the the response was not an HTTP Status 200 OK. Response Status: Unauthorized. Response Description: Unauthorized. Error Message: --no error exception--.",
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
                    ExpiresIn = 10
                });

                var mockRestResponseUserInfo = new Mock<IRestResponse<UserInfoResult>>();
                mockRestResponseUserInfo.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponseUserInfo.Setup(x => x.Data).Returns(new UserInfoResult()); // Missing required info.

                var mockRestClient = new Mock<IRestClient>();
                mockRestClient
                    .Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                mockRestClient.
                    Setup(x => x.Execute<UserInfoResult>(It.IsAny<IRestRequest>()))
                              .Returns(mockRestResponseUserInfo.Object);
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://www.windowsazure.com"));

                var linkedinProvider = new LinkedInProvider(new ProviderParams {Key = "aa", Secret = "bb"})
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };
                var linkedInAuthenticationServiceSettings = new LinkedInAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () =>
                    linkedinProvider.AuthenticateClient(linkedInAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "We were unable to retrieve the User Id from LinkedIn API, the user may have denied the authorization.",
                    result.Message);
            }

            [Fact]
            public void GivenExecutingRetrieveSomeUserInfo_AuthenticateClient_ReturnsAnAuthenticatedClient()
            {
                // Arrange.
                const string accessToken = "aaa";
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                var accessTokenResult = new AccessTokenResult
                {
                    AccessToken = accessToken,
                    ExpiresIn = 10
                };
                mockRestResponse.Setup(x => x.Data).Returns(accessTokenResult);

                var userInfoResult = new UserInfoResult
                {
                    EmailAddress = "asdasdas",
                    Id = "1",
                    FirstName = "AAAA",
                    LastName = "bbbbbb"
                };
                var mockRestResponseUserInfo = new Mock<IRestResponse<UserInfoResult>>();
                mockRestResponseUserInfo.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponseUserInfo.Setup(x => x.Data).Returns(userInfoResult);

                var mockRestClient = new Mock<IRestClient>();
                mockRestClient
                    .Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                mockRestClient.
                    Setup(x => x.Execute<UserInfoResult>(It.IsAny<IRestRequest>()))
                              .Returns(mockRestResponseUserInfo.Object);
                mockRestClient.Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
                              .Returns(new Uri("http://www.windowsazure.com"));

                var linkedinProvider = new LinkedInProvider(new ProviderParams {Key = "aa", Secret = "bb"})
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";

                var queryStringParameters = new NameValueCollection
                {
                    {"code", accessToken},
                    {"state", existingState}
                };
                var linkedInAuthenticationServiceSettings = new LinkedInAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result = linkedinProvider.AuthenticateClient(linkedInAuthenticationServiceSettings,
                                                                 queryStringParameters);

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("linkedin", result.ProviderName);
                Assert.NotNull(result.AccessToken);
                Assert.Equal(accessToken, result.AccessToken.PublicToken);
                Assert.True(DateTime.UtcNow <= result.AccessToken.ExpiresOn);
                Assert.NotNull(result.UserInformation);
                Assert.Equal(GenderType.Unknown, result.UserInformation.Gender);
                Assert.Equal(userInfoResult.Id, result.UserInformation.Id);
                Assert.Equal(userInfoResult.FirstName + " " + userInfoResult.LastName, result.UserInformation.Name);
            }
        }

        public class RedirectToAuthenticateFacts
        {
            [Fact]
            public void GivenSomeState_RedirectToAuthenticate_ReturnsAUri()
            {
                // Arrange.
                var linkedinProvider = new LinkedInProvider(new ProviderParams {Key = "aa", Secret = "bb"});

                // Act.
                var result =
                    linkedinProvider.RedirectToAuthenticate(new LinkedInAuthenticationServiceSettings
                    {
                        CallBackUri =
                            new Uri("http://wwww.pewpew.com/"),
                        State = "bleh"
                    });

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "https://www.linkedin.com/uas/oauth2/authorization?response_type=code&client_id=aa&redirect_uri=http://wwww.pewpew.com/&scope=r_basicprofile%20r_emailaddress&state=bleh",
                    result.AbsoluteUri);
            }
        }
    }

    // ReSharper restore InconsistentNaming
}