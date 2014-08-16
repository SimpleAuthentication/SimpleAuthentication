using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Providers;
using Xunit;

namespace SimpleAuthentication.Tests.Providers
{
    public class WindowsLiveProviderFacts
    {
        public class GetRedirectToAuthenticateSettingsFacts
        {
            [Fact]
            public async Task
                GivenACallbackUrl_GetRedirectToAuthenticateSettingsFacts_ReturnsSomeRedirectToAUthenticateSettings()
            {
                // Arramge.
                var providerParams = new ProviderParams("00000000400ED488",
                    "OAc-A5hoXE0eOolc6aczF2xvnq5sLfRr");
                var provider = new WindowsLiveProvider(providerParams);
                var callbackUri = new Uri("http://www.localhost.me?provider=windowsLive");

                // Act.
                var result = await provider.GetRedirectToAuthenticateSettingsAsync(callbackUri);

                // Assert.
                result.State.ShouldNotBeNullOrEmpty();
                result.RedirectUri.AbsoluteUri.ShouldBe(
                    string.Format("https://login.live.com/oauth20_authorize.srf?client_id=00000000400ED488&redirect_uri=http%3A%2F%2Fwww.localhost.me%2F%3Fprovider%3DwindowsLive&response_type=code&scope=wl.signin%2Cwl.basic%2Cwl.emails&state={0}",
                    result.State));
            }
        }

        public class AuthenticateClientAsyncFacts
        {
            [Fact]
            public async Task GivenARepsonse_AuthenticateClientAsync_ReturnsAnAuthenticatedClient()
            {
                // Arramge.
                var providerParams = new ProviderParams("00000000400ED488",
                    "OAc-A5hoXE0eOolc6aczF2xvnq5sLfRr");
                var provider = new WindowsLiveProvider(providerParams);
                var callbackUri = new Uri("http://www.localhost.me?provider=windowsLive");
                const string stateKey = "state";
                const string state = "adyiuhj97&^*&shdgf\\//////\\dsf";
                var querystring = new Dictionary<string, string>
                {
                    {stateKey, state},
                    {"code", "4/P7q7W91a-oMsCeLvIaQm6bTrgtp7"}
                };
                var accessTokenJson = File.ReadAllText("Sample Data\\WindowsLive-AccessToken-Content.json");
                var accessTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(accessTokenJson);
                var userInformationJson = File.ReadAllText("Sample Data\\WindowsLive-UserInfo-Content.json");
                var userInformationResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(userInformationJson);
                HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(
                    new Dictionary<string, HttpResponseMessage>
                    {
                        {"https://login.live.com/oauth20_token.srf", accessTokenResponse},
                        {
                            "https://apis.live.net/v5.0/me?access_token=EwCIAq1DBAAUGCCXc8wU/zFu9QnLdZXy+YnElFkAAaQn28y+8VQWmn7VMOVO9u",
                            userInformationResponse
                        }
                    });

                // Act.
                var result = await provider.AuthenticateClientAsync(querystring,
                    state,
                    callbackUri);

                // Assert.
                result.ProviderName.ShouldBe("WindowsLive");
                result.UserInformation.Gender.ShouldBe(GenderType.Male);
                result.UserInformation.Id.ShouldBe("1234");
                result.UserInformation.Locale.ShouldBe("en-au");
                result.UserInformation.Name.ShouldBe("Tanis Half-Elven");
                result.UserInformation.UserName.ShouldBe(null);
                result.UserInformation.Email.ShouldBe("tanis.half-elven@InnOfLastHope.krynn");
                result.UserInformation.Picture.ShouldBe("https://apis.live.net/v5.0/1234/picture");
            }
        }
    }
}
