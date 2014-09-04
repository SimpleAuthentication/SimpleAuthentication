using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Shouldly;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers;
using Xunit;

namespace SimpleAuthentication.Tests.Providers
{
    public class TwitterProviderFacts
    {
        public class AuthenticateClientAsyncFacts
        {
            [Fact]
            public async Task GivenAValidQuerystring_AuthenticateClientAsync_ReturnsAnUserAuthenticatedUser()
            {
                // Arrange.
                var providerParams = new ProviderParams("Rb7qNNPUPsRSYkznFTbF6Q",
                    "pP1jBdYOlmCzo08QFJjGIHY4YSyPdGLPO2m1q47hu9c");
                var provider = new TwitterProvider(providerParams);

                var accessTokenContent = File.ReadAllText("Sample Data\\Twitter-AccessToken-Content.txt");
                var accessTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(accessTokenContent);
                var verifyCredentialsContent = File.ReadAllText("Sample Data\\Twitter-VerifyCredentials-Content.json");
                var verifyCredentialsResponse =
                    FakeHttpMessageHandler.GetStringHttpResponseMessage(verifyCredentialsContent);
                HttpClientFactory.MessageHandler =
                    new FakeHttpMessageHandler(new Dictionary<string, HttpResponseMessage>
                    {
                        {"https://api.twitter.com/oauth/access_token", accessTokenResponse},
                        {"https://api.twitter.com/1.1/account/verify_credentials.json", verifyCredentialsResponse}
                    });

                const string state = "B9608CE5-1E3F-4AF9-A4FA-043BFF70F53C";
                var querystring = new Dictionary<string, string>
                {
                    {"state", state},
                    {"oauth_token", "abcdef"},
                    {"oauth_verifier", "qwerty"},
                };

                // Act.
                var result = await provider.AuthenticateClientAsync(querystring, state, null);

                // Assert.
                result.ProviderName.ShouldBe("Twitter");
                result.AccessToken.Token.ShouldBe("16357275-5h2o01LoiGsdEVPqNsvYzj9l9n9A7ThRednaL3JyI");
                result.AccessToken.Secret.ShouldBe("CvYNcm7Vs2IGyguHrNw2DArmCVqTtwrCw0oWeb1hH8");
                result.AccessToken.ExpiresOn.ShouldBe(DateTime.MaxValue);
                result.UserInformation.Name.ShouldBe("Matt Harris");
                result.UserInformation.Email.ShouldBeNullOrEmpty();
                result.UserInformation.Id.ShouldBe("777925");
                result.UserInformation.UserName.ShouldBe("themattharris");
                result.RawUserInformation.ShouldNotBe(null);
            }

            [Fact]
            public void GivenAQuerystringWithAMissingVerifierTokenKeyValue_AuthenticateClientAsync_ThrowsAnException()
            {
                // Arrange.
                var providerParams = new ProviderParams("Rb7qNNPUPsRSYkznFTbF6Q",
                    "pP1jBdYOlmCzo08QFJjGIHY4YSyPdGLPO2m1q47hu9c");
                var provider = new TwitterProvider(providerParams);
                const string state = "B9608CE5-1E3F-4AF9-A4FA-043BFF70F53C";
                var querystring = new Dictionary<string, string>
                {
                    {"state", state},
                    {"oauth_token", "abcdef"}
                };

                // Act.
                var exception = Should.Throw<AuthenticationException>(
                    async () => await provider.AuthenticateClientAsync(querystring, state, null));

                // Assert.
                exception.Message.ShouldBe("Verifier token response content from 'Twitter' expected the key/value 'oauth_verifier' but none was retrieved.");
            }

            [Fact]
            public void GivenAnInvalidAccessToken_AuthenticateClientAsync_ThrowsAnException()
            {
                // Arrange.
                var providerParams = new ProviderParams("Rb7qNNPUPsRSYkznFTbF6Q",
                    "pP1jBdYOlmCzo08QFJjGIHY4YSyPdGLPO2m1q47hu9c");
                var provider = new TwitterProvider(providerParams);

                var accessTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage("i am a bad access token");
                HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(accessTokenResponse);
                
                const string state = "B9608CE5-1E3F-4AF9-A4FA-043BFF70F53C";
                var querystring = new Dictionary<string, string>
                {
                    {"state", state},
                    {"oauth_token", "abcdef"},
                    {"oauth_verifier", "qwerty"},
                };

                // Act.
                var exception = Should.Throw<AuthenticationException>(
                    async () => await provider.AuthenticateClientAsync(querystring, state, null));

                // Assert.
                exception.Message.ShouldBe("Access token response content from Twitter expected the key/value 'oauth_token' but none was retrieved. Content: i am a bad access token");
            }

