using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Nancy.SimpleAuthentication;
using Shouldly;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Config;
using Xunit;

namespace SimpleAuthentication.Tests
{
    public class WebApplicationServiceFacts
    {
        public class RedirectToProviderFacts
        {
            [Fact(Skip = "Waiting for Phillip to help.")]
            public void GivenSomeRedirectToProviderData_RedirectToProvider_ReturnsARedirectResult()
            {
                //// Arrange.
                //var authenticationProviderCallback = A.Fake<IAuthenticationProviderCallback>();
                //var provider = new Provider
                //{
                //    Name = "Google",
                //    Key = "some ** key",
                //    Secret = "some secret"
                //};
                //var configuration = new Configuration
                //{
                //    Providers = new[]
                //    {
                //        provider
                //    }
                //};

                //var configService = A.Fake<IConfigService>();
                //A.CallTo(() => configService.GetConfiguration()).Returns(configuration);

                //var redirectToProviderData = new RedirectToProviderData(provider.Name,
                //    new Uri("http://www.pewpew.com/a/b/c"),
                //    null,
                //    null);

                //var traceSource = new TraceSource("test");

                //var webApplicationService = new WebApplicationService(authenticationProviderCallback,
                //    configService,
                //    traceSource,
                //    "asdasdasd",
                //    "axxxxxxxxxx");

                //// Act.
                //var result = webApplicationService.RedirectToProvider(redirectToProviderData);

                //// Assert.
                //result.RedirectUrl.AbsoluteUri.ShouldStartWith(
                //    "https://accounts.google.com/o/oauth2/auth?client_id=some%20%2A%2A%20key&redirect_uri=http%3A%2F%2Fwww.pewpew.com%2Faxxxxxxxxxx&response_type=code&scope=profile%20email&state=");
                //result.CacheData.ProviderKey.ShouldBe(redirectToProviderData.ProviderKey);
                //result.CacheData.State.ShouldNotBe(null);
                //result.CacheData.ReturnUrl.ShouldBe(null);
            }

            [Fact(Skip = "Waiting for Phillip to help.")]
            public void GivenSomeRedirectToProviderDataIncludingAReturnUrl_GetRedirectToProvider_ReturnsARedirectResponse()
            {
                //// Arrange.
                //var authenticationProviderCallback = A.Fake<IAuthenticationProviderCallback>();
                //var provider = new Provider
                //{
                //    Name = "Google",
                //    Key = "some ** key",
                //    Secret = "some secret"
                //};
                //var configuration = new Configuration
                //{
                //    Providers = new[]
                //    {
                //        provider
                //    }
                //};

                //var configService = A.Fake<IConfigService>();
                //A.CallTo(() => configService.GetConfiguration()).Returns(configuration);

                //var redirectToProviderData = new RedirectToProviderData(provider.Name,
                //    new Uri("http://www.pewpew.com/a/b/c"),
                //    null,
                //    "/foo/bar?a=b");

                //var traceSource = new TraceSource("test");

                //var webApplicationService = new WebApplicationService(authenticationProviderCallback,
                //    configService,
                //    traceSource,
                //    "asdasdasd",
                //    "axxxxxxxxxx");

                //// Act.
                //var result = webApplicationService.RedirectToProvider(redirectToProviderData);

                //// Assert.
                //result.RedirectUrl.AbsoluteUri.ShouldStartWith(
                //    "https://accounts.google.com/o/oauth2/auth?client_id=some%20%2A%2A%20key&redirect_uri=http%3A%2F%2Fwww.pewpew.com%2Faxxxxxxxxxx&response_type=code&scope=profile%20email&state=");
                //result.CacheData.ProviderKey.ShouldBe(redirectToProviderData.ProviderKey);
                //result.CacheData.State.ShouldNotBe(null);
                //result.CacheData.ReturnUrl.ShouldBe(redirectToProviderData.ReturnUrl);
            }

