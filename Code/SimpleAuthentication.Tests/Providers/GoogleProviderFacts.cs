using System.IO;
using System.Net;
using Newtonsoft.Json;
using Shouldly;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleAuthentication.Core.Providers.Google;
using Xunit;

namespace SimpleAuthentication.Tests.Providers
{
    public class GoogleProviderFacts
    {
        public class GoogleProviderTestClass : GoogleProvider
        {
            public GoogleProviderTestClass(ProviderParams providerParams) : base(providerParams)
            {
            }

            public new async Task<AccessTokenResult> GetAccessTokenFromProviderAsync(string authorizationCode,
                Uri redirectUrl)
            {
                return await base.GetAccessTokenFromProviderAsync(authorizationCode, redirectUrl);
            }

            public new AccessToken MapAccessTokenResultToAccessToken(AccessTokenResult accessTokenResult)
            {
                return base.MapAccessTokenResultToAccessToken(accessTokenResult);
            }

            public new async Task<UserInformation> RetrieveUserInformationAsync(AccessToken accessToken)
            {
                return await base.RetrieveUserInformationAsync(accessToken);
            }
        }

        public class GetRedirectToAuthenticateSettingsFacts
        {
            [Fact]
            public void GivenACallbackUrl_GetRedirectToAuthenticateSettings_ReturnsSomeRedirectToAuthenticateSettings()
            {
                // Arrange.
                var providerParams = new ProviderParams
                {
                    PublicApiKey = "some public api key",
                    SecretApiKey = "some secret api key"
                };
                var provider = new GoogleProvider(providerParams);
                var callBackUrl = new Uri("Http://www.foo.com/callback");
                
                // Act.
                var settings = provider.GetRedirectToAuthenticateSettings(callBackUrl);

                // Assert.
                settings.ShouldNotBe(null);
                settings.State.ShouldNotBeNullOrEmpty();
                Guid.Parse(settings.State);
                settings.RedirectUri.ShouldNotBe(null);
                settings.RedirectUri.AbsoluteUri.ShouldStartWith("https://accounts.google.com/o/oauth2/auth?client_id=some%20public%20api%20key&redirect_uri=http://www.foo.com/callback&response_type=code&scope=https://www.googleapis.com/auth/userinfo.profile%20https://www.googleapis.com/auth/userinfo.email&state=");
            }
        }

        public class GetAccessTokenFromProviderAsyncFacts
        {
            [Fact]
            public async Task GivenAValidAuthorizationCodeAndRedirectUri_GetAccessTokenFromProviderAsync_ReturnsAnAccessToken()
            {
                // Arrange.
                var providerParams = new ProviderParams
                {
                    PublicApiKey = "some public api key",
                    SecretApiKey = "some secret api key"
                };
                var provider = new GoogleProviderTestClass(providerParams);
                const string authorizationCode = "aasdasds";
                var redirectUrl = new Uri("http://a.b.c.d");
                var accessTokenJson = File.ReadAllText("Sample Data\\Google-AccessToken-Content.json");
                var accessTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(accessTokenJson);
                HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(
                        "https://accounts.google.com/o/oauth2/token",
                        accessTokenResponse);
                // Act.
                var result = await provider.GetAccessTokenFromProviderAsync(authorizationCode, redirectUrl);

                // Assert.
                result.ShouldNotBe(null);
                var expectedAccessToken = JsonConvert.DeserializeObject<dynamic>(accessTokenJson);
                result.AccessToken.ShouldBe((string)expectedAccessToken.access_token);
                result.ExpiresIn.ShouldBe((long)expectedAccessToken.expires_in);
                result.IdToken.ShouldBe((string)expectedAccessToken.id_token);
                result.TokenType.ShouldBe((string)expectedAccessToken.token_type);
            }

            [Fact]
            public async Task GivenAnInvalidAuthorizationCode_GetAccessTokenFromProviderAsync_ReturnNoAccessToken()
            {
                // Arrange.
                var providerParams = new ProviderParams
                {
                    PublicApiKey = "some public api key",
                    SecretApiKey = "some secret api key"
                };
                var provider = new GoogleProviderTestClass(providerParams);
                const string authorizationCode = "aasdasds";
                var redirectUrl = new Uri("http://a.b.c.d");
                const string errorJson = "{\"error\" : \"invalid_grant\"}";
                var errorResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(errorJson, HttpStatusCode.BadRequest);
                HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(
                        "https://accounts.google.com/o/oauth2/token",
                        errorResponse);
                // Act.
                var result = await provider.GetAccessTokenFromProviderAsync(authorizationCode, redirectUrl);

                // Assert.
                result.ShouldBe(null);
            }
        }

        public class MapAccessTokenResultToAccessTokenFacts
        {
            [Fact]
            public void GivenAValidAccessTokenResult_MapAccessTokenResultToAccessToken_ReturnsAnAccessToken()
            {
                // Arrange.
                var providerParams = new ProviderParams
                {
                    PublicApiKey = "some public api key",
                    SecretApiKey = "some secret api key"
                };
                var provider = new GoogleProviderTestClass(providerParams);
                var accessTokenResult = new AccessTokenResult
                {
                    AccessToken = "aaa",
                    ExpiresIn = 222,
                    IdToken = "bbb",
                    TokenType = "ccc"
                };

                // Act.
                var result = provider.MapAccessTokenResultToAccessToken(accessTokenResult);

                // Assert.
                result.ShouldNotBe(null);
                result.ExpiresOn.ShouldBeGreaterThan(DateTime.UtcNow.AddSeconds(accessTokenResult.ExpiresIn - 10));
                result.PublicToken.ShouldBe(accessTokenResult.AccessToken);
                result.SecretToken.ShouldBeNullOrEmpty();
            }
        }

        public class RetrieveUserInformationAsyncFacts
        {
            [Fact]
            public async Task GivenAValidAccessToken_RetrieveUserInformationAsync_ReturnsSomeUserInformation()
            {
                // Arrange.
                var providerParams = new ProviderParams
                {
                    PublicApiKey = "some public api key",
                    SecretApiKey = "some secret api key"
                };
                var provider = new GoogleProviderTestClass(providerParams);
                var accessToken = new AccessToken
                {
                    ExpiresOn = DateTime.UtcNow,
                    PublicToken = "aaaaa",
                    SecretToken = null
                };
                var userInformationJson = File.ReadAllText("Sample Data\\Google-UserInfoResult-Content.json");
                var userinformationResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(userInformationJson);
                HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(
                        "https://www.googleapis.com/oauth2/v2/userinfo?access_token=aaaaa",
                        userinformationResponse);
                // 

                // Act.
                var result = await provider.RetrieveUserInformationAsync(accessToken);

                // Assert.
                result.ShouldNotBe(null);
                var expectedUserInfoResult = JsonConvert.DeserializeObject<UserInfoResult>(userInformationJson);
                result.Email.ShouldBe(expectedUserInfoResult.Email);
                result.Gender.ShouldBe(GenderTypeHelpers.ToGenderType(expectedUserInfoResult.Gender));
                result.Id.ShouldBe(expectedUserInfoResult.Id);
                result.Locale.ShouldBe(expectedUserInfoResult.Locale);
                result.Name.ShouldBe(expectedUserInfoResult.Name);
                result.Picture.ShouldBe(expectedUserInfoResult.Picture);
                result.UserName.ShouldBe(expectedUserInfoResult.GivenName);
            }
        }
    }
}
