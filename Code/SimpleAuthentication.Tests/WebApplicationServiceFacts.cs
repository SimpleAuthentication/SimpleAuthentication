using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FakeItEasy;
using Nancy;
using Nancy.SimpleAuthentication;
using Shouldly;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;
using WorldDomination.Net.Http;
using Xunit;

namespace SimpleAuthentication.Tests
{
    public class WebApplicationServiceFacts
    {
        public class AuthenticateCallbackAsyncFacts
        {
            private static AuthenticateCallbackAsyncData FakeAuthenticateCallbackAsyncData
            {
                get
                {
                    const string state = "qweryt*%%$*/.;'";
                    var requestUrl = new Uri("http://www.foo.com.au");
                    var cacheData = new CacheData("google", state, "someReturnUrl");
                    var queryStringValues = new Dictionary<string, string>
                    {
                        {"state", state},
                        {"code", "offline"},
                        {"a", "b"}
                    };
                    return new AuthenticateCallbackAsyncData(requestUrl,
                        cacheData,
                        queryStringValues);
                }
            }

            [Fact]
            public void GivenNoStateKeyInTheQueryString_AuthenticateCallbackAsync_ThrowsAnException()
            {
                // Arrange.
                var authenticationProviderFactory = A.Fake<IAuthenticationProviderFactory>();
                A.CallTo(() => authenticationProviderFactory.AuthenticationProviders)
                    .Returns(TestHelpers.AuthenticationProviders);

                const string callbackResult = "this is someview content!";
                var authenticationCallbackProvider = A.Fake<IAuthenticationProviderCallback>();
                A.CallTo(() => authenticationCallbackProvider.Process(A<INancyModule>._,
                    A<AuthenticateCallbackResult>._)).Returns(callbackResult);

                var nancyModule = A.Fake<INancyModule>();

                var traceSource = new TraceSource("test");

                var webApplicationService = new WebApplicationService(authenticationProviderFactory,
                    traceSource,
                    "some callback route");

                var authenticationCallbackAsyncData = new AuthenticateCallbackAsyncData(
                    new Uri("http://a.b.c"),
                    new CacheData("Google", "b", null),
                    new Dictionary<string, string>());

                // Act.
                var exception = Should.Throw<AuthenticationException>(async () =>
                    await webApplicationService.AuthenticateCallbackAsync<IAuthenticationProviderCallback,
                        INancyModule,
                        dynamic>(authenticationCallbackProvider,
                            nancyModule,
                            authenticationCallbackAsyncData));

                // Assert.
                exception.Message.ShouldBe(
                    "The callback querystring doesn't include a state key/value parameter. We need one of these so we can do a CSRF check. Please check why the request url from the provider is missing the parameter: 'state'. eg. &state=something...");
            }

            [Fact]
            public void GivenAStateKeysthatDoNotMatch_AuthenticateCallbackAsync_ThrowsAnException()
            {
                // Arrange.
                const string callbackResult = "this is someview content!";
                var authenticationCallbackProvider = A.Fake<IAuthenticationProviderCallback>();
                A.CallTo(() => authenticationCallbackProvider.Process(A<INancyModule>._,
                    A<AuthenticateCallbackResult>._)).Returns(callbackResult);
                var nancyModule = A.Fake<INancyModule>();

                var traceSource = new TraceSource("test");
                var authenticationProviderFactory = A.Fake<IAuthenticationProviderFactory>();
                A.CallTo(() => authenticationProviderFactory.AuthenticationProviders)
                    .Returns(TestHelpers.AuthenticationProviders);
                var webApplicationService = new WebApplicationService(authenticationProviderFactory,
                    traceSource,
                    "some callback route");

                var authenticationCallbackAsyncData = new AuthenticateCallbackAsyncData(
                    new Uri("http://a.b.c"),
                    new CacheData("Google", "bbbb", null),
                    new Dictionary<string, string>
                    {
                        {"state", "asdasd"}
                    });

                // Act.
                var exception = Should.Throw<AuthenticationException>(async () =>
                    await webApplicationService.AuthenticateCallbackAsync<IAuthenticationProviderCallback,
                        INancyModule,
                        dynamic>(authenticationCallbackProvider,
                            nancyModule,
                            authenticationCallbackAsyncData));

                // Assert.
                exception.Message.ShouldBe(
                    "CSRF check fails: The callback 'state' value 'asdasd' doesn't match the server's *remembered* state value '****'.");
            }

