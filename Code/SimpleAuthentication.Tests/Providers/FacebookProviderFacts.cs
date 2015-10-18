using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Shouldly;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers;
using SimpleAuthentication.Core.Providers.Facebook;
using Xunit;

namespace SimpleAuthentication.Tests.Providers
{
    public class FacebookProviderFacts
    {
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
                            string.Format(
                                "https://graph.facebook.com/v2.0/me?fields=id,name,gender,email,link,locale&access_token={0}",
                                keyValues["access_token"]),
                            userInformationResponse
                        }
                    });

                // Arrange.
                var result = await provider.AuthenticateClientAsync(querystring, state, redirectUrl);

                // Assert.
                result.ProviderName.ShouldBe("Facebook");
                result.AccessToken.Token.ShouldBe(keyValues["access_token"]);
                result.UserInformation.Email.ShouldBe("foo@pewpew.com");
                result.RawUserInformation.ShouldNotBe(null);
            }

            [Fact]
            public void GivenAQuerystingMissingAStateKeyValue_AuthenticateClientAsync_ThrowsAnException()
            {
                // Arrange.
                var provider = new FacebookProvider(new ProviderParams("a", "b"));
                const string state = "hi";

                // NOTE: State is missing.
                var querystring = new Dictionary<string, string>
                {
                    {"code", "4/P7q7W91a-oMsCeLvIaQm6bTrgtp7"}
                };
                var redirectUrl = new Uri("http://www.mywebsite.com/go/here/please");

                // Arrange.
                var result =
                    Should.Throw<AuthenticationException>(
                        async () => await provider.AuthenticateClientAsync(querystring, state, redirectUrl));

                // Assert.
                result.Message.ShouldBe(
                    "The callback querystring doesn't include a state key/value parameter. We need one of these so we can do a CSRF check. Please check why the request url from the provider is missing the parameter: 'state'. eg. &state=something...");
            }

            [Fact]
            public void GivenAQuerystingStateDataWhichMismatchesSomeServerStateData_AuthenticateClientAsync_ThrowsAnException()
            {
                // Arrange.
                var provider = new FacebookProvider(new ProviderParams("a", "b"));
                const string state = "hi";

                var querystring = new Dictionary<string, string>
                {
                    {"state", "pewpew"},
                    {"code", "4/P7q7W91a-oMsCeLvIaQm6bTrgtp7"}
                };
                var redirectUrl = new Uri("http://www.mywebsite.com/go/here/please");

                // Arrange.
                var result = Should.Throw<AuthenticationException>(async () => await provider.AuthenticateClientAsync(querystring, state, redirectUrl));

                // Assert.
                result.Message.ShouldBe("CSRF check fails: The callback 'state' value 'pewpew' doesn't match the server's *remembered* state value 'hi.");
            }

            [Fact]
            public void GivenAQuerystingWithError_AuthenticateClientAsync_ThrowsAnException()
            {
                // Arrange.
                const string publicApiKey = "adskfhsd kds j k&^%*&^%*%/\\/\\/\\/111";
                const string secretApiKey = "xxxxxxxxx asdsad as das kds j k&^%*&^%*%/\\/\\/\\/111";
                var provider = new FacebookProvider(new ProviderParams(publicApiKey, secretApiKey));
                const string state = "adyiuhj97&^*&shdgf\\//////\\dsf";

                var querystring = new Dictionary<string, string>
                {
                    {"state", state},
                    {"error", "something bad happened at the provider. ru-roh."}
                };
                var redirectUrl = new Uri("http://www.mywebsite.com/go/here/please");

                // Arrange.
                var result = Should.Throw<AuthenticationException>(async () => await provider.AuthenticateClientAsync(querystring, state, redirectUrl));

                // Assert.
                result.Message.ShouldBe("Failed to retrieve an authorization code from Facebook. The error provided is: something bad happened at the provider. ru-roh.");
            }
        }

        public class GetRedirectToAuthenticateSettingsFacts
        {
            [Fact]
            public async Task GivenACallbackUrl_GetRedirectToAuthenticateSettingsAsync_ReturnsSomeRedirectToAuthenticateSettings()
            {
                // Arrange.
                const string publicApiKey = "adskfhsd kds j k&^%*&^%*%/\\/\\/\\/111";
                const string secretApiKey = "xxxxxxxxx asdsad as das kds j k&^%*&^%*%/\\/\\/\\/111";
                var provider = new FacebookProvider(new ProviderParams(publicApiKey, secretApiKey));
                var callbackUrl = new Uri("http://www.mywebsite.com/auth/callback?provider=facebookz0r");

                // Arrange.
                var result = await provider.GetRedirectToAuthenticateSettingsAsync(callbackUrl);

                // Assert.
                result.RedirectUri.AbsoluteUri.ShouldBe(
                    string.Format(
                        "https://www.facebook.com/dialog/oauth?client_id=adskfhsd%20kds%20j%20k%26%5E%25%2A%26%5E%25%2A%25%2F%5C%2F%5C%2F%5C%2F111&redirect_uri=http%3A%2F%2Fwww.mywebsite.com%2Fauth%2Fcallback%3Fprovider%3Dfacebookz0r&response_type=code&scope=public_profile%2Cemail&state={0}",
                        result.State));
                result.State.ShouldNotBeNullOrEmpty();
            }

            [Fact]
            public async Task GivenACallbackUrlAndIsMobile_GetRedirectToAuthenticateSettingsAsync_ReturnsSomeRedirectToAuthenticateSettings()
            {
                // Arrange.
                const string publicApiKey = "adskfhsd kds j k&^%*&^%*%/\\/\\/\\/111";
                const string secretApiKey = "xxxxxxxxx asdsad as das kds j k&^%*&^%*%/\\/\\/\\/111";
                var provider = new FacebookProvider(new ProviderParams(publicApiKey, secretApiKey))
                {
                    IsMobile = true
                };
                var callbackUrl = new Uri("http://www.mywebsite.com/auth/callback?provider=facebookz0r");

                // Arrange.
                var result = await provider.GetRedirectToAuthenticateSettingsAsync(callbackUrl);

                // Assert.
                result.RedirectUri.AbsoluteUri.ShouldBe(
                    string.Format(
                         "https://m.facebook.com/dialog/oauth?client_id=adskfhsd%20kds%20j%20k%26%5E%25%2A%26%5E%25%2A%25%2F%5C%2F%5C%2F%5C%2F111&redirect_uri=http%3A%2F%2Fwww.mywebsite.com%2Fauth%2Fcallback%3Fprovider%3Dfacebookz0r&response_type=code&scope=public_profile%2Cemail&state={0}",
                        result.State));
                result.State.ShouldNotBeNullOrEmpty();
            }

            [Fact]
            public async Task GivenACallbackUrlAndADisplayType_GetRedirectToAuthenticateSettingsAsync_ReturnsSomeRedirectToAuthenticateSettings()
            {
                // Arrange.
                const string publicApiKey = "adskfhsd kds j k&^%*&^%*%/\\/\\/\\/111";
                const string secretApiKey = "xxxxxxxxx asdsad as das kds j k&^%*&^%*%/\\/\\/\\/111";
                var provider = new FacebookProvider(new ProviderParams(publicApiKey, secretApiKey))
                {
                    DisplayType = DisplayType.PopUp
                };
                var callbackUrl = new Uri("http://www.mywebsite.com/auth/callback?provider=facebookz0r");

                // Arrange.
                var result = await provider.GetRedirectToAuthenticateSettingsAsync(callbackUrl);

                // Assert.
                result.RedirectUri.AbsoluteUri.ShouldBe(
                    string.Format(
                         "https://www.facebook.com/dialog/oauth?client_id=adskfhsd%20kds%20j%20k%26%5E%25%2A%26%5E%25%2A%25%2F%5C%2F%5C%2F%5C%2F111&redirect_uri=http%3A%2F%2Fwww.mywebsite.com%2Fauth%2Fcallback%3Fprovider%3Dfacebookz0r&response_type=code&scope=public_profile%2Cemail&state={0}&display=popup",
                        result.State));
                result.State.ShouldNotBeNullOrEmpty();
            }

        }
    }
}