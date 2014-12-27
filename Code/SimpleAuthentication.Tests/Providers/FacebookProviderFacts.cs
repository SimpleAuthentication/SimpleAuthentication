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
using WorldDomination.Net.Http;
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
                var provider = TestHelpers.AuthenticationProviders["facebook"];
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
                result.Message.ShouldBe("CSRF check fails: The callback 'state' value 'pewpew' doesn't match the server's *remembered* state value '**'.");
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
            public void GivenACallbackUrl_GetRedirectToAuthenticateSettings_ReturnsSomeRedirectToAuthenticateSettings()
            {
                // Arrange.
                var provider = (FacebookProvider)TestHelpers.AuthenticationProviders["facebook"]; 
                var callbackUrl = new Uri("http://www.mywebsite.com/auth/callback?provider=facebookz0r");

                // Arrange.
                var result = provider.GetRedirectToAuthenticateSettings(callbackUrl);

                // Assert.
                var queryStringSegments = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", TestHelpers.ConfigProviderKey),
                    new KeyValuePair<string, string>("redirect_uri", callbackUrl.AbsoluteUri),
                    new KeyValuePair<string, string>("response_type", "code"),
                    new KeyValuePair<string, string>("scope", "public_profile,email"),
                    new KeyValuePair<string, string>("state", result.State)
                }.ToEncodedString();
                var url = string.Format("https://www.facebook.com/dialog/oauth?{0}", queryStringSegments);
                result.RedirectUri.AbsoluteUri.ShouldBe(url);
                result.State.ShouldNotBeNullOrEmpty();
            }

            [Fact]
            public void GivenACallbackUrlAndIsMobile_GetRedirectToAuthenticateSettings_ReturnsSomeRedirectToAuthenticateSettings()
            {
                // Arrange.
                var provider = (FacebookProvider)TestHelpers.AuthenticationProviders["facebook"]; 
                provider.IsMobile = true;
                
                var callbackUrl = new Uri("http://www.mywebsite.com/auth/callback?provider=facebookz0r");

                // Arrange.
                var result = provider.GetRedirectToAuthenticateSettings(callbackUrl);

                // Assert.
                var queryStringSegments = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", TestHelpers.ConfigProviderKey),
                    new KeyValuePair<string, string>("redirect_uri", callbackUrl.AbsoluteUri),
                    new KeyValuePair<string, string>("response_type", "code"),
                    new KeyValuePair<string, string>("scope", "public_profile,email"),
                    new KeyValuePair<string, string>("state", result.State)
                }.ToEncodedString();
                var url = string.Format("https://m.facebook.com/dialog/oauth?{0}", queryStringSegments);
                result.RedirectUri.AbsoluteUri.ShouldBe(url);
                result.State.ShouldNotBeNullOrEmpty();
            }

            [Fact]
            public void GivenACallbackUrlAndADisplayType_GetRedirectToAuthenticateSettings_ReturnsSomeRedirectToAuthenticateSettings()
            {
                // Arrange.
                var provider = (FacebookProvider) TestHelpers.AuthenticationProviders["facebook"];
                provider.DisplayType = DisplayType.PopUp;
                
                var callbackUrl = new Uri("http://www.mywebsite.com/auth/callback?provider=facebookz0r");

                // Arrange.
                var result = provider.GetRedirectToAuthenticateSettings(callbackUrl);

                // Assert.
                var queryStringSegments = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", TestHelpers.ConfigProviderKey),
                    new KeyValuePair<string, string>("redirect_uri", callbackUrl.AbsoluteUri),
                    new KeyValuePair<string, string>("response_type", "code"),
                    new KeyValuePair<string, string>("scope", "public_profile,email"),
                    new KeyValuePair<string, string>("state", result.State),
                    new KeyValuePair<string, string>("display", "popup")
                }.ToEncodedString();
                var url = string.Format("https://www.facebook.com/dialog/oauth?{0}", queryStringSegments);
                result.RedirectUri.AbsoluteUri.ShouldBe(url);
                result.State.ShouldNotBeNullOrEmpty();
            }

        }
    }
}