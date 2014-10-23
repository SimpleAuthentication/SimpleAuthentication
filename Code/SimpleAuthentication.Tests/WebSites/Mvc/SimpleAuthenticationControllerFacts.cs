using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CsQuery;
using FakeItEasy;
using Shouldly;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Config;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Mvc;
using Xunit;

namespace SimpleAuthentication.Tests.WebSites.Mvc
{
    public class SimpleAuthenticationControllerFacts
    {
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

                var controller = new SimpleAuthenticationController(authenticationProviderCallback, configService)
                {
                    ControllerContext = controllerContext
                };

                const string providerName = "google";

                // Act.
                var exception = Should.Throw<AuthenticationException>(() => controller.RedirectToProvider(providerName));

                // Assert.
                exception.Message.ShouldBe("No Authentication Provider config settings where found. As such, we'll never be able to authenticate successfully against another service. How to fix this: add at least one Authentication Provider configuration data into your config file's <appSettings> section (recommended and easiest answer) [eg. <add key=\"sa.Google\" value=\"key:587140099194.apps.googleusercontent.com;secret:npk1_gx-gqJmLiJRPFooxCEY\"/> or add a custom config section to your .config file (looks a bit more pro, but is also a bit more complex to setup). For more info (especially the convention rules for the appSettings key/value> please refer to: ");
            }

            [Fact]
            public void GivenNoProviderNameRoute_RedirectToProvider_ThrowsAnException()
            {
                // Arrange.
                var authenticationProviderCallback = A.Fake<IAuthenticationProviderCallback>();
                var configService = A.Fake<IConfigService>();
                var controller = new SimpleAuthenticationController(authenticationProviderCallback, configService);

                // Act.
                var exception = Should.Throw<AuthenticationException>(() => controller.RedirectToProvider(null));

                // Assert.
                exception.Message.ShouldBe(
                    "ProviderName value missing. You need to supply a valid provider name so we know where to redirect the user Eg. ..../authenticate/google");
            }

            [Fact]
            public void GivenAProviderNameButNoReturnUrl_RedirectToProvider_ReturnsARedirectResult()
            {
                // Arrange.
                var authenticationProviderCallback = A.Fake<IAuthenticationProviderCallback>();

                var provider = new Provider
                {
                    Name = "Google",
                    Key = "some ** key",
                    Secret = "some secret"
                };
                var configuration = new Configuration()
                {
                    Providers = new[] {provider}
                };
                var configService = A.Fake<IConfigService>();
                A.CallTo(() => configService.GetConfiguration()).Returns(configuration);

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

                var provider = new Provider
                {
                    Name = "Google",
                    Key = "some ** key",
                    Secret = "some secret"
                };
                var configuration = new Configuration()
                {
                    Providers = new[] { provider }
                };
                var configService = A.Fake<IConfigService>();
                A.CallTo(() => configService.GetConfiguration()).Returns(configuration);

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
            //[Fact]
            //public void 
        }
    }
}