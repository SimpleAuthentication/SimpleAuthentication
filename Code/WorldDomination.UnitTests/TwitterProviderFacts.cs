using System;
using System.Collections.Specialized;
using System.Net;
using Moq;
using RestSharp;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Twitter;
using Xunit;

namespace WorldDomination.UnitTests
{
    // ReSharper disable InconsistentNaming

    public class TwitterProviderFacts
    {
        //public class RedirectToAuthenticateFacts
        //{
        //    [Fact]
        //    public void GivenAValidRequestToken_RedirectToAuthenticate_ReturnsARedirectResult()
        //    {
        //        // Arrange.
        //        var redirectUri =
        //            new Uri(
        //                "http://api.twitter.com/oauth/authorize?oauth_token=5rlgo7AwGJt67vSoHmF227QMSHXhvmOmhN5YJpRlEo");
        //        var mockRestResponse = new Mock<IRestResponse>();
        //        mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
        //        mockRestResponse
        //            .Setup(x => x.Content)
        //            .Returns("oauth_token=aaaaaaa&oauth_token_secret=asdasd");
        //        var mockRestClient = new Mock<IRestClient>();
        //        mockRestClient
        //            .Setup(x => x.BuildUri(It.IsAny<IRestRequest>()))
        //            .Returns(redirectUri);
        //        mockRestClient
        //            .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
        //            .Returns(mockRestResponse.Object);
        //        var twitterProvider = new TwitterProvider("a", "b", mockRestClient.Object);
        //        const string callbackUrl = "someCallBackUri";

        //        // Act.
        //        var result = twitterProvider.RedirectToAuthenticate(callbackUrl);

        //        // Assert.
        //        Assert.NotNull(result);
        //        Assert.Equal(result.Url, redirectUri.AbsoluteUri);
        //    }

        //    [Fact]
        //    public void GivenARequestTokenError_RedirectToAuthenticate_ThrowsAnAuthenticationException()
        //    {
        //        // Arrange.
        //        var mockRestResponse = new Mock<IRestResponse>();
        //        mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.Unauthorized);
        //        mockRestResponse.Setup(x => x.StatusDescription).Returns("Unauthorized");
        //        var mockRestClient = new Mock<IRestClient>();
        //        mockRestClient
        //            .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
        //            .Returns(mockRestResponse.Object);
        //        var twitterProvider = new TwitterProvider("a", "b", mockRestClient.Object);

        //        // Act.
        //        var result = Assert.Throws<AuthenticationException>(() => twitterProvider.RedirectToAuthenticate("pewpew"));

        //        // Assert.
        //        Assert.NotNull(result);
        //        Assert.Null(result.InnerException);
        //        Assert.Equal("Failed to obtain a Request Token from Twitter OR the the response was not an HTTP Status 200 OK. Response Status: Unauthorized. Response Description: Unauthorized", result.Message);
        //    }

        //    [Fact]
        //    public void GivenAInvalidRequestToken_RedirectToAuthenticate_ThrowsAnAuthenticationException()
        //    {
        //        // Arrange.
        //        var mockRestResponse = new Mock<IRestResponse>();
        //        mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
        //        mockRestResponse.Setup(x => x.Content)
        //            .Returns("oauth_token=aaaaaaa&missing_an_oauth_token_secret=asdasd");
        //        var mockRestClient = new Mock<IRestClient>();
        //        mockRestClient
        //            .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
        //            .Returns(mockRestResponse.Object);
        //        var twitterProvider = new TwitterProvider("a", "b", mockRestClient.Object);

        //        // Act.
        //        var result = Assert.Throws<AuthenticationException>(() => twitterProvider.RedirectToAuthenticate("pewpew"));

        //        // Assert.
        //        Assert.NotNull(result);
        //        Assert.Null(result.InnerException);
        //        Assert.Equal("Retrieved a Twitter Request Token but it doesn't contain both the oauth_token and oauth_token_secret parameters.", result.Message);
        //    }
        //}

        //public class RetrieveUserInformationFacts
        //{
        //    [Fact]
        //    public void GivenTheCallbackParamtersAreInvalid_RetrieveUserInformation_ThrowsAnAuthenticationException()
        //    {
        //        // Arrange.
        //        var mockRestClient = new Mock<IRestClient>();
        //        mockRestClient
        //            .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
        //            .Throws<Exception>();
        //        var twitterProvider = new TwitterProvider("a", "b", mockRestClient.Object);
        //        var nameValueCollection = new NameValueCollection(); // Missing 2x required params.
                
        //        // Act.
        //        var result =
        //            Assert.Throws<AuthenticationException>(
        //                () => twitterProvider.RetrieveUserInformation(new TwitterClient(), nameValueCollection));

        //        // Assert.
        //        Assert.NotNull(result);
        //        Assert.Equal("Failed to retrieve an oauth_token and an oauth_token_secret after the client has signed and approved via Twitter.", result.Message);
        //    }

