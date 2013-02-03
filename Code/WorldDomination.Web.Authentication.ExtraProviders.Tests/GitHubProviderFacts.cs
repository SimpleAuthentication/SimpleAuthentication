﻿using System;
using System.Collections.Specialized;
using System.Net;
using Moq;
using RestSharp;
using WorldDomination.Web.Authentication.ExtraProviders.GitHub;
using Xunit;

namespace WorldDomination.Web.Authentication.ExtraProviders.Tests
{
    // ReSharper disable InconsistentNaming

    public class GitHubProviderFacts
    {
        public class AuthenticateClientFacts
        {
            [Fact]
            public void GivenADifferentStateValue_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var githubProvider = new GitHubProvider("aa", "bb", null);
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "a"},
                    {"state", "b"}
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => githubProvider.AuthenticateClient(queryStringParameters, "foo"));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("The states do not match. It's possible that you may be a victim of a CSRF.",
                             result.Message);
            }

            [Fact]
            public void GivenGitHubReturnedAnError_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var githubProvider = new GitHubProvider("aa", "bb", null);
                const string existingState = "http://2p1s.com";
                var queryStringParameters = new NameValueCollection
                {
                    {
                        "error",
                        "I dont' always use bayonets. But when I do, I transport them on Aircraft Carriers."
                    },
                    {"state", existingState}
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => githubProvider.AuthenticateClient(queryStringParameters, existingState));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to retrieve an authorization code from GitHub. The error provided is: I dont' always use bayonets. But when I do, I transport them on Aircraft Carriers.",
                    result.Message);
            }

            [Fact]
            public void GivenNoCodeAndNoErrorWasReturned_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var githubProvider = new GitHubProvider("aa", "bb", null);
                const string existingState = "http://2p1s.com";
                var queryStringParameters = new NameValueCollection
                {
                    {"aaa", "bbb"},
                    {"state", existingState}
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => githubProvider.AuthenticateClient(queryStringParameters, existingState));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("No code parameter provided in the response query string from GitHub.", result.Message);
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
                var githubProvider = new GitHubProvider("aa", "bb", new RestClientFactory(mockRestClient.Object));
                const string existingState = "http://2p1s.com";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => githubProvider.AuthenticateClient(queryStringParameters, existingState));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to obtain an Access Token from GitHub OR the the response was not an HTTP Status 200 OK. Response Status: BadRequest. Response Description: Bad Request",
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
                var githubProvider = new GitHubProvider("aa", "bb", new RestClientFactory(mockRestClient.Object));
                const string existingState = "http://2p1s.com";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => githubProvider.AuthenticateClient(queryStringParameters, existingState));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Failed to obtain an Access Token from GitHub.", result.Message);
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
                var githubProvider = new GitHubProvider("aa", "bb", new RestClientFactory(mockRestClient.Object));
                const string existingState = "http://2p1s.com";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => githubProvider.AuthenticateClient(queryStringParameters, existingState));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to obtain an Access Token from GitHub OR the the response was not an HTTP Status 200 OK. Response Status: Unauthorized. Response Description: Unauthorized",
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
                var githubProvider = new GitHubProvider("aa", "bb", new RestClientFactory(mockRestClient.Object));
                const string existingState = "http://2p1s.com";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => githubProvider.AuthenticateClient(queryStringParameters, existingState));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Retrieved a GitHub Access Token but it doesn't contain one or more of either: access_token or token_type",
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
                    TokenType = "overly attached girlfriend"
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

                var githubProvider = new GitHubProvider("aa", "bb", new RestClientFactory(mockRestClient.Object));
                const string existingState = "http://2p1s.com";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => githubProvider.AuthenticateClient(queryStringParameters, existingState));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to obtain User Info from GitHub OR the the response was not an HTTP Status 200 OK. Response Status: Unauthorized. Response Description: Unauthorized",
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
                    TokenType = "overly attached girlfriend"
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

                var githubProvider = new GitHubProvider("aa", "bb", new RestClientFactory(mockRestClient.Object));
                const string existingState = "http://2p1s.com";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => githubProvider.AuthenticateClient(queryStringParameters, existingState));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Retrieve some user info from the GitHub Api, but we're missing one or more of either: Id, Login, and Name.",
                    result.Message);
            }

            [Fact]
            public void GivenExecutingRetrieveSomeUserInfo_AuthenticateClient_ReturnsAnAuthenticatedClient()
            {
                // Arrange.
                const string accessToken = "aaa";
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponse.Setup(x => x.Data).Returns(new AccessTokenResult
                {
                    AccessToken = accessToken,
                    TokenType = "overly attached girlfriend"
                });

                var userInfoResult = new UserInfoResult
                {
                    Email = "aaa",
                    Id = 1,
                    Login = "fun",
                    Name = "eee",
                    AvatarUrl = "http://2p1s.com/zomg",
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

                var githubProvider = new GitHubProvider("aa", "bb", new RestClientFactory(mockRestClient.Object));
                const string existingState = "http://2p1s.com";

                var queryStringParameters = new NameValueCollection
                {
                    {"code", accessToken},
                    {"state", existingState}
                };

                // Act.
                var result = githubProvider.AuthenticateClient(queryStringParameters, existingState);

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("github", result.ProviderName);
                Assert.Equal(accessToken, result.AccessToken);
                Assert.Equal(new DateTime(),result.AccessTokenExpiresOn);
                Assert.NotNull(result.UserInformation);
                Assert.Equal(GenderType.Unknown, result.UserInformation.Gender);
                Assert.Equal(userInfoResult.Id.ToString(), result.UserInformation.Id);
                Assert.Equal(userInfoResult.Location, result.UserInformation.Locale);
                Assert.Equal(userInfoResult.Name, result.UserInformation.Name);
                Assert.Equal(userInfoResult.AvatarUrl, result.UserInformation.Picture);
                Assert.Equal(userInfoResult.Login, result.UserInformation.UserName);
            }
        }

        public class RedirectToAuthenticateFacts
        {
            [Fact]
            public void GivenSomeState_RedirectToAuthenticate_ReturnsAUri()
            {
                // Arrange.
                var githubProvider = new GitHubProvider("aa", "bb", null);

                // Act.
                var result =
                    githubProvider.RedirectToAuthenticate(new GitHubAuthenticationServiceSettings
                    {
                        CallBackUri =
                            new Uri("http://wwww.pewpew.com/"),
                        State = "bleh"
                    });

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "https://github.com/login/oauth/authorize?client_id=aa&redirect_uri=http://wwww.pewpew.com/&response_type=code&state=bleh",
                    result.AbsoluteUri);
            }
        }
    }

    // ReSharper restore InconsistentNaming
}