            [Fact]
            public async Task GivenSomeFullData_AuthenticateCallbackAsync_ACallbackResult()
            {
                // Arrange.
                const string callbackResult = "this is someview content!";
                var authenticationCallbackProvider = A.Fake<IAuthenticationProviderCallback>();
                A.CallTo(() => authenticationCallbackProvider.Process(A<INancyModule>._,
                    A<AuthenticateCallbackResult>._)).Returns(callbackResult);
                var nancyModule = A.Fake<INancyModule>();

                var accessTokenJson = File.ReadAllText("Sample Data\\Google-AccessToken-Content.json");
                var accessTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(accessTokenJson);
                var userInformationJson = File.ReadAllText("Sample Data\\Google-UserInfoResult-Content.json");
                var userInformationResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(userInformationJson);
                HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(
                    new Dictionary<string, HttpResponseMessage>
                    {
                        {"https://accounts.google.com/o/oauth2/token", accessTokenResponse},
                        {
                            "https://www.googleapis.com/plus/v1/people/me?access_token=ya29.MwAjlO-LAHrX3RoAAABjuR4Tt5Ctgp8PvfK5RN8RURPjQW_dYL5Hu7-hETXapw",
                            userInformationResponse
                        }
                    });

                var traceSource = new TraceSource("test");
                var authenticationProviderFactory = A.Fake<IAuthenticationProviderFactory>();
                A.CallTo(() => authenticationProviderFactory.AuthenticationProviders)
                    .Returns(TestHelpers.AuthenticationProviders);

                var webApplicationService = new WebApplicationService(authenticationProviderFactory,
                    traceSource,
                    "some callback route");

                // Act.
                var result = await webApplicationService.AuthenticateCallbackAsync<IAuthenticationProviderCallback,
                    INancyModule,
                    dynamic>(authenticationCallbackProvider,
                        nancyModule,
                        FakeAuthenticateCallbackAsyncData);

                // Assert.
                ((string) result).ShouldBe(callbackResult);
            }
        }

        public class AuthenticateMeAsyncFacts
        {
            [Fact]
            public void GivenNoProviderKey_AuthenticateMeAsync_ThrowsAnException()
            {
                // Arrange.
                var traceSource = new TraceSource("test");

                var authenticationProviderFactory = A.Fake<IAuthenticationProviderFactory>();
                var webApplicationService = new WebApplicationService(authenticationProviderFactory,
                    traceSource,
                    "asdjhsdkfhds");

                // Act and Assert.
                var result =
                    Should.Throw<ArgumentNullException>(
                        async () => await webApplicationService.AuthenticateMeAsync(null, new AccessToken()));
            }

            [Fact]
            public void GivenNoAccessToken_AuthenticateMeAsync_ThrowsAnException()
            {
                // Arrange.
                var traceSource = new TraceSource("test");

                var authenticationProviderFactory = A.Fake<IAuthenticationProviderFactory>();
                var webApplicationService = new WebApplicationService(authenticationProviderFactory,
                    traceSource,
                    "asdjhsdkfhds");

                // Act and Assert.
                var result =
                    Should.Throw<ArgumentNullException>(
                        async () => await webApplicationService.AuthenticateMeAsync("aaa", null));
            }