            [Fact(Skip = "Waiting for Phillip to help.")]
            public void GivenSomeRedirectToProviderDataIncludingAReferer_GetRedirectToProvider_ReturnsARedirectResponse()
            {
                //// Arrange.
                //var authenticationProviderCallback = A.Fake<IAuthenticationProviderCallback>();
                //var provider = new Provider
                //{
                //    Name = "Google",
                //    Key = "some ** key",
                //    Secret = "some secret"
                //};
                //var configuration = new Configuration
                //{
                //    Providers = new[]
                //    {
                //        provider
                //    }
                //};

                //var configService = A.Fake<IConfigService>();
                //A.CallTo(() => configService.GetConfiguration()).Returns(configuration);

                //var redirectToProviderData = new RedirectToProviderData(provider.Name,
                //    new Uri("http://www.pewpew.com/a/b/c"),
                //    "http://www.aa.bb.com",
                //    null);

                //var traceSource = new TraceSource("test");

                //var webApplicationService = new WebApplicationService(authenticationProviderCallback,
                //    configService,
                //    traceSource,
                //    "asdasdasd",
                //    "axxxxxxxxxx");

                //// Act.
                //var result = webApplicationService.RedirectToProvider(redirectToProviderData);

                //// Assert.
                //result.RedirectUrl.AbsoluteUri.ShouldStartWith(
                //    "https://accounts.google.com/o/oauth2/auth?client_id=some%20%2A%2A%20key&redirect_uri=http%3A%2F%2Fwww.pewpew.com%2Faxxxxxxxxxx&response_type=code&scope=profile%20email&state=");
                //result.CacheData.ProviderKey.ShouldBe(redirectToProviderData.ProviderKey);
                //result.CacheData.State.ShouldNotBe(null);
                //result.CacheData.ReturnUrl.ShouldBe(redirectToProviderData.Referer);
            }

            [Fact(Skip = "Waiting for Phillip to help.")]
            public void GivenSomeRedirectToProviderDataIncludingAReturnUrlAndAReferer_GetRedirectToProvider_ReturnsARedirectResponse()
            {
                //// Arrange.
                //var authenticationProviderCallback = A.Fake<IAuthenticationProviderCallback>();
                //var provider = new Provider
                //{
                //    Name = "Google",
                //    Key = "some ** key",
                //    Secret = "some secret"
                //};
                //var configuration = new Configuration
                //{
                //    Providers = new[]
                //    {
                //        provider
                //    }
                //};

                //var configService = A.Fake<IConfigService>();
                //A.CallTo(() => configService.GetConfiguration()).Returns(configuration);

                //var redirectToProviderData = new RedirectToProviderData(provider.Name,
                //    new Uri("http://www.pewpew.com/a/b/c"),
                //    "http://www.aa.bb.com",
                //    "http://www.xxx.net/a/b/c?d=e&f=g");

                //var traceSource = new TraceSource("test");

                //var webApplicationService = new WebApplicationService(authenticationProviderCallback,
                //    configService,
                //    traceSource,
                //    "asdasdasd",
                //    "axxxxxxxxxx");

                //// Act.
                //var result = webApplicationService.RedirectToProvider(redirectToProviderData);

                //// Assert.
                //result.RedirectUrl.AbsoluteUri.ShouldStartWith(
                //    "https://accounts.google.com/o/oauth2/auth?client_id=some%20%2A%2A%20key&redirect_uri=http%3A%2F%2Fwww.pewpew.com%2Faxxxxxxxxxx&response_type=code&scope=profile%20email&state=");
                //result.CacheData.ProviderKey.ShouldBe(redirectToProviderData.ProviderKey);
                //result.CacheData.State.ShouldNotBe(null);
                //result.CacheData.ReturnUrl.ShouldBe(redirectToProviderData.ReturnUrl);
            }
        }

        public class AuthenticateCallbackAsyncFacts
        {
            [Fact(Skip = "Waiting for Phillip to help.")]
            public async Task GivenNoSession_AuthenticateCallbackAsync_ThrowsAnException()
            {
                //// Arrange.
                //var authenticationProviderCallback = A.Fake<IAuthenticationProviderCallback>();
                //var provider = new Provider
                //{
                //    Name = "Google",
                //    Key = "some ** key",
                //    Secret = "some secret"
                //};
                //var configuration = new Configuration
                //{
                //    Providers = new[]
                //    {
                //        provider
                //    }
                //};

                //var configService = A.Fake<IConfigService>();
                //A.CallTo(() => configService.GetConfiguration()).Returns(configuration);

                //var traceSource = new TraceSource("test");

                //var webApplicationService = new WebApplicationService(configService,
                //    traceSource,
                //    "asdasdasd");

                //var requestUrl = new Uri("http://a.b.c/");
                //var cacheData = new CacheData("google",
                //    "some state",
                //    null);
                //var queryParamters = new Dictionary<string, string>();
                //var authenticateCallbackAsyncData = new AuthenticateCallbackAsyncData(requestUrl,
                //    cacheData,
                //    queryParamters);

                //// Act.
                //var result = await webApplicationService.AuthenticateCallbackAsync(authenticateCallbackAsyncData);

                //// Assert.
            }
        }
    }
}