        //    [Fact]
        //    public void GivenExecutingARequestToRetrieveARequestTokenThrowsAnException_RetrieveUserInformation_ThrowsAnAuthenticationException()
        //    {
        //        // Arrange.
        //        var mockRestClient = new Mock<IRestClient>();
        //        mockRestClient
        //            .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
        //            .Throws<Exception>();
        //        var twitterProvider = new TwitterProvider("a", "b", mockRestClient.Object);
        //        var nameValueCollection = new NameValueCollection
        //                                  {
        //                                      {"oauth_token", "aaa"},
        //                                      {"oauth_verifier", "bbbb"}
        //                                  };
        //        // Act.
        //        var result =
        //            Assert.Throws<AuthenticationException>(
        //                () => twitterProvider.RetrieveUserInformation(new TwitterClient(), nameValueCollection));

        //        // Assert.
        //        Assert.NotNull(result);
        //        Assert.Equal("Failed to convert Request Token to an Access Token, from Twitter.", result.Message);
        //    }

        //    [Fact]
        //    public void GivenAnInvalidRequestToken_RetrieveUserInformation_ThrowsAnAuthenticationException()
        //    {
        //        // Arrange.
        //        var mockRestClient = new Mock<IRestClient>();
        //        var mockRestResponse = new Mock<IRestResponse>();
        //        mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.Unauthorized);
        //        mockRestResponse.Setup(x => x.StatusDescription).Returns("Unauthorized");
        //        mockRestClient
        //            .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
        //            .Returns(mockRestResponse.Object);
        //        var twitterProvider = new TwitterProvider("a", "b", mockRestClient.Object);
        //        var nameValueCollection = new NameValueCollection
        //                                  {
        //                                      {"oauth_token", "aaa"},
        //                                      {"oauth_verifier", "bbbb"}
        //                                  };
        //        // Act.
        //        var result =
        //            Assert.Throws<AuthenticationException>(
        //                () => twitterProvider.RetrieveUserInformation(new TwitterClient(), nameValueCollection));

        //        // Assert.
        //        Assert.NotNull(result);
        //        Assert.Equal("Failed to obtain an Access Token from Twitter OR the the response was not an HTTP Status 200 OK. Response Status: Unauthorized. Response Description: Unauthorized", result.Message);
        //    }

        //    [Fact]
        //    public void GivenAnRequestTokenWithMissingParameters_RetrieveUserInformation_ThrowsAnAuthenticationException()
        //    {
        //        // Arrange.
        //        var mockRestClient = new Mock<IRestClient>();
        //        var mockRestResponse = new Mock<IRestResponse>();
        //        mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
        //        mockRestClient
        //            .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
        //            .Returns(mockRestResponse.Object);
        //        var twitterProvider = new TwitterProvider("a", "b", mockRestClient.Object);
        //        var nameValueCollection = new NameValueCollection
        //                                  {
        //                                      {"oauth_token", "aaa"}
        //                                  }; // Missing oauth_secret.
        //        // Act.
        //        var result =
        //            Assert.Throws<AuthenticationException>(
        //                () => twitterProvider.RetrieveUserInformation(new TwitterClient(), nameValueCollection));

        //        // Assert.
        //        Assert.NotNull(result);
        //        Assert.Equal("Failed to retrieve an oauth_token and an oauth_token_secret after the client has signed and approved via Twitter.", result.Message);
        //    }

        //    [Fact]
        //    public void GivenExecutingVerifyCredentialsThrowsAnException_RetrieveUserInformation_ThrowsAnAuthenticationException()
        //    {
        //        // Arrange.
        //        var mockRestClient = new Mock<IRestClient>();
                
        //        var mockRestResponseRetrieveRequestToken = new Mock<IRestResponse>();
        //        mockRestResponseRetrieveRequestToken.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
        //        mockRestResponseRetrieveRequestToken.Setup(x => x.Content).Returns("oauth_token=aaa&oauth_token_secret=ccc");

        //        var verifyCredentialsResult = new VerifyCredentialsResult
        //                                      {
        //                                          Name = "Some Name",
        //                                          Id = 1234,
        //                                          Lang = "en-au",
        //                                          ScreenName = "Some-Screen-Name"
        //                                      };
        //        var mockRestResponseVerifyCredentials = new Mock<IRestResponse<VerifyCredentialsResult>>();
        //        mockRestResponseVerifyCredentials.Setup(x => x.Data).Returns(verifyCredentialsResult);

        //        mockRestClient
        //            .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
        //            .Returns(mockRestResponseRetrieveRequestToken.Object);
        //        mockRestClient
        //            .Setup(x => x.Execute<VerifyCredentialsResult>(It.IsAny<IRestRequest>()))
        //            .Throws(new Exception("Mock exception."));

