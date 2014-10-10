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
    public class InstagramProviderFacts
    {
        public class GetRedirectToAuthenticateSettingsAsyncFacts
        {
            [Fact]
            public void GivenARedirectUrl_GetRedirectToAuthenticateSettings_ReturnsARedirectToAuthenticateSettings()
            {
                // Arrange.
                var providerParams = new ProviderParams("zdskjhf&*^65sdfh/.<>\\sdf",
                    "szdkjhg&^%178~/.,<>\\[]{}sdsf sd df s");
                var provider = new InstagramProvider(providerParams);
                var callbackUri = new Uri("http://www.mysite.com/pew/pew?provider=instagram");

                // Act.
                var result = provider.GetRedirectToAuthenticateSettings(callbackUri);

                // Assert.
                result.State.ShouldNotBe(null);
                result.RedirectUri.AbsoluteUri.ShouldBe(
                    string.Format(
                        "https://api.instagram.com/oauth/authorize/?client_id=zdskjhf%26%2A%5E65sdfh%2F.%3C%3E%5Csdf&redirect_uri=http%3A%2F%2Fwww.mysite.com%2Fpew%2Fpew%3Fprovider%3Dinstagram&response_type=code&scope=basic&state={0}",
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
                var provider = new InstagramProvider(providerParams);
                const string stateKey = "state";
                const string state = "adyi#&(*,./,.!~`  uhj97&^*&shdgf\\//////\\dsf";
                var querystring = new Dictionary<string, string>
                {
                    {stateKey, state},
                    {"code", "4/P7q7W91a-oMsCeLvIaQm6bTrgtp7"}
                };
                var redirectUrl = new Uri("http://www.mywebsite.com/go/here/please");
                var accessTokenJson = File.ReadAllText("Sample Data\\Instagram-AccessToken-Content.json");
                var accessTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(accessTokenJson);
                var userInformationJson = File.ReadAllText("Sample Data\\Instagram-UserInfoResult-Content.json");
                var userInformationResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(userInformationJson);
                HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(
                    new Dictionary<string, HttpResponseMessage>
                    {
                        {"https://api.instagram.com/oauth/access_token", accessTokenResponse},
                        {"https://api.instagram.com/v1/users/self?access_token=fb2e77d.47a0479900504cb3ab4a1f626d174d2d", userInformationResponse}
                    });

                // Act.
                var result = await provider.AuthenticateClientAsync(querystring, state, redirectUrl);

                // Assert.
                result.AccessToken.Token.ShouldBe("fb2e77d.47a0479900504cb3ab4a1f626d174d2d");
                result.UserInformation.Email.ShouldBe(null);
                result.UserInformation.Name.ShouldBe("Flint Fireforge");
                result.UserInformation.UserName.ShouldBe("Flint.Fireforge");
                result.UserInformation.Id.ShouldBe("1468815002");
                result.UserInformation.Picture.ShouldBe("http://images.ak.instagram.com/profiles/anonymousUser.jpg");
                result.AccessToken.ExpiresOn.ShouldBe(DateTime.MaxValue);
            }
        } 
    }
}