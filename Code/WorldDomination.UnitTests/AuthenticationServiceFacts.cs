using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;
using Moq;
using RestSharp;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Facebook;
using Xunit;

namespace WorldDomination.UnitTests
{
    // ReSharper disable InconsistentNaming

    public class AuthenticationServiceFacts
    {
        public class AddProviderFacts
        {
            [Fact]
            public void GivenANewProvider_AddProvider_AddsTheProviderToTheProviderCollection()
            {
                // Arrange.
                var authenticationService = new AuthenticationService();

                // Act.
                authenticationService.AddProvider(new FacebookProvider("a", "b", new Uri("http://www.google.com")));

                // Assert.
                var providers = authenticationService.AuthenticationProviders;
                Assert.NotNull(providers);
                Assert.Equal(1, providers.Count);
                Assert.NotNull(providers["Facebook"]);
            }

            [Fact]
            public void GivenAnExistingProvider_AddProvider_ThrowsAnException()
            {
                // Arrange.
                var authenticationService = new AuthenticationService();
                var facebookProvider = new FacebookProvider("a", "b", new Uri("http://www.google.com"));
                // Act.
                authenticationService.AddProvider(facebookProvider);
                var result = Assert.Throws<AuthenticationException>( 
                    () => authenticationService.AddProvider(facebookProvider));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Trying to add a Facebook provider, but one already exists.", result.Message);
            }
        }

        public class RedirectToAuthenticationProviderFacts
        {
            [Fact]
            public void GivenAnInvalidProviderKey_RedirectToAuthenticationProvider_ThrowsAnException()
            {
                // Arrange.
                const string providerKey = "aaa";
                const string state = "asd";
                var authenticationService = new AuthenticationService();

                // Act and Assert
                var result = authenticationService.RedirectToAuthenticationProvider(providerKey, state);

            }
        }

        //public class CheckCallbackFacts
        //{
        //    [Fact]
        //    public void GiveAnHttpResponseBaseHasNoParams_CheckCallback_ThrowsAnException()
        //    {
        //        // Arrange.
        //        var mockHttpRequestBase = new Mock<HttpRequestBase>();

        //        var authenticationService = new AuthenticationService();

        //        // Act.
        //        var result = Assert.Throws<AuthenticationException>(() => authenticationService.CheckCallback(mockHttpRequestBase.Object, "meh"));

        //        // Assert.
        //        Assert.NotNull(result);
        //        Assert.Equal("No request params found - unable to determine from where we authenticated with/against.", result.Message);

        //    }

        //    [Fact]
        //    public void GivenAFacebookCallback_CheckCallback_RetrievesAnAccessTokenAnUserData()
        //    {
        //        // Arrange.
        //        var mockRestResponseAccessToken = new Mock<IRestResponse>();
        //        mockRestResponseAccessToken.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
        //        mockRestResponseAccessToken.Setup(x => x.Content).Returns("access_token=foo&expires_on=1000");

        //        var meResult = new MeResult
        //        {
        //            Id = 1,
        //            FirstName = "some firstname",
        //            LastName = "some lastname",
        //            Link = "http://whatever",
        //            Locale = "en-au",
        //            Name = "Hi there",
        //            Timezone = 10,
        //            Username = "PewPew",
        //            Verified = true
        //        };

        //        var mockRestResponseApiMe = new Mock<IRestResponse<MeResult>>();
        //        mockRestResponseApiMe.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
        //        mockRestResponseApiMe.Setup(x => x.Data).Returns(meResult);

        //        var mockRestClient = new Mock<IRestClient>();
        //        mockRestClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
        //            .Returns(mockRestResponseAccessToken.Object);
        //        mockRestClient.Setup(x => x.Execute<MeResult>(It.IsAny<IRestRequest>()))
        //            .Returns(mockRestResponseApiMe.Object);

        //        const string state = "someState";
        //        var facebookProvider = new FacebookProvider("a", "b", new Uri("http://www.google.com"),
        //                                                    mockRestClient.Object);
        //        var authenticationService = new AuthenticationService();
        //        authenticationService.AddProvider(facebookProvider);

        //        var mockHttpRequestBase = new Mock<HttpRequestBase>();
        //        mockHttpRequestBase.Setup(x => x.Params)
        //            .Returns(new NameValueCollection
        //                     {
        //                         {"code", "aaaa"},
        //                         {"state", state}
        //                     });
        //        // Act.
        //        var facebookClient =
        //            authenticationService.CheckCallback(mockHttpRequestBase.Object, state) as FacebookClient;


        //        // Assert.
        //        Assert.NotNull(facebookClient);
        //        Assert.NotNull(facebookClient.AccessToken);
        //        Assert.NotNull(facebookClient.Code);
        //        Assert.NotNull(facebookClient.UserInformation);
        //        Assert.True(facebookClient.UserInformation.Id > 0);
        //        Assert.NotNull(facebookClient.UserInformation.Name);
        //        Assert.NotNull(facebookClient.UserInformation.UserName);
        //    }
        //}
    }

    // ReSharper restore InconsistentNaming
}