        //        var twitterProvider = new TwitterProvider("a", "b", mockRestClient.Object);
        //        var nameValueCollection = new NameValueCollection
        //                                  {
        //                                      {"oauth_token", "aaa"},
        //                                      {"oauth_verifier", "bbb"}
        //                                  };
        //        // Act.
        //        var result =
        //            Assert.Throws<AuthenticationException>(
        //                () => twitterProvider.RetrieveUserInformation(new TwitterClient(), nameValueCollection));

        //        // Assert.
        //        Assert.NotNull(result);
        //        Assert.Equal("Failed to retrieve VerifyCredentials json data from the Twitter Api.", result.Message);
        //        Assert.NotNull(result.InnerException);
        //        Assert.Equal("Mock exception.", result.InnerException.Message);
        //    }

        //    [Fact]
        //    public void GivenAnInvalidVerifyCredentials_RetrieveUserInformation_ThrowsAnAuthenticationException()
        //    {
        //        // Arrange.
        //        var mockRestClient = new Mock<IRestClient>();

        //        var mockRestResponseRetrieveRequestToken = new Mock<IRestResponse>();
        //        mockRestResponseRetrieveRequestToken.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
        //        mockRestResponseRetrieveRequestToken.Setup(x => x.Content).Returns("oauth_token=aaa&oauth_token_secret=ccc");
                
        //        var mockRestResponseVerifyCredentials = new Mock<IRestResponse<VerifyCredentialsResult>>();
        //        mockRestResponseVerifyCredentials.Setup(x => x.StatusCode).Returns(HttpStatusCode.Unauthorized);
        //        mockRestResponseVerifyCredentials.Setup(x => x.StatusDescription).Returns("Unauthorized");

        //        mockRestClient
        //            .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
        //            .Returns(mockRestResponseRetrieveRequestToken.Object);
        //        mockRestClient
        //            .Setup(x => x.Execute<VerifyCredentialsResult>(It.IsAny<IRestRequest>()))
        //            .Returns(mockRestResponseVerifyCredentials.Object);

        //        var twitterProvider = new TwitterProvider("a", "b", mockRestClient.Object);
        //        var nameValueCollection = new NameValueCollection
        //                                  {
        //                                      {"oauth_token", "aaa"},
        //                                      {"oauth_verifier", "bbb"}
        //                                  };
        //        // Act.
        //        var result =
        //            Assert.Throws<AuthenticationException>(
        //                () => twitterProvider.RetrieveUserInformation(new TwitterClient(), nameValueCollection));

        //        // Assert.
        //        Assert.NotNull(result);
        //        Assert.Equal("Failed to retrieve VerifyCredentials json data OR the the response was not an HTTP Status 200 OK. Response Status: Unauthorized. Response Description: Unauthorized", result.Message);
        //    }

        //    [Fact]
        //    public void GivenSomeValidVerifyCredentials_RetrieveUserInformation_ReturnsATwitterClientWithUserInformation()
        //    {
        //        // Arrange.
        //        var mockRestClient = new Mock<IRestClient>();

        //        var mockRestResponseRetrieveRequestToken = new Mock<IRestResponse>();
        //        mockRestResponseRetrieveRequestToken.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
        //        mockRestResponseRetrieveRequestToken.Setup(x => x.Content).Returns("oauth_token=aaa&oauth_token_secret=ccc");

        //        var verifyCredentialsResult = new VerifyCredentialsResult
        //        {
        //            Name = "Some Name",
        //            Id = 1234,
        //            Lang = "en-au",
        //            ScreenName = "Some-Screen-Name"
        //        };
        //        var mockRestResponseVerifyCredentials = new Mock<IRestResponse<VerifyCredentialsResult>>();
        //        mockRestResponseVerifyCredentials.Setup(x => x.Data).Returns(verifyCredentialsResult);
        //        mockRestResponseVerifyCredentials.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);

        //        mockRestClient
        //            .Setup(x => x.Execute(It.IsAny<IRestRequest>()))
        //            .Returns(mockRestResponseRetrieveRequestToken.Object);
        //        mockRestClient
        //            .Setup(x => x.Execute<VerifyCredentialsResult>(It.IsAny<IRestRequest>()))
        //            .Returns(mockRestResponseVerifyCredentials.Object);

        //        var twitterProvider = new TwitterProvider("a", "b", mockRestClient.Object);
        //        var nameValueCollection = new NameValueCollection
        //                                  {
        //                                      {"oauth_token", "aaa"},
        //                                      {"oauth_verifier", "bbb"}
        //                                  };
        //        var twitterClient = new TwitterClient();

        //        // Act.
        //        twitterProvider.RetrieveUserInformation(twitterClient, nameValueCollection);

        //        // Assert.
        //        Assert.NotNull(twitterClient.UserInformation);
        //        Assert.NotNull(twitterClient.UserInformation.Id);
        //        Assert.NotNull(twitterClient.UserInformation.Locale);
        //        Assert.NotNull(twitterClient.UserInformation.Name);
        //        Assert.NotNull(twitterClient.UserInformation.UserName);
        //    }
        //}
    }

    // ReSharper restore InconsistentNaming
}