using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Shouldly;
using SimpleAuthentication.Core;
using SimpleAuthentication.ExtraProviders;
using WorldDomination.Net.Http;
using Xunit;

namespace SimpleAuthentication.Tests.ExtraProviders
{
    public class GitHubProviderFacts
    {
        public class GetRedirectToAuthenticateSettingsAsyncFacts
        {
            [Fact]
            public void GivenARedirectUrl_GetRedirectToAuthenticateSettings_ReturnsARedirectToAuthenticateSettings()
            {
                // Arrange.
                var provider = TestHelpers.AuthenticationProviders["github"];
                var callbackUri = new Uri("http://www.mysite.com/pew/pew?provider=github");

                // Act.
                var result = provider.GetRedirectToAuthenticateSettings(callbackUri);

                // Assert.
                result.State.ShouldNotBe(null);
                var queryStringSegments = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", TestHelpers.ConfigProviderKey),
                    new KeyValuePair<string, string>("redirect_uri", callbackUri.AbsoluteUri),
                    new KeyValuePair<string, string>("response_type", "code"),
                    new KeyValuePair<string, string>("scope", "user:email"),
                    new KeyValuePair<string, string>("state", result.State)
                }.ToEncodedString();
                var url = string.Format("https://github.com/login/oauth/authorize?{0}", queryStringSegments);
                
                result.RedirectUri.AbsoluteUri.ShouldBe(url);
            }
        }

        public class AuthenticateClientAsyncFacts
        {
            [Fact]
            public async Task GivenAValidResponse_AuthenticateClientAsync_ReturnsAnAuthenticatedClient()
            {
                // Arrange.
                var providerParams = new ProviderParams("zdskjhf&*^65sdfh/.<>\\sdf",
                    "szdkjhg&^%178~/.,<>\\[]{}sdsf sd df s");
                var provider = new GitHubProvider(providerParams);
                const string stateKey = "state";
                const string state = "adyi#&(*,./,.!~`  uhj97&^*&shdgf\\//////\\dsf";
                var querystring = new Dictionary<string, string>
                {
                    {stateKey, state},
                    {"code", "4/P7q7W91a-oMsCeLvIaQm6bTrgtp7"}
                };
                var redirectUrl = new Uri("http://www.mywebsite.com/go/here/please");
                var accessTokenJson = File.ReadAllText("Sample Data\\GitHub-AccessToken-Content.txt");
                var accessTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(accessTokenJson);
                var userInformationJson = File.ReadAllText("Sample Data\\GitHub-UserInfoResult-Content.json");
                var userInformationResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(userInformationJson);
                HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(
                    new Dictionary<string, HttpResponseMessage>
                    {
                        {"https://github.com/login/oauth/access_token", accessTokenResponse},
                        {"https://api.github.com/user?access_token=1a048d8e06ee61de21625d2820", userInformationResponse}
                    });

                // Act.
                var result = await provider.AuthenticateClientAsync(querystring, state, redirectUrl);
 
                // Assert.
                result.AccessToken.Token.ShouldBe("1a048d8e06ee61de21625d2820");
                result.UserInformation.Email.ShouldBe("Kitiara.Uth.Matar@dragon-highlords.takhisis-armies.krynn");
                result.UserInformation.Name.ShouldBe("Kitiara Uth Matar");
                result.UserInformation.UserName.ShouldBe("Kitiara.Uth.Matar");
                result.UserInformation.Id.ShouldBe("69");
                result.UserInformation.Picture.ShouldBe("http://i.imgur.com/J3oaqoB.jpg");
                result.AccessToken.ExpiresOn.ShouldBe(DateTime.MaxValue);
            }
        }
    }
}
