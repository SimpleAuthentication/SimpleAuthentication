using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FakeItEasy;
using Shouldly;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Config;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Mvc;
using WorldDomination.Net.Http;
using Xunit;

namespace SimpleAuthentication.Tests.WebSites.Mvc
{
    public class SimpleAuthenticationControllerFacts
    {
        public static Configuration ConfigurationWithGoogleProvider
        {
            get
            {
                var provider = new Provider
                {
                    Name = "Google",
                    Key = "some ** key",
                    Secret = "some secret"
                };
                
                return new Configuration
                {
                    Providers = new[]
                    {
                        provider
                    }
                };
            }
        }

        public class RedirectToProviderFacts
        {
            [Fact]
            public void GivenNoAppSettings_RedirectToProvider_ThrowsAnException()
            {
                // Arrange.
                var authenticationProviderCallback = A.Fake<IAuthenticationProviderCallback>();
                var configService = A.Fake<IConfigService>();

                var request = A.Fake<HttpRequestBase>();
                //A.CallTo(() => request.QueryString["returnUrl"]).Returns(null);
                A.CallTo(() => request.QueryString).Returns(new NameValueCollection());
                A.CallTo(() => request.UrlReferrer).Returns(null);
                var requestUrl = new Uri("http://localhost:45216/SimpleAuthentication/RedirectToProvider?providerName=google");
                A.CallTo(() => request.Url).Returns(requestUrl);
                A.CallTo(() => request.UrlReferrer).Returns(new Uri(requestUrl.Authority));
                var httpContext = A.Fake<HttpContextBase>();
                A.CallTo(() => httpContext.Request).Returns(request);
                var controllerContext = new ControllerContext(httpContext, new RouteData(), A.Fake<ControllerBase>());

                // Act.
                var exception = Should.Throw<AuthenticationException>(() =>
                    new SimpleAuthenticationController(authenticationProviderCallback, configService)
                    {
                        ControllerContext = controllerContext
                    });

                // Assert.
                exception.Message.ShouldBe("No Authentication Provider config settings where found. As such, we'll never be able to authenticate successfully against another service. How to fix this: add at least one Authentication Provider configuration data into your config file's <appSettings> section (recommended and easiest answer) [eg. <add key=\"sa.Google\" value=\"key:587140099194.apps.googleusercontent.com;secret:npk1_gx-gqJmLiJRPFooxCEY\"/> or add a custom config section to your .config file (looks a bit more pro, but is also a bit more complex to setup). For more info (especially the convention rules for the appSettings key/value> please refer to: ");
            }

            [Fact]
            public void GivenNoProviderNameRoute_RedirectToProvider_ThrowsAnException()
            {
                // Arrange.
                var authenticationProviderCallback = A.Fake<IAuthenticationProviderCallback>();
                var configService = A.Fake<IConfigService>();
                A.CallTo(() => configService.GetConfiguration()).Returns(ConfigurationWithGoogleProvider);

                var controller = new SimpleAuthenticationController(authenticationProviderCallback, configService);

                // Act.
                var exception = Should.Throw<AuthenticationException>(() => controller.RedirectToProvider(null));

                // Assert.
                exception.Message.ShouldBe(
                    "ProviderName value missing. You need to supply a valid provider name so we know where to redirect the user Eg. ....authenticate/google");
            }

            [Fact]
            public void GivenAProviderNameButNoReturnUrl_RedirectToProvider_ReturnsARedirectResult()
            {
                // Arrange.
                var authenticationProviderCallback = A.Fake<IAuthenticationProviderCallback>();
                var configService = A.Fake<IConfigService>();
                A.CallTo(() => configService.GetConfiguration()).Returns(ConfigurationWithGoogleProvider);

                var request = A.Fake<HttpRequestBase>();
                A.CallTo(() => request.QueryString).Returns(new NameValueCollection());
                A.CallTo(() => request.UrlReferrer).Returns(null);
                var requestUrl = new Uri("http://localhost:45216/SimpleAuthentication/RedirectToProvider?providerName=google");
                A.CallTo(() => request.Url).Returns(requestUrl);
                A.CallTo(() => request.UrlReferrer).Returns(new Uri(requestUrl.Authority));
                var httpContext = A.Fake<HttpContextBase>();
                A.CallTo(() => httpContext.Request).Returns(request);
                var controllerContext = new ControllerContext(httpContext, new RouteData(), A.Fake<ControllerBase>());

                var controller = new SimpleAuthenticationController(authenticationProviderCallback, configService)
                {
                    ControllerContext = controllerContext
                };

                const string providerName = "google";
                
                // Act.
                var result = controller.RedirectToProvider(providerName);

                // Assert.
                result.Permanent.ShouldBe(false);
                result.Url.ShouldStartWith("https://accounts.google.com/o/oauth2/auth?client_id=some%20%2A%2A%20key&redirect_uri=http%3A%2F%2Flocalhost%3A45216%2Fauthenticate%2Fcallback&response_type=code&scope=profile%20email&state=");
                var cacheData =
                    controller.Session["SimpleAuthentication-StateKey-427B6ED7-A803-4F18-A396-0084417B548D"] as
                        CacheData;
                cacheData.ProviderKey.ShouldBe(providerName);
                cacheData.ReturnUrl.ShouldBe(requestUrl.Authority);
                cacheData.State.ShouldNotBe(null);
            }

