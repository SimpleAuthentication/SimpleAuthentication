using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using FakeItEasy;
using Nancy;
using Nancy.SimpleAuthentication;
using Nancy.Testing;
using Shouldly;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Config;
using SimpleAuthentication.Core.Providers;
using WorldDomination.Net.Http;
using Xunit;

namespace SimpleAuthentication.Tests.WebSites.Nancy
{
    public class SimpleAuthenticationModuleFacts
    {
        public class GetRedirectToProviderFacts : NancyModuleTestBase<SimpleAuthenticationModule>
        {
            [Fact]
            public void GivenAValidProviderKey_GetRedirectToProvider_ReturnsARedirectResponse()
            {
                // Arrange.
                var authenticationCallbackProvider = A.Fake<IAuthenticationProviderCallback>();

                var providers = new[]
                {
                    new Provider
                    {
                        Name = "Google",
                        Key = "some.*.(.). key",
                        Secret = "some secret"
                    }
                };

                var configuration = new Configuration
                {
                    Providers = providers
                };

                var configService = A.Fake<IConfigService>();
                A.CallTo(() => configService.GetConfiguration())
                    .Returns(configuration);

                var providerScanner = A.Fake<IProviderScanner>();
                A.CallTo(() => providerScanner.GetDiscoveredProviders())
                    .Returns(new[] {typeof (GoogleProvider)});

                var authenticationProviderFactory = new AuthenticationProviderFactory(configService,
                    providerScanner);
                AddModuleDependency(typeof (IAuthenticationProviderFactory), authenticationProviderFactory);
                AddModuleDependency(typeof(IAuthenticationProviderCallback), authenticationCallbackProvider);
                
                var browser = Browser();

                // Act.
                var result = browser.Get("/authenticate/google", with =>
                {
                    with.HttpRequest();
                    with.HostName("foo.com");
                });

                // Assert.
                result.ShouldNotBe(null);
                result.StatusCode.ShouldBe(HttpStatusCode.SeeOther);
                result.Body.AsString().ShouldBeNullOrEmpty();
                result.Headers.Count.ShouldBe(1);
                result.Headers["Location"]
                    .ShouldStartWith(
                        "https://accounts.google.com/o/oauth2/auth?client_id=some%20%2A%2A%20key&redirect_uri=http%3A%2F%2Ffoo.com%2Fauthenticate%2Fcallback&response_type=code&scope=profile%20email&state=");
                var cacheData =
                    (CacheData)result.Context.Request.Session["SimpleAuthentication-StateKey-cf92a651-d638-4ce4-a393-f612d3be4c3a"];
                cacheData.ProviderKey.ShouldBe("google");
                cacheData.State.ShouldNotBe(null);
                cacheData.ReturnUrl.ShouldBe(null);
            }

            //[Fact]
            //public void GivenAMissingProviderKey_GetRedirectToProvider_ReturnsANotFound()
            //{
            //    // Arrange.
            //    var authenticationCallbackProvider = A.Fake<IAuthenticationProviderCallback>();
            //    var configService = A.Fake<IConfigService>();

            //    AddModuleDependency(typeof(IAuthenticationProviderCallback), authenticationCallbackProvider);
            //    AddModuleDependency(typeof(IConfigService), configService);

            //    var browser = Browser();

            //    // Act.
            //    var result = browser.Get("/authenticate/", with => with.HttpRequest());

            //    // Assert.
            //    result.ShouldNotBe(null);
            //    result.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            //}

            //[Fact]
            //public void GivenAValidProviderKeyAndAValidIdentifier_GetRedirectToProvider_ReturnsARedirectResponse()
            //{
            //    // Arrange.
            //    var authenticationCallbackProvider = A.Fake<IAuthenticationProviderCallback>();

            //    var configuration = new Configuration
            //    {
            //        Providers = new[]
            //        {
            //            new Provider
            //            {
            //                Name = "Google",
            //                Key = "some key",
            //                Secret = "some secret"
            //            }
            //        }
            //    };
            //    var configService = A.Fake<IConfigService>();
            //    A.CallTo(() => configService.GetConfiguration()).Returns(configuration);

            //    AddModuleDependency(typeof(IAuthenticationProviderCallback), authenticationCallbackProvider);
            //    AddModuleDependency(typeof(IConfigService), configService);

            //    var browser = Browser();

