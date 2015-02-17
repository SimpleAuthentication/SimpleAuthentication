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
    public class LinkedInProviderFacts
    {
        public class GetRedirectToAuthenticateSettingsAsyncFacts
        {
            [Fact]
            public async Task
                GivenARedirectUrl_GetRedirectToAuthenticateSettings_ReturnsARedirectToAuthenticateSettings()
            {
                // Arrange.
                var providerParams = new ProviderParams("zdskjhf&*^65sdfh/.<>\\sdf",
                    "szdkjhg&^%178~/.,<>\\[]{}sdsf sd df s");
                var provider = new LinkedInProvider(providerParams);
                var callbackUri = new Uri("http://www.mysite.com/pew/pew?provider=linkedin");

                // Act.
                var result = provider.GetRedirectToAuthenticateSettings(callbackUri);

                // Assert.
                result.State.ShouldNotBe(null);
                result.RedirectUri.AbsoluteUri.ShouldBe(
                    string.Format(
                        "https://www.linkedin.com/uas/oauth2/authorization?client_id=zdskjhf%26%2A%5E65sdfh%2F.%3C%3E%5Csdf&redirect_uri=http%3A%2F%2Fwww.mysite.com%2Fpew%2Fpew%3Fprovider%3Dlinkedin&response_type=code&scope=r_basicprofile%20r_emailaddress&state={0}",
                        result.State));
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
                var provider = new LinkedInProvider(providerParams);
                const string stateKey = "state";
                const string state = "adyi#&(*,./,.!~`  uhj97&^*&shdgf\\//////\\dsf";
                var querystring = new Dictionary<string, string>
                {
                    {stateKey, state},
                    {"code", "4/P7q7W91a-oMsCeLvIaQm6bTrgtp7"}
                };
                var redirectUrl = new Uri("http://www.mywebsite.com/go/here/please");
                var accessTokenJson = File.ReadAllText("Sample Data\\LinkedIn-AccessToken-Content.json");
                var accessTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(accessTokenJson);
                var userInformationJson = File.ReadAllText("Sample Data\\LinkedIn-UserInfoResult-Content.json");
                var userInformationResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(userInformationJson);
                HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(
                    new Dictionary<string, HttpResponseMessage>
                    {
                        {"https://www.linkedin.com/uas/oauth2/accessToken", accessTokenResponse},
                        {"https://api.linkedin.com/v1/people/~:(id,formatted-name,email-address,picture-url)?oauth2_access_token=fb2e77d.47a0479900504cb3ab4a1f626d174d2d&format=json", userInformationResponse}
                    });

                // Act.
                var result = await provider.AuthenticateClientAsync(querystring, state, redirectUrl);

                // Assert.
                result.AccessToken.Token.ShouldBe("fb2e77d.47a0479900504cb3ab4a1f626d174d2d");
                result.UserInformation.Email.ShouldBe("flint@fireforge.net");
                result.UserInformation.Name.ShouldBe("Flint Fireforge");
                result.UserInformation.UserName.ShouldBe(null);
                result.UserInformation.Id.ShouldBe("1468815002");
                result.UserInformation.Picture.ShouldBe("https://media.licdn.com/mpr/mprx/anonymousUser");
                result.AccessToken.ExpiresOn.ShouldBeGreaterThan(DateTime.Now);
            }
        }
    }
}