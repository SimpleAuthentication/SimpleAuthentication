using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Shouldly;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers;
using WorldDomination.Net.Http;
using Xunit;

namespace SimpleAuthentication.Tests.Providers
{
    public class GoogleProviderFacts
    {
        public class GetRedirectToAuthenticateSettingsFacts
        {
            [Fact]
            public void GivenACallbackUrl_GetRedirectToAuthenticateSettings_ReturnsSomeRedirectToAuthenticateSettings()
            {
                // Arrange.
                const string publicApiKey = "adskfhsd kds j k&^%*&^%*%/\\/\\/\\/111";
                const string secretApiKey = "xxxxxxxxx asdsad as das kds j k&^%*&^%*%/\\/\\/\\/111";
                var provider = new GoogleProvider(new ProviderParams(publicApiKey, secretApiKey));
                var callbackUrl = new Uri("http://www.mywebsite.com/auth/callback?provider=googlez0r");

                // Arrange.
                var result = provider.GetRedirectToAuthenticateSettings(callbackUrl);

                // Assert.
                result.RedirectUri.AbsoluteUri.ShouldStartWith(
                    string.Format("https://accounts.google.com/o/oauth2/auth?client_id=adskfhsd%20kds%20j%20k%26%5E%25%2A%26%5E%25%2A%25%2F%5C%2F%5C%2F%5C%2F111&redirect_uri=http%3A%2F%2Fwww.mywebsite.com%2Fauth%2Fcallback%3Fprovider%3Dgooglez0r&response_type=code&scope=profile%20email&state={0}",
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
                var provider = new GoogleProvider(new ProviderParams(publicApiKey, secretApiKey));
                const string stateKey = "state";
                const string state = "adyiuhj97&^*&shdgf\\//////\\dsf";
                var querystring = new Dictionary<string, string>
                {
                    {stateKey, state},
                    {"code", "4/P7q7W91a-oMsCeLvIaQm6bTrgtp7"}
                };
                var redirectUrl = new Uri("http://www.mywebsite.com/go/here/please");
                var accessTokenJson = File.ReadAllText("Sample Data\\Google-AccessToken-Content.json");
                var accessTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(accessTokenJson); 
                var userInformationJson = File.ReadAllText("Sample Data\\Google-UserInfoResult-Content.json");
                var userInformationResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(userInformationJson); 
                HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(
                    new Dictionary<string, HttpResponseMessage>
                    {
                        {"https://accounts.google.com/o/oauth2/token", accessTokenResponse},
                        {"https://www.googleapis.com/plus/v1/people/me?access_token=ya29.MwAjlO-LAHrX3RoAAABjuR4Tt5Ctgp8PvfK5RN8RURPjQW_dYL5Hu7-hETXapw", userInformationResponse}
                    });

                // Arrange.
                var result = await provider.AuthenticateClientAsync(querystring, state, redirectUrl);

                // Assert.
                result.ProviderName.ShouldBe("Google");
                result.AccessToken.Token.ShouldBe("ya29.MwAjlO-LAHrX3RoAAABjuR4Tt5Ctgp8PvfK5RN8RURPjQW_dYL5Hu7-hETXapw");
                result.UserInformation.Email.ShouldBe("foo@pewpew.com");
                result.RawUserInformation.ShouldNotBe(null);
            }

            [Fact]
            public void GivenAQuerystingMissingAStateKeyValue_AuthenticateClientAsync_ThrowsAnException()
            {
                // Arrange.
                var provider = new GoogleProvider(new ProviderParams("a", "b"));
                const string state = "hi";

                // NOTE: State is missing.
                var querystring = new Dictionary<string, string>
                {
                    {"code", "4/P7q7W91a-oMsCeLvIaQm6bTrgtp7"}
                };
                var redirectUrl = new Uri("http://www.mywebsite.com/go/here/please");
            
                // Arrange.
                var result = Should.Throw<AuthenticationException>(async () => await provider.AuthenticateClientAsync(querystring, state, redirectUrl));

                // Assert.
                result.Message.ShouldBe("The callback querystring doesn't include a state key/value parameter. We need one of these so we can do a CSRF check. Please check why the request url from the provider is missing the parameter: 'state'. eg. &state=something...");
            }

            [Fact]
            public void GivenAQuerystingStateDataWhichMismatchesSomeServerStateData_AuthenticateClientAsync_ThrowsAnException()
            {
                // Arrange.
                var provider = new GoogleProvider(new ProviderParams("a", "b"));
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
                var provider = new GoogleProvider(new ProviderParams(publicApiKey, secretApiKey));
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
                result.Message.ShouldBe("Failed to retrieve an authorization code from Google. The error provided is: something bad happened at the provider. ru-roh.");
            }
        }
    }
}