            //    // Act.
            //    var result = browser.Get("/authenticate/google", with =>
            //    {
            //        with.HttpRequest();
            //        with.HostName("www.pewpew.com");
            //    });

            //    // Assert.
            //    result.ShouldNotBe(null);
            //    result.StatusCode.ShouldBe(HttpStatusCode.SeeOther);
            //    result.Headers.Count.ShouldBe(1);
            //    result.Headers["Location"].ShouldStartWith("https://accounts.google.com/o/oauth2/auth?client_id=some%20key&redirect_uri=http%3A%2F%2Fwww.pewpew.com%2Fauthenticate%2Fcallback&response_type=code&scope=profile%20email&state=");
            //}

            //[Fact]
            //public void GivenAValidProviderKeyAndAReferrerHeader_GetRedirectToProvider_ReturnsARedirectResponse()
            //{
            //    // Arrange.
            //    var authenticationCallbackProvider = A.Fake<IAuthenticationProviderCallback>();

            //    var configuration = new Configuration
            //    {
            //        Providers = new[]
            //        {
            //            new Provider
            //            {
            //                Name = "Google",
            //                Key = "some key",
            //                Secret = "some secret"
            //            }
            //        }
            //    };
            //    var configService = A.Fake<IConfigService>();
            //    A.CallTo(() => configService.GetConfiguration()).Returns(configuration);

            //    AddModuleDependency(typeof(IAuthenticationProviderCallback), authenticationCallbackProvider);
            //    AddModuleDependency(typeof(IConfigService), configService);

            //    var browser = Browser();

            //    // Act.
            //    var result = browser.Get("/authenticate/google", with =>
            //    {
            //        with.HttpRequest();
            //        with.HostName("www.pewpew.com");
            //        with.Header("Referer", "/homepage");
            //    });

            //    // Assert.
            //    result.ShouldNotBe(null);
            //    result.StatusCode.ShouldBe(HttpStatusCode.SeeOther);
            //    result.Headers.Count.ShouldBe(1);
            //    result.Headers["Location"].ShouldStartWith("https://accounts.google.com/o/oauth2/auth?client_id=some%20key&redirect_uri=http%3A%2F%2Fwww.pewpew.com%2Fauthenticate%2Fcallback&response_type=code&scope=profile%20email&state=");
            //}
        }

        //public class GetAuthenticateCallbackFacts : NancyModuleTestBase<SimpleAuthenticationModule>
        //{
        //    [Fact]
        //    public void GivenNoSessionCacheData_GetAuthenticateCallback_ReturnsAnErrorResponse()
        //    {
        //        // Arrange.
        //        var authenticationCallbackProvider = A.Fake<IAuthenticationProviderCallback>();
        //        var configService = A.Fake<IConfigService>();

        //        AddModuleDependency(typeof(IAuthenticationProviderCallback), authenticationCallbackProvider);
        //        AddModuleDependency(typeof(IConfigService), configService);

        //        var browser = Browser();

        //        // Act.
        //        var exception = Should.Throw<Exception>(() =>
        //            browser.Get("/authenticate/callback", with => with.HttpRequest()));

        //        // Assert.
        //        exception.ShouldNotBe(null);
        //        exception.InnerException.InnerException.Message.ShouldBe("No cache data or cached State value was found which generally means that a Cross Site Request Forgery attempt might be made. A 'State' value is generated by the server when a client prepares to redirect to an Authentication Provider and passes that generated state value to that Provider. The provider then passes that state value back, which proves that the client (ie. that's -you-) have actually authenticated against a provider. Otherwise, anyone can just hit the callback Url and impersonate another user, bypassing the authentication stage. So what's the solution: make sure you call the 'RedirectToProvider' endpoint *before* you hit the 'AuthenticateCallbackAsync' callback endpoint.");
        //    }

        //    [Fact]
        //    public void GivenAValidAuthenticatedUser_GetAuthenticateCallback_ReturnsARedirectStatus()
        //    {
        //        // Arrange.
        //        const string returnUrl = "/";
        //        const HttpStatusCode expectedStatusCode = HttpStatusCode.SeeOther;
        //        var formatter = A.Fake<IResponseFormatter>();
        //        var redirectResponse = formatter.AsRedirect(returnUrl);
        //        var authenticationCallbackProvider = A.Fake<IAuthenticationProviderCallback>();
        //        A.CallTo(() => authenticationCallbackProvider
        //            .Process(A<AuthenticateCallbackResult>._))
        //            .Returns(redirectResponse);

