using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Shouldly;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleAuthentication.Core.Providers.Facebook;
using Xunit;

namespace SimpleAuthentication.Tests.Providers
{
    public class FacebookProviderFacts
    {

        public class GetRedirectToAuthenticateSettingsFacts
        {
            [Fact]
            public void GivenACallbackUrl_GetRedirectToAuthenticateSettings_ReturnsSomeRedirectToAuthenticateSettings()
            {
                // Arrange.
                const string publicApiKey = "adskfhsd kds j k&^%*&^%*%/\\/\\/\\/111";
                const string secretApiKey = "xxxxxxxxx asdsad as das kds j k&^%*&^%*%/\\/\\/\\/111";
                var provider = new FacebookProvider(new ProviderParams(publicApiKey, secretApiKey));
                var callbackUrl = new Uri("http://www.mywebsite.com/auth/callback?provider=facebookz0r");

                // Arrange.
                var result = provider.GetRedirectToAuthenticateSettings(callbackUrl);

                // Assert.
                result.RedirectUri.AbsoluteUri.ShouldBe(
                    string.Format("https://www.facebook.com/dialog/oauth?client_id=adskfhsd%20kds%20j%20k%26%5E%25%2A%26%5E%25%2A%25%2F%5C%2F%5C%2F%5C%2F111&redirect_uri=http%3A%2F%2Fwww.mywebsite.com%2Fauth%2Fcallback%3Fprovider%3Dfacebookz0r&response_type=code&state={0}",
                    result.State));
                result.State.ShouldNotBeNullOrEmpty();
            }
        }

        public class AuthenticateClientAsyncFacts
        {
            [Fact]
            public async Task GivenSomeValidCallbackData_AuthenticateClientAsync_ReturnsSomeUserInformation()
            {
                // Arrange.
                const string publicApiKey = "adskfhsd kds j k&^%*&^%*%/\\/\\/\\/111";
                const string secretApiKey = "xxxxxxxxx asdsad as das kds j k&^%*&^%*%/\\/\\/\\/111";
                var provider = new FacebookProvider(new ProviderParams(publicApiKey, secretApiKey));
                const string state = "adyiuhj97&^*&shdgf\\//////\\dsf";

                var querystring = new Dictionary<string, string>
                {
                    {"state", state},
                    {"code", "4/P7q7W91a-oMsCeLvIaQm6bTrgtp7"}
                };
                var redirectUrl = new Uri("http://www.mywebsite.com/go/here/please");
                var accessTokenContent = File.ReadAllText("Sample Data\\Facebook-AccessToken-Content.txt");
                var keyValues = SystemHelpers.ConvertKeyValueContentToDictionary(accessTokenContent);
                var accessTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(accessTokenContent);
                var userInformationJson = File.ReadAllText("Sample Data\\Facebook-UserInfoResult-Content.json");
                var userInformationResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(userInformationJson);
                HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(
                    new Dictionary<string, HttpResponseMessage>
                    {
                        {"https://graph.facebook.com/oauth/access_token", accessTokenResponse},
                        {
                            string.Format("https://graph.facebook.com/v2.0/me?fields=id,name,gender,email,link,locale&access_token={0}", keyValues["access_token"]),
                            userInformationResponse
                        }
                    });

                // Arrange.
                var result = await provider.AuthenticateClientAsync(querystring, state, redirectUrl);

                // Assert.
                result.ProviderName.ShouldBe("Facebook");
                result.AccessToken.Token.ShouldBe(keyValues["access_token"]);
                result.UserInformation.Email.ShouldBe("foo@pewpew.com");
            }
        }

        /*
        public class FacebookProviderTestClass : FacebookProvider
        {
            public FacebookProviderTestClass(ProviderParams providerParams) : base(providerParams)
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
                var provider = new FacebookProvider(providerParams);
                var callBackUrl = new Uri("Http://www.foo.com/callback");
                
                // Act.
                var settings = provider.GetRedirectToAuthenticateSettings(callBackUrl);

                // Assert.
                settings.ShouldNotBe(null);
                settings.State.ShouldNotBeNullOrEmpty();
                Guid.Parse(settings.State);
                settings.RedirectUri.ShouldNotBe(null);
                settings.RedirectUri.AbsoluteUri.ShouldStartWith("https://www.facebook.com/dialog/oauth?client_id=some%20public%20api%20key&redirect_uri=http://www.foo.com/callback&response_type=code&scope=email&state=");
            }

            [Fact]
            public void GivenACallbackUrlAndThisIsAMobileSite_GetRedirectToAuthenticateSettings_ReturnsSomeRedirectToAuthenticateSettings()
            {
                // Arrange.
                var providerParams = new ProviderParams
                {
                    PublicApiKey = "some public api key",
                    SecretApiKey = "some secret api key"
                };
                var provider = new FacebookProvider(providerParams)
                {
                    IsMobile = true
                };
                var callBackUrl = new Uri("Http://www.foo.com/callback");

                // Act.
                var settings = provider.GetRedirectToAuthenticateSettings(callBackUrl);

                // Assert.
                settings.ShouldNotBe(null);
                settings.State.ShouldNotBeNullOrEmpty();
                Guid.Parse(settings.State);
                settings.RedirectUri.ShouldNotBe(null);
                settings.RedirectUri.AbsoluteUri.ShouldStartWith("https://m.facebook.com/dialog/oauth?client_id=some%20public%20api%20key&redirect_uri=http://www.foo.com/callback&response_type=code&scope=email&state=");
            }
        }


        public class GetAccessTokenFromProviderAsyncFacts
        {
            [Fact]
            public async Task
                GivenAValidAuthorizationCodeAndRedirectUri_GetAccessTokenFromProviderAsync_ReturnsAnAccessToken()
            {
                // Arrange.
                var providerParams = new ProviderParams
                {
                    PublicApiKey = "some public api key",
                    SecretApiKey = "some secret api key"
                };
                var provider = new FacebookProviderTestClass(providerParams);
                const string authorizationCode = "aasdasds";
                var redirectUrl = new Uri("http://a.b.c.d");
                var accessTokenJson = File.ReadAllText("Sample Data\\Facebook-AccessToken-Content.txt");
                var accessTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(accessTokenJson);
                HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(
                    "https://graph.facebook.com/oauth/access_token?client_id=some public api key&client_secret=some secret api key&redirect_uri=http:%2F%2Fa.b.c.d%2F&code=aasdasds&format=json",
                    accessTokenResponse);

                // Act.
                var result = await provider.GetAccessTokenFromProviderAsync(authorizationCode, redirectUrl);

                // Assert.
                result.ShouldNotBe(null);
                result.AccessToken.ShouldBe("CAAGsQgZADz7EBAMCMehE0nR2HpS8NJklENXZCg8r1Q36AcV94c9pQycDKYNdyWXb0oqNBXJOkZCy4hosYmv2hNHjjZB5q8ZAhKG2rfhndrBHWyKZC3W4hVZBLvsZAgtzdlu0T8khnUBNl0ZA1P56n7LFQP6tZA89zc14y3EllZByTJZAZA9KVrTydltQZBZCZAQPWAvyqyKquXGcMnYO2wxeNMGcv9Ps");
                result.Expires.ShouldBe(5183995);
            }
        }


        */



        /*
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
        */
    }
}
