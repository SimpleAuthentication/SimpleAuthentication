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
using SimpleAuthentication.Core.Config;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers;
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
                    .Returns(TestHelpers.AuthenticationProvidersWithGoogle);

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
                    .Returns(TestHelpers.AuthenticationProvidersWithGoogle);
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
            public async Task GivenSomeFullData_AuthenticateCallbackAsync_ThrowsAnException()
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
                    .Returns(TestHelpers.AuthenticationProvidersWithGoogle);

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

        public class RedirectToProviderFacts
        {
            [Fact]
            public void GivenSomeRedirectToProviderData_RedirectToProvider_ReturnsARedirectResult()
            {
                // Arrange.
                var traceSource = new TraceSource("test");

                var authenticationProviderFactory = A.Fake<IAuthenticationProviderFactory>();
                A.CallTo(() => authenticationProviderFactory.AuthenticationProviders)
                    .Returns(TestHelpers.AuthenticationProvidersWithGoogle);

                var webApplicationService = new WebApplicationService(authenticationProviderFactory,
                    traceSource,
                    "asdjhsdkfhds");

                var redirectToProviderData = new RedirectToProviderData(TestHelpers.AuthenticationProvidersWithGoogle.First().Value.Name,
                    new Uri("http://www.pewpew.com/a/b/c"),
                    null,
                    null);

                // Act.
                var result = webApplicationService.RedirectToProvider(redirectToProviderData);

                // Assert.
                result.RedirectUrl.AbsoluteUri.ShouldStartWith(
                     "https://accounts.google.com/o/oauth2/auth?client_id=some%20%2A%2A%20key&redirect_uri=http%3A%2F%2Fwww.pewpew.com%2Fasdjhsdkfhds&response_type=code&scope=profile%20email&state=");
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
                    .Returns(TestHelpers.AuthenticationProvidersWithGoogle);

                var webApplicationService = new WebApplicationService(authenticationProviderFactory,
                    traceSource,
                    "asdjhsdkfhds");

                var redirectToProviderData = new RedirectToProviderData(TestHelpers.AuthenticationProvidersWithGoogle.First().Value.Name,
                    new Uri("http://www.pewpew.com/a/b/c"),
                    null,
                    "/foo/bar?a=b");

                // Act.
                var result = webApplicationService.RedirectToProvider(redirectToProviderData);

                // Assert.
                result.RedirectUrl.AbsoluteUri.ShouldStartWith(
                     "https://accounts.google.com/o/oauth2/auth?client_id=some%20%2A%2A%20key&redirect_uri=http%3A%2F%2Fwww.pewpew.com%2Fasdjhsdkfhds&response_type=code&scope=profile%20email&state=");
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
                    .Returns(TestHelpers.AuthenticationProvidersWithGoogle);

                var webApplicationService = new WebApplicationService(authenticationProviderFactory,
                    traceSource,
                    "asdjhsdkfhds");

                var redirectToProviderData = new RedirectToProviderData(TestHelpers.AuthenticationProvidersWithGoogle.First().Value.Name,
                    new Uri("http://www.pewpew.com/a/b/c"),
                    "http://www.aa.bb.com",
                    null);

                // Act.
                var result = webApplicationService.RedirectToProvider(redirectToProviderData);

                // Assert.
                result.RedirectUrl.AbsoluteUri.ShouldStartWith(
                      "https://accounts.google.com/o/oauth2/auth?client_id=some%20%2A%2A%20key&redirect_uri=http%3A%2F%2Fwww.pewpew.com%2Fasdjhsdkfhds&response_type=code&scope=profile%20email&state=");
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
                    .Returns(TestHelpers.AuthenticationProvidersWithGoogle);

                var webApplicationService = new WebApplicationService(authenticationProviderFactory,
                    traceSource,
                    "asdjhsdkfhds");

                var redirectToProviderData = new RedirectToProviderData(TestHelpers.AuthenticationProvidersWithGoogle.First().Value.Name,
                    new Uri("http://www.pewpew.com/a/b/c"),
                    "http://www.aa.bb.com",
                    "http://www.xxx.net/a/b/c?d=e&f=g");

                // Act.
                var result = webApplicationService.RedirectToProvider(redirectToProviderData);

                // Assert.
                result.RedirectUrl.AbsoluteUri.ShouldStartWith(
                      "https://accounts.google.com/o/oauth2/auth?client_id=some%20%2A%2A%20key&redirect_uri=http%3A%2F%2Fwww.pewpew.com%2Fasdjhsdkfhds&response_type=code&scope=profile%20email&state=");
                result.CacheData.ProviderKey.ShouldBe(redirectToProviderData.ProviderKey);
                result.CacheData.State.ShouldNotBe(null);
                result.CacheData.ReturnUrl.ShouldBe(redirectToProviderData.ReturnUrl);
            }
        }
    }
}