        //        var configuration = new Configuration
        //        {
        //            Providers = new[]
        //            {
        //                new Provider
        //                {
        //                    Name = "Google",
        //                    Key = "some key",
        //                    Secret = "some secret"
        //                }
        //            }
        //        };

        //        var configService = A.Fake<IConfigService>();
        //        A.CallTo(() => configService.GetConfiguration()).Returns(configuration);
        //        var accessTokenJson = File.ReadAllText("Sample Data\\Google-AccessToken-Content.json");
        //        var userInformationJson = File.ReadAllText("Sample Data\\Google-UserInfoResult-Content.json");

        //        var accessTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(accessTokenJson);
        //        var userInformationResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(userInformationJson);
        //        HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(
        //            new Dictionary<string, HttpResponseMessage>
        //            {
        //                {"https://accounts.google.com/o/oauth2/token", accessTokenResponse },
        //                {"https://www.googleapis.com/plus/v1/people/me?access_token=", userInformationResponse}
        //            });

        //        MappedDependencies = new List<Tuple<Type, object>>
        //        {
        //            new Tuple<Type, object>(typeof(IAuthenticationProviderCallback), authenticationCallbackProvider),
        //            new Tuple<Type, object>(typeof(IConfigService), configService),
        //        };

        //        const string state = "foo";
        //        var cacheData = new CacheData("Google", state, returnUrl);
        //        const string sessionKey = "SimpleAuthentication-StateKey-cf92a651-d638-4ce4-a393-f612d3be4c3a";
        //        var session = new Dictionary<string, object>
        //        {
        //            {sessionKey, cacheData}
        //        };
        //        var browser = Browser(session: session);

        //        // Act.
        //        var response = browser.Get("/authenticate/callback", with =>
        //        {
        //            with.HttpRequest();
        //            with.Query("state", state);
        //            with.Query("code", "special_code_from_google");
        //            with.HostName("foo.com");
        //        });

        //        // Assert.
        //        response.ShouldNotBe(null);
        //        response.StatusCode.ShouldBe(expectedStatusCode);
        //        response.Headers["Location"].ShouldBe(returnUrl);
        //        response.Context.Request.Session[sessionKey].ShouldBe(null);
        //    }

        //    [Fact]
        //    public void GivenAMissingCachedStateValue_GetAuthenticateCallback_ThrowsAnException()
        //    {
        //        // Arrange.
        //        var authenticationCallbackProvider = A.Fake<IAuthenticationProviderCallback>();
        //        var configuration = new Configuration
        //        {
        //            Providers = new[]
        //            {
        //                new Provider
        //                {
        //                    Name = "Google",
        //                    Key = "some key",
        //                    Secret = "some secret"
        //                }
        //            }
        //        };

        //        var configService = A.Fake<IConfigService>();
        //        A.CallTo(() => configService.GetConfiguration()).Returns(configuration);

        //        AddModuleDependency(typeof(IAuthenticationProviderCallback), authenticationCallbackProvider);
        //        AddModuleDependency(typeof(IConfigService), configService);

        //        var browser = Browser();

        //        // Act.
        //        var result = Assert.Throws<Exception>(() =>
        //        {
        //            browser.Get("/authenticate/callback", with =>
        //            {
        //                with.HttpRequest();
        //                with.Query("providerkey", "google");
        //                with.HostName("foo.com");
        //            });
        //        });

        //        // Assert.
        //        result.ShouldNotBe(null);
        //        result.InnerException.ShouldNotBe(null);
        //        result.InnerException.InnerException.ShouldNotBe(null);
        //        result.InnerException.InnerException.Message.ShouldBe(
        //            "No cache data or cached State value was found which generally means that a Cross Site Request Forgery attempt might be made. A 'State' value is generated by the server when a client prepares to redirect to an Authentication Provider and passes that generated state value to that Provider. The provider then passes that state value back, which proves that the client (ie. that's -you-) have actually authenticated against a provider. Otherwise, anyone can just hit the callback Url and impersonate another user, bypassing the authentication stage. So what's the solution: make sure you call the 'RedirectToProvider' endpoint *before* you hit the 'AuthenticateCallbackAsync' callback endpoint.");
        //    }
        //}
    }
}