            [Fact]
            public async Task GivenSomeValidCredentials_AuthenticateMeAsync_ReturnsAnAuthenticatedClient()
            {
                // Arrange.
                var traceSource = new TraceSource("test");

                var accessToken = new AccessToken
                {
                    Token = "12C63F29-67CD-4FDD-97FA-95529EFA9758"
                };

                var authenticationProviderFactory = A.Fake<IAuthenticationProviderFactory>();
                A.CallTo(() => authenticationProviderFactory.AuthenticationProviders)
                    .Returns(TestHelpers.AuthenticationProviders);

                var userInformationJson = File.ReadAllText("Sample Data\\Google-UserInfoResult-Content.json");
                var userInformationResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(userInformationJson);
                HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(
                    new Dictionary<string, HttpResponseMessage>
                    {
                        {
                            "https://www.googleapis.com/plus/v1/people/me?access_token=" + accessToken.Token,
                            userInformationResponse
                        }
                    });

                var webApplicationService = new WebApplicationService(authenticationProviderFactory,
                    traceSource,
                    "asdjhsdkfhds");

                // Act.
                var result = await webApplicationService.AuthenticateMeAsync("google", accessToken);

                // Assert.
                result.UserInformation.Email.ShouldBe("foo@pewpew.com");
            }
        }

        public class RedirectToProviderFacts
        {
            [Fact]
            public void GivenSomeRedirectToProviderData_RedirectToProvider_ReturnsARedirectResult()
            {
                // Arrange.
                var traceSource = new TraceSource("test");
                var googleProvider = TestHelpers.AuthenticationProviders.Single(x => x.Key == "google");
                var authenticationProviderFactory = A.Fake<IAuthenticationProviderFactory>();
                A.CallTo(() => authenticationProviderFactory.AuthenticationProviders)
                    .Returns(TestHelpers.AuthenticationProviders);

                const string callbackRoute = "asdjhsdkfhds";
                var webApplicationService = new WebApplicationService(authenticationProviderFactory,
                    traceSource,
                    callbackRoute);

                const string redirectUrl = "http://www.pewpew.com/a/b/c";
                var redirectToProviderData =
                    new RedirectToProviderData(googleProvider.Key,
                        new Uri(redirectUrl),
                        null,
                        null);

                // Act.
                var result = webApplicationService.RedirectToProvider(redirectToProviderData);

                // Assert.
                var queryStringSegments = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", TestHelpers.ConfigProviderKey),
                    new KeyValuePair<string, string>("redirect_uri", "http://www.pewpew.com/" + callbackRoute),
                    new KeyValuePair<string, string>("response_type", "code"),
                    new KeyValuePair<string, string>("scope", "profile email"),
                    new KeyValuePair<string, string>("state", string.Empty)
                }.ToEncodedString();
                var url = string.Format("https://accounts.google.com/o/oauth2/auth?{0}", queryStringSegments);
                result.RedirectUrl.AbsoluteUri.ShouldStartWith(url);
                result.CacheData.ProviderKey.ShouldBe(redirectToProviderData.ProviderKey);
                result.CacheData.State.ShouldNotBe(null);
                result.CacheData.ReturnUrl.ShouldBe(null);
            }

            [Fact]
            public void
                GivenSomeRedirectToProviderDataIncludingAReturnUrl_GetRedirectToProvider_ReturnsARedirectResponse()
            {
                // Arrange.
                var traceSource = new TraceSource("test");

                var authenticationProviderFactory = A.Fake<IAuthenticationProviderFactory>();
                A.CallTo(() => authenticationProviderFactory.AuthenticationProviders)
                    .Returns(TestHelpers.AuthenticationProviders);

                var webApplicationService = new WebApplicationService(authenticationProviderFactory,
                    traceSource,
                    "asdjhsdkfhds");

                var redirectToProviderData =
                    new RedirectToProviderData(TestHelpers.AuthenticationProviders.First().Value.Name,
                        new Uri("http://www.pewpew.com/a/b/c"),
                        null,
                        "/foo/bar?a=b");

                // Act.
                var result = webApplicationService.RedirectToProvider(redirectToProviderData);

                // Assert.
                var queryStringSegments = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", TestHelpers.ConfigProviderKey),
                    new KeyValuePair<string, string>("redirect_uri", "http://www.pewpew.com/asdjhsdkfhds"),
                    new KeyValuePair<string, string>("response_type", "code"),
                    new KeyValuePair<string, string>("scope", "profile email"),
                    new KeyValuePair<string, string>("state", string.Empty)
                }.ToEncodedString();
                var url = string.Format("https://accounts.google.com/o/oauth2/auth?{0}", queryStringSegments);
                result.RedirectUrl.AbsoluteUri.ShouldStartWith(url);
                result.CacheData.ProviderKey.ShouldBe(redirectToProviderData.ProviderKey);
                result.CacheData.State.ShouldNotBe(null);
                result.CacheData.ReturnUrl.ShouldBe(redirectToProviderData.ReturnUrl);
            }