            [Fact]
            public void GivenAProviderNameWithAReturnUrl_RedirectToProvider_ReturnsARedirectResult()
            {
                // Arrange.
                var authenticationProviderCallback = A.Fake<IAuthenticationProviderCallback>();

                var configService = A.Fake<IConfigService>();
                A.CallTo(() => configService.GetConfiguration()).Returns(ConfigurationWithGoogleProvider);

                var request = A.Fake<HttpRequestBase>();
                //A.CallTo(() => request.QueryString["returnUrl"]).Returns(null);
                const string returnUrl = "/foo/bar?a=1234&b=xxx";
                var queryString = new NameValueCollection {{"returnUrl", returnUrl}};
                A.CallTo(() => request.QueryString).Returns(queryString);
                A.CallTo(() => request.UrlReferrer).Returns(null);
                var requestUrl = new Uri("http://localhost:45216/SimpleAuthentication/RedirectToProvider?providerName=google");
                A.CallTo(() => request.Url).Returns(requestUrl);
                A.CallTo(() => request.UrlReferrer).Returns(new Uri(requestUrl.Authority));
                var httpContext = A.Fake<HttpContextBase>();
                A.CallTo(() => httpContext.Request).Returns(request);
                var controllerContext = new ControllerContext(httpContext, new RouteData(), A.Fake<ControllerBase>());

                var controller = new SimpleAuthenticationController(authenticationProviderCallback, configService)
                {
                    ControllerContext = controllerContext
                };

                const string providerName = "google";

                // Act.
                var result = controller.RedirectToProvider(providerName);

                // Assert.
                result.Permanent.ShouldBe(false);
                result.Url.ShouldStartWith("https://accounts.google.com/o/oauth2/auth?client_id=some%20%2A%2A%20key&redirect_uri=http%3A%2F%2Flocalhost%3A45216%2Fauthenticate%2Fcallback&response_type=code&scope=profile%20email&state=");
                var cacheData =
                    controller.Session["SimpleAuthentication-StateKey-427B6ED7-A803-4F18-A396-0084417B548D"] as
                        CacheData;
                cacheData.ProviderKey.ShouldBe(providerName);
                cacheData.ReturnUrl.ShouldBe(returnUrl);
                cacheData.State.ShouldNotBe(null);
            }
        }

        public class AuthenticateCallbackResultAsyncFacts
        {
            [Fact]
            public void GivenNoSession_AuthenticateCallbackResultAsync_ThrowsAnException()
            {
                // Arrange.
                var authenticationProviderCallback = A.Fake<IAuthenticationProviderCallback>();

                var configService = A.Fake<IConfigService>();
                A.CallTo(() => configService.GetConfiguration()).Returns(ConfigurationWithGoogleProvider);

                var session = A.Fake<HttpSessionStateBase>();
                A.CallTo(() => session["SimpleAuthentication-StateKey-427B6ED7-A803-4F18-A396-0084417B548D"])
                    .Returns(null);
                var httpContext = A.Fake<HttpContextBase>();
                A.CallTo(() => httpContext.Session).Returns(session);

                var controllerContext = new ControllerContext(httpContext, new RouteData(), A.Fake<ControllerBase>());

                var controller = new SimpleAuthenticationController(authenticationProviderCallback, configService)
                {
                    ControllerContext = controllerContext
                };

                // Act.
                var exception =
                    Should.Throw<AuthenticationException>(async () => await controller.AuthenticateCallback());

                // Assert.
                exception.Message.ShouldBe("No cache data or cached State value was found which generally means that a Cross Site Request Forgery attempt might be made. A 'State' value is generated by the server when a client prepares to redirect to an Authentication Provider and passes that generated state value to that Provider. The provider then passes that state value back, which proves that the client (ie. that's -you-) have actually authenticated against a provider. Otherwise, anyone can just hit the callback Url and impersonate another user, bypassing the authentication stage. So what's the solution: make sure you call the 'RedirectToProvider' endpoint *before* you hit the 'AuthenticateCallbackAsync' callback endpoint.");
            }

