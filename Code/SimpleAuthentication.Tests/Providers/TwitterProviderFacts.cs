using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Providers;
using SimpleAuthentication.Core.Providers.Twitter;
using Xunit;

namespace SimpleAuthentication.Tests.Providers
{
    public class TwitterProviderFacts
    {
        public class TwitterProviderTestClass : TwitterProvider
        {
            public TwitterProviderTestClass(ProviderParams providerParams) : base(providerParams)
            {
            }

            public new async Task<RequestTokenResult> GetRequestTokenAsync
                (Uri callbackUri, string state)
            {
                return await base.GetRequestTokenAsync(callbackUri, state);
            }

            public new async Task<AccessToken> GetAccessTokenAsync(IDictionary<string, string> queryString,
                string callbackUrl)
            {
                return await base.GetAccessTokenAsync(queryString, callbackUrl);
            }
        }

        public class GetRequestTokenAsyncFacts
        {
            [Fact]
            public async Task GivenSomeValidData_GetRequestTokenAsync_ReturnsARequestTokenResult()
            {
                // Arrange.
                var providerParams = new ProviderParams("a b c", "d e f / \\ & * @ #");
                var twitterProvider = new TwitterProviderTestClass(providerParams);
                var callbackUrl = new Uri("http://localhost/authenticate/callback");
                const string state = "some-state";

                // Fake up a request token.
                var requestTokenContent = File.ReadAllText("Sample Data\\Twitter-RequestToken-Content.txt");
                var requestTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(requestTokenContent,
                    mediaType: "text/html");
                HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(
                        "https://api.twitter.com/oauth/request_token",
                        requestTokenResponse);

                // Act.
                var result = await twitterProvider.GetRequestTokenAsync(callbackUrl, state);

                // Assert.
                result.OAuthToken.ShouldBe("NPcudxy0yU5T3tBzho7iCotZ3cnetKwcTIRlX0iwRl0");
                result.OAuthTokenSecret.ShouldBe("veNRnAWe6inFuo8o2u8SLLZLjolYDmDP7SzL0YfYI");

            }
        }

        public class GetAccessTokenAsyncFacts
        {
            [Fact]
            public async Task GivenARequestToken_GetAccessTokenAsync_ReturnsAnAccessTokenResult()
            {
                // Arrange.
                var providerParams = new ProviderParams("a b c", "d e f / \\ & * @ #");
                var twitterProvider = new TwitterProviderTestClass(providerParams);
                var queryString = new Dictionary<string, string>
                {
                    {"oauth_token", "aasdasd"},
                    {"oauth_verifier", "asdsadas"},
                    {"not-used", "xxxxxx"}
                };
                const string callbackUrl = "http://localhost/authenticate/callback";
                

                // Fake up a request token.
                var requestTokenContent = File.ReadAllText("Sample Data\\Twitter-RequestToken-Content.txt");
                var requestTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(requestTokenContent,
                    mediaType: "text/html");
                HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(
                        "https://api.twitter.com/oauth/access_token",
                        requestTokenResponse);

                // Act.
                var result = await twitterProvider.GetAccessTokenAsync(queryString, callbackUrl);

                // Assert.
            }

            [Fact]
            public void GivenAnIncompleteQueryString_GetAccessTokenAsync_ThrowsAnException()
            {
                // Arrange.
                var providerParams = new ProviderParams("a b c", "d e f / \\ & * @ #");
                var twitterProvider = new TwitterProviderTestClass(providerParams);

                // NOTE: oauth_verifier is missing.
                var queryString = new Dictionary<string, string>
                {
                    {"oauth_token", "aasdasd"},
                    {"not-used", "xxxxxx"}
                };
                const string callbackUrl = "http://localhost/authenticate/callback";


                // Fake up a request token.
                var requestTokenContent = File.ReadAllText("Sample Data\\Twitter-RequestToken-Content.txt");
                var requestTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(requestTokenContent,
                    mediaType: "text/html");
                HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(
                        "https://api.twitter.com/oauth/access_token",
                        requestTokenResponse);

                // Act and Assert
                var result =  Should.Throw<Exception>(() => twitterProvider.GetAccessTokenAsync(queryString, callbackUrl));
                result.Message.ShouldBe("Failed to recieve an oauth_token and/or oauth_verifier values. Both are required. oauth_token: aasdasd. oauth_verifier: -no verifier-");
            }
        }
        public class GetRedirectToAuthenticateSettingsFacts
        {
            [Fact]
            public void GivenACallbackUrl_GetRedirectToAuthenticateSettings_ReturnsSomeRedirectToAuthenticateSettings()
            {
                // Arrange.
                var providerParams = new ProviderParams("a b c", "d e f / \\ & * @ #");
                var provider = new TwitterProvider(providerParams);
                var callBackUrl = new Uri("Http://www.foo.com/callback");

                // Fake up a request token.
                var requestTokenContent = File.ReadAllText("Sample Data\\Twitter-RequestToken-Content.txt");
                var requestTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(requestTokenContent,
                    mediaType: "text/html");
                HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(
                        "https://api.twitter.com/oauth/request_token",
                        requestTokenResponse);

                // Act.
                var settings = provider.GetRedirectToAuthenticateSettings(callBackUrl);

                // Assert.
                settings.ShouldNotBe(null);
                settings.State.ShouldNotBeNullOrEmpty();
                Guid.Parse(settings.State);
                settings.RedirectUri.ShouldNotBe(null);
                settings.RedirectUri.AbsoluteUri.ShouldStartWith("https://api.twitter.com/oauth/authenticate?oauth_token=NPcudxy0yU5T3tBzho7iCotZ3cnetKwcTIRlX0iwRl0");
            }

            [Fact(Skip = "TODO")]
            public void
                GivenACallbackUrlButTheRequestTokenIsMissingData_GetRedirectToAuthenticateSettings_ThrowsAnExeption()
            {
                
            }
        }

        //public class AuthenticateClientAsyncFacts
        //{
        //    [Fact]
        //    public void GivenAValidAccessToken_AuthenticateClientAsync_ReturnsSomeUserInformation()
        //    {
        //        // Arrange.
        //        var providerParams = new ProviderParams
        //        {
        //            PublicApiKey = "some public api key",
        //            SecretApiKey = "some secret api key"
        //        };
        //        var provider = new TwitterProvider(providerParams);
        //        var callBackUrl = new Uri("Http://www.foo.com/callback");

        //        // Fake up a request token.
        //        var requestTokenContent = File.ReadAllText("Sample Data\\Twitter-RequestToken-Content.txt");
        //        var requestTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(requestTokenContent,
        //            mediaType: "text/html");
        //        HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(
        //                "https://api.twitter.com/oauth/request_token",
        //                requestTokenResponse);

        //        var state = "abcde";
        //        var callbackUrl = "http://localhost/foo";

        //        // Act.
        //        var settings = provider.AuthenticateClientAsync();   
        //    }
        //}
    }
}