            [Fact]
            public void GivenAnAccessTokenUnauthorizedError_AuthenticateClientAsync_ThrowsAnException()
            {
                // Arrange.
                var providerParams = new ProviderParams("Rb7qNNPUPsRSYkznFTbF6Q",
                    "pP1jBdYOlmCzo08QFJjGIHY4YSyPdGLPO2m1q47hu9c");
                var provider = new TwitterProvider(providerParams);

                var accessTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage("i am a bad access token",
                    HttpStatusCode.Unauthorized);
                HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(accessTokenResponse);

                const string state = "B9608CE5-1E3F-4AF9-A4FA-043BFF70F53C";
                var querystring = new Dictionary<string, string>
                {
                    {"state", state},
                    {"oauth_token", "abcdef"},
                    {"oauth_verifier", "qwerty"},
                };

                // Act.
                var exception = Should.Throw<AuthenticationException>(
                    async () => await provider.AuthenticateClientAsync(querystring, state, null));

                // Assert.
                exception.Message.ShouldBe("Failed to retrieve an HttpStatus-OK while attempting to get a Request Token from Twitter. Status: Unauthorized. Content: i am a bad access token.");
            }

            [Fact]
            public void GivenAnInvalidVerifyCredentials_AuthenticateClientAsync_ThrowsAnException()
            {
                // Arrange.
                var providerParams = new ProviderParams("Rb7qNNPUPsRSYkznFTbF6Q",
                    "pP1jBdYOlmCzo08QFJjGIHY4YSyPdGLPO2m1q47hu9c");
                var provider = new TwitterProvider(providerParams);

                var accessTokenContent = File.ReadAllText("Sample Data\\Twitter-AccessToken-Content.txt");
                var accessTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(accessTokenContent);
                var verifyCredentialsResponse =
                    FakeHttpMessageHandler.GetStringHttpResponseMessage("i am some bad user credentials");
                HttpClientFactory.MessageHandler =
                    new FakeHttpMessageHandler(new Dictionary<string, HttpResponseMessage>
                    {
                        {"https://api.twitter.com/oauth/access_token", accessTokenResponse},
                        {"https://api.twitter.com/1.1/account/verify_credentials.json", verifyCredentialsResponse}
                    });

                const string state = "B9608CE5-1E3F-4AF9-A4FA-043BFF70F53C";
                var querystring = new Dictionary<string, string>
                {
                    {"state", state},
                    {"oauth_token", "abcdef"},
                    {"oauth_verifier", "qwerty"},
                };

                // Act.
                var exception = Should.Throw<AuthenticationException>(
                    async () => await provider.AuthenticateClientAsync(querystring, state, null));

                // Assert.
                exception.Message.ShouldBe("Failed to deserialize the Twitter Verify Credentials response json content. Possibly because the content isn't json? Content attempted: i am some bad user credentials");
            }
        }

        public class GetRedirectToAuthenticateSettingsFacts
        {
            [Fact]
            public void GivenACallbackUrl_GetRedirectToAuthenticateSettingsAsync_ReturnsSomeRedirectToAuthenticationSettings()
            {
                // Arrange.
                var providerParams = new ProviderParams("Rb7qNNPUPsRSYkznFTbF6Q",
                    "pP1jBdYOlmCzo08QFJjGIHY4YSyPdGLPO2m1q47hu9c");
                var provider = new TwitterProvider(providerParams);
                var callbackUrl = new Uri("http://www.mysite.com/callback/endpoint?provider=pewpew");
                var accessTokenContent = File.ReadAllText("Sample Data\\Twitter-RequestToken-Content.txt");
                var accessTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(accessTokenContent);
                HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(accessTokenResponse);

                // Act.
                var settings = provider.GetRedirectToAuthenticateSettings(callbackUrl);

                // Assert.
                settings.RedirectUri.AbsoluteUri.ShouldBe(
                    "https://twitter.com/oauth/authenticate?oauth_token=NPcudxy0yU5T3tBzho7iCotZ3cnetKwcTIRlX0iwRl0");
                settings.State.ShouldNotBe(null);
            }
        }
    }
}