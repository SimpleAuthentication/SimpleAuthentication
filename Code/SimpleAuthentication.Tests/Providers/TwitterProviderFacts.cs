using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Shouldly;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Providers;
using Xunit;

namespace SimpleAuthentication.Tests.Providers
{
    public class TwitterProviderFacts
    {
        public class Facts
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
                var verifyCredentialsContent = File.ReadAllText("Sample Data\\Twitter-VerifyCredentials-Content.txt");
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
                var authenticatedClient = await provider.AuthenticateClientAsync(querystring, state, null);

                // Assert.
                authenticatedClient.ProviderName.ShouldBe("Twitter");
                authenticatedClient.AccessToken.Token.ShouldBe("16357275-5h2o01LoiGsdEVPqNsvYzj9l9n9A7ThRednaL3JyI");
                authenticatedClient.AccessToken.Secret.ShouldBe("CvYNcm7Vs2IGyguHrNw2DArmCVqTtwrCw0oWeb1hH8");
                authenticatedClient.AccessToken.ExpiresOn.ShouldBe(DateTime.MaxValue);
                authenticatedClient.UserInformation.Name.ShouldBe("Matt Harris");
                authenticatedClient.UserInformation.Email.ShouldBeNullOrEmpty();
                authenticatedClient.UserInformation.Id.ShouldBe("777925");
                authenticatedClient.UserInformation.UserName.ShouldBe("themattharris");
            }
        }

        public class GetRedirectToAuthenticateSettingsFacts
        {
            [Fact]
            public async Task
                GivenACallbackUrl_GetRedirectToAuthenticateSettingsAsync_ReturnsSomeRedirectToAuthenticationSettings()
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
                var settings = await provider.GetRedirectToAuthenticateSettingsAsync(callbackUrl);

                // Assert.
                settings.RedirectUri.AbsoluteUri.ShouldBe(
                    "https://twitter.com/oauth/authenticate?oauth_token=NPcudxy0yU5T3tBzho7iCotZ3cnetKwcTIRlX0iwRl0");
                settings.State.ShouldNotBe(null);
            }
        }
    }
}