            [Fact]
            public void GivenSomeRedirectToProviderDataIncludingAReferer_GetRedirectToProvider_ReturnsARedirectResponse()
            {
                // Arrange.
                var traceSource = new TraceSource("test");

                var authenticationProviderFactory = A.Fake<IAuthenticationProviderFactory>();
                A.CallTo(() => authenticationProviderFactory.AuthenticationProviders)
                    .Returns(TestHelpers.AuthenticationProviders);

                const string callbackRoute = "asdjhsdkfhds";
                var webApplicationService = new WebApplicationService(authenticationProviderFactory,
                    traceSource,
                    callbackRoute);

                var redirectToProviderData =
                    new RedirectToProviderData("google",
                        new Uri("http://www.pewpew.com/a/b/c"),
                        "http://www.aa.bb.com",
                        null);

                // Act.
                var result = webApplicationService.RedirectToProvider(redirectToProviderData);

                // Assert.
                var queryStringSegments = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", TestHelpers.ConfigProviderKey),
                    new KeyValuePair<string, string>("redirect_uri", "http://www.pewpew.com/" + callbackRoute),
                    new KeyValuePair<string, string>("response_type", "code"),
                    new KeyValuePair<string, string>("scope", "profile email"),
                    new KeyValuePair<string, string>("state", string.Empty)
                }.ToEncodedString();
                var url = string.Format("https://accounts.google.com/o/oauth2/auth?{0}", queryStringSegments);
                result.RedirectUrl.AbsoluteUri.ShouldStartWith(url);
                result.CacheData.ProviderKey.ShouldBe(redirectToProviderData.ProviderKey);
                result.CacheData.State.ShouldNotBe(null);
                result.CacheData.ReturnUrl.ShouldBe(redirectToProviderData.Referer);
            }

            [Fact]
            public void
                GivenSomeRedirectToProviderDataIncludingAReturnUrlAndAReferer_GetRedirectToProvider_ReturnsARedirectResponse
                ()
            {
                // Arrange.
                var traceSource = new TraceSource("test");

                var authenticationProviderFactory = A.Fake<IAuthenticationProviderFactory>();
                A.CallTo(() => authenticationProviderFactory.AuthenticationProviders)
                    .Returns(TestHelpers.AuthenticationProviders);

                var webApplicationService = new WebApplicationService(authenticationProviderFactory,
                    traceSource,
                    "asdjhsdkfhds");

                var redirectToProviderData =
                    new RedirectToProviderData("google",
                        new Uri("http://www.pewpew.com/a/b/c"),
                        "http://www.aa.bb.com",
                        "http://www.xxx.net/a/b/c?d=e&f=g");

                // Act.
                var result = webApplicationService.RedirectToProvider(redirectToProviderData);

                // Assert.
                var queryStringSegments = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", TestHelpers.ConfigProviderKey),
                    new KeyValuePair<string, string>("redirect_uri", "http://www.pewpew.com/asdjhsdkfhds"),
                    new KeyValuePair<string, string>("response_type", "code"),
                    new KeyValuePair<string, string>("scope", "profile email"),
                    new KeyValuePair<string, string>("state", string.Empty)
                }.ToEncodedString();
                var url = string.Format("https://accounts.google.com/o/oauth2/auth?{0}", queryStringSegments);
                result.RedirectUrl.AbsoluteUri.ShouldStartWith(url);
                result.CacheData.ProviderKey.ShouldBe(redirectToProviderData.ProviderKey);
                result.CacheData.State.ShouldNotBe(null);
                result.CacheData.ReturnUrl.ShouldBe(redirectToProviderData.ReturnUrl);
            }
        }
    }
}