            [Fact]
            public void GivenNoStateInTheQueryString_AuthenticateCallbackResultAsync_ThrowsAnException()
            {
                // Arrange.
                var authenticationProviderCallback = A.Fake<IAuthenticationProviderCallback>();

                var configService = A.Fake<IConfigService>();
                var configuration = ConfigurationWithGoogleProvider;
                A.CallTo(() => configService.GetConfiguration()).Returns(configuration);

                var request = A.Fake<HttpRequestBase>();
                A.CallTo(() => request.QueryString).Returns(new NameValueCollection());
                A.CallTo(() => request.Url).Returns(new Uri("http://www.foo.com"));

                var cacheData = new CacheData(configuration.Providers.First().Name, "asdadsds", null);
                var session = A.Fake<HttpSessionStateBase>();
                A.CallTo(() => session["SimpleAuthentication-StateKey-427B6ED7-A803-4F18-A396-0084417B548D"])
                    .Returns(cacheData);

                var httpContext = A.Fake<HttpContextBase>();
                A.CallTo(() => httpContext.Request).Returns(request);
                A.CallTo(() => httpContext.Session).Returns(session);
                var controllerContext = new ControllerContext(httpContext, new RouteData(), A.Fake<ControllerBase>());

                var controller = new SimpleAuthenticationController(authenticationProviderCallback, configService)
                {
                    ControllerContext = controllerContext
                };

                // Act.
                var exception =
                    Should.Throw<AuthenticationException>(async () => await controller.AuthenticateCallback());

                // Assert.
                exception.Message.ShouldBe("The callback querystring doesn't include a state key/value parameter. We need one of these so we can do a CSRF check. Please check why the request url from the provider is missing the parameter: 'state'. eg. &state=something...");
            }

            [Fact]
            public async Task GivenAValidCallback_AuthenticateCallbackResultAsync_ThrowsAnException()
            {
                // Arrange.
                const string redirectResultUrl ="http://www.dancedancepewpew.com/a/b/c";
                var redirectResult = new RedirectResult(redirectResultUrl);
                var authenticationProviderCallback = A.Fake<IAuthenticationProviderCallback>();
                A.CallTo(() => authenticationProviderCallback.Process(A<Controller>._,
                    A<AuthenticateCallbackResult>._)).Returns(redirectResult);
                const string state = "barbra streisand";

                var configService = A.Fake<IConfigService>();
                var configuration = ConfigurationWithGoogleProvider;
                A.CallTo(() => configService.GetConfiguration()).Returns(configuration);

                var queryString = new NameValueCollection
                {
                    {"state", state},
                    {"code", "offline"}
                };
                var request = A.Fake<HttpRequestBase>();
                A.CallTo(() => request.QueryString).Returns(queryString);
                A.CallTo(() => request.Url).Returns(new Uri("http://www.foo.com"));

                var cacheData = new CacheData(configuration.Providers.First().Name, state, null);
                var session = A.Fake<HttpSessionStateBase>();
                A.CallTo(() => session["SimpleAuthentication-StateKey-427B6ED7-A803-4F18-A396-0084417B548D"])
                    .Returns(cacheData);

                var httpContext = A.Fake<HttpContextBase>();
                A.CallTo(() => httpContext.Request).Returns(request);
                A.CallTo(() => httpContext.Session).Returns(session);
                var controllerContext = new ControllerContext(httpContext, new RouteData(), A.Fake<ControllerBase>());

                var controller = new SimpleAuthenticationController(authenticationProviderCallback, configService)
                {
                    ControllerContext = controllerContext
                };

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

                // Act.
                var result = await controller.AuthenticateCallback() as RedirectResult;

                // Assert.
                result.Url.ShouldBe(redirectResultUrl);
                A.CallTo(() => authenticationProviderCallback.Process(A<Controller>._, 
                    A<AuthenticateCallbackResult>._)).MustHaveHappened(Repeated.Exactly.Once);
            }
        }
    }
}