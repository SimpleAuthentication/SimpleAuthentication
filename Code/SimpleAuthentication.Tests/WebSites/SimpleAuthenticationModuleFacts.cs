using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CsQuery;
using FakeItEasy;
using Nancy;
using Nancy.SimpleAuthentication;
using Nancy.Testing;
using Shouldly;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Config;
using SimpleAuthentication.Core.Providers;
using SimpleAuthentication.Core.Providers.Google;
using Xunit;

namespace SimpleAuthentication.Tests.WebSites
{
    public class SimpleAuthenticationModuleFacts
    {
        public class GetRedirectToProviderFacts : NancyModuleTestBase<SimpleAuthenticationModule>
        {
            [Fact]
            public void GivenAValidProviderKey_GetRedirectToProvider_ReturnsARedirectResponse()
            {
                // Arrange.
                var authenticationCallbackProvider = A.Fake<IAuthenticationCallbackProvider>();
                var configuration = new Configuration
                {
                    Providers = new []
                    {
                        new Provider
                        {
                            Name = "Google",
                            Key = "some key",
                            Secret = "some secret"
                        }
                    }
                };

                var configService = A.Fake<IConfigService>();
                A.CallTo(() => configService.GetConfiguration()).Returns(configuration);
                MappedDependencies = new List<Tuple<Type, object>>
                {
                    new Tuple<Type, object>(typeof(IAuthenticationCallbackProvider), authenticationCallbackProvider),
                    new Tuple<Type, object>(typeof(IConfigService), configService)
                };

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
                    .ShouldStartWith("https://accounts.google.com/o/oauth2/auth?client_id=some%20key&redirect_uri=http%3A%2F%2Ffoo.com%2Fauthenticate%2Fcallback%3Fproviderkey%3Dgoogle&response_type=code&scope=https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fuserinfo.profile%20https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fuserinfo.email&state=");
            }

            [Fact]
            public void GivenAMissingProviderKey_GetRedirectToProvider_ReturnsANotFound()
            {
                // Arrange.
                var authenticationCallbackProvider = A.Fake<IAuthenticationCallbackProvider>();
                var configService = A.Fake<IConfigService>();

                MappedDependencies = new List<Tuple<Type, object>>
                {
                    new Tuple<Type, object>(typeof(IAuthenticationCallbackProvider), authenticationCallbackProvider),
                    new Tuple<Type, object>(typeof(IConfigService), configService)
                };

                var browser = Browser();

                // Act.
                var result = browser.Get("/authenticate/", with => with.HttpRequest());

                // Assert.
                result.ShouldNotBe(null);
                result.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            }

            [Fact]
            public void GivenAValidProviderKeyAndAValidIdentifier_GetRedirectToProvider_ReturnsARedirectResponse()
            {
                // Arrange.
                var authenticationCallbackProvider = A.Fake<IAuthenticationCallbackProvider>();

                var configuration = new Configuration
                {
                    Providers = new[]
                    {
                        new Provider
                        {
                            Name = "Google",
                            Key = "some key",
                            Secret = "some secret"
                        }
                    }
                };
                var configService = A.Fake<IConfigService>();
                A.CallTo(() => configService.GetConfiguration()).Returns(configuration);

                MappedDependencies = new List<Tuple<Type, object>>
                {
                    new Tuple<Type, object>(typeof(IAuthenticationCallbackProvider), authenticationCallbackProvider),
                    new Tuple<Type, object>(typeof(IConfigService), configService)
                };

                var browser = Browser();

                // Act.
                var result = browser.Get("/authenticate/google", with =>
                {
                    with.HttpRequest();
                    with.HostName("www.pewpew.com");
                });

                // Assert.
                result.ShouldNotBe(null);
                result.StatusCode.ShouldBe(HttpStatusCode.SeeOther);
                result.Headers.Count.ShouldBe(1);
                result.Headers["Location"].ShouldStartWith("https://accounts.google.com/o/oauth2/auth?client_id=some%20key&redirect_uri=http%3A%2F%2Fwww.pewpew.com%2Fauthenticate%2Fcallback%3Fproviderkey%3Dgoogle&response_type=code&scope=https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fuserinfo.profile%20https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fuserinfo.email&state=");
            }

            [Fact]
            public void GivenAValidProviderKeyAndAReferrerHeader_GetRedirectToProvider_ReturnsARedirectResponse()
            {
                // Arrange.
                var authenticationCallbackProvider = A.Fake<IAuthenticationCallbackProvider>();

                var configuration = new Configuration
                {
                    Providers = new[]
                    {
                        new Provider
                        {
                            Name = "Google",
                            Key = "some key",
                            Secret = "some secret"
                        }
                    }
                };
                var configService = A.Fake<IConfigService>();
                A.CallTo(() => configService.GetConfiguration()).Returns(configuration);

                MappedDependencies = new List<Tuple<Type, object>>
                {
                    new Tuple<Type, object>(typeof(IAuthenticationCallbackProvider), authenticationCallbackProvider),
                    new Tuple<Type, object>(typeof(IConfigService), configService)
                };

                var browser = Browser();

                // Act.
                var result = browser.Get("/authenticate/google", with =>
                {
                    with.HttpRequest();
                    with.HostName("www.pewpew.com");
                    with.Header("Referer", "/homepage");
                });

                // Assert.
                result.ShouldNotBe(null);
                result.StatusCode.ShouldBe(HttpStatusCode.SeeOther);
                result.Headers.Count.ShouldBe(1);
                result.Headers["Location"].ShouldStartWith("https://accounts.google.com/o/oauth2/auth?client_id=some%20key&redirect_uri=http%3A%2F%2Fwww.pewpew.com%2Fauthenticate%2Fcallback%3Fproviderkey%3Dgoogle&response_type=code&scope=https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fuserinfo.profile%20https%3A%2F%2Fwww.googleapis.com%2Fauth%2Fuserinfo.email&state=");
            }
        }

        public class AuthenticateCallbackAsyncFacts : NancyModuleTestBase<SimpleAuthenticationModule>
        {
            [Fact]
            public void GivenAValidAuthenticatedUser_AuthenticateCallbackAsync_ReturnsARedirectStatus()
            {
                // Arrange.
                const HttpStatusCode expectedStatusCode = HttpStatusCode.SeeOther;
                var authenticationCallbackProvider = A.Fake<IAuthenticationCallbackProvider>();
                A.CallTo(() => authenticationCallbackProvider
                    .Process(A<NancyModule>._, A<AuthenticateCallbackResult>._))
                    .Returns(new Response
                    {
                        StatusCode = expectedStatusCode
                    });
                var configuration = new Configuration
                {
                    Providers = new[]
                    {
                        new Provider
                        {
                            Name = "Google",
                            Key = "some key",
                            Secret = "some secret"
                        }
                    }
                };

                var configService = A.Fake<IConfigService>();
                A.CallTo(() => configService.GetConfiguration()).Returns(configuration);

                var accessTokenJson = File.ReadAllText("Sample Data\\Google-AccessToken-Content.json");
                var userInformationJson = File.ReadAllText("Sample Data\\Google-UserInfoResult-Content.json");
                
                var accessTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(accessTokenJson);
                var userInformationResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(userInformationJson);
                HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(
                    new Dictionary<string, HttpResponseMessage>
                    {
                        {"https://accounts.google.com/o/oauth2/token", accessTokenResponse },
                        {"https://www.googleapis.com/oauth2/v2/userinfo?access_token=", userInformationResponse}
                    });
                    
                MappedDependencies = new List<Tuple<Type, object>>
                {
                    new Tuple<Type, object>(typeof(IAuthenticationCallbackProvider), authenticationCallbackProvider),
                    new Tuple<Type, object>(typeof(IConfigService), configService),
                };

                const string state = "foo";
                var session = new Dictionary<string, object>
                {
                    {"SimpleAuthentication-StateKey-cf92a651-d638-4ce4-a393-f612d3be4c3a", state}
                };
                var browser = Browser(session: session);

                // Act.
                var result = browser.Get("/authenticate/callback", with =>
                {
                    with.HttpRequest();
                    with.Query("providerkey", "google");
                    with.Query("state", state);
                    with.Query("code", "special_code_from_google");
                    with.HostName("foo.com");
                });

                // Assert.
                result.ShouldNotBe(null);
                result.StatusCode.ShouldBe(expectedStatusCode);
            }

            [Fact]
            public void GivenAMissingQueryStringProviderName_AuthenticateCallbackAsync_ThrowsAnException()
            {
                // Arrange.
                var authenticationCallbackProvider = A.Fake<IAuthenticationCallbackProvider>();
                var configuration = new Configuration
                {
                    Providers = new[]
                    {
                        new Provider
                        {
                            Name = "Google",
                            Key = "some key",
                            Secret = "some secret"
                        }
                    }
                };

                var configService = A.Fake<IConfigService>();
                A.CallTo(() => configService.GetConfiguration()).Returns(configuration);
                MappedDependencies = new List<Tuple<Type, object>>
                {
                    new Tuple<Type, object>(typeof(IAuthenticationCallbackProvider), authenticationCallbackProvider),
                    new Tuple<Type, object>(typeof(IConfigService), configService)
                };

                var browser = Browser();

                // Act.
                // NOTE: No OnError is wired up to the bootstrap, so no HTTP 500 status is returned.
                var result = Assert.Throws<Exception>(() =>
                {
                    browser.Get("/authenticate/callback", with =>
                    {
                        with.HttpRequest();
                        with.HostName("foo.com");
                    });
                });

                // Assert.
                result.ShouldNotBe(null);
                result.InnerException.ShouldNotBe(null);
                result.InnerException.InnerException.ShouldNotBe(null);
                result.InnerException.InnerException.Message.ShouldBe("ProviderKey value missing. You need to supply a valid provider key so we know where to redirect the user Eg. providerkey=google.");
            }

            [Fact]
            public void GivenAMissingCachedStateValue_AuthenticateCallbackAsync_ThrowsAnException()
            {
                // Arrange.
                var authenticationCallbackProvider = A.Fake<IAuthenticationCallbackProvider>();
                var configuration = new Configuration
                {
                    Providers = new[]
                    {
                        new Provider
                        {
                            Name = "Google",
                            Key = "some key",
                            Secret = "some secret"
                        }
                    }
                };

                var configService = A.Fake<IConfigService>();
                A.CallTo(() => configService.GetConfiguration()).Returns(configuration);
                MappedDependencies = new List<Tuple<Type, object>>
                {
                    new Tuple<Type, object>(typeof(IAuthenticationCallbackProvider), authenticationCallbackProvider),
                    new Tuple<Type, object>(typeof(IConfigService), configService)
                };

                var browser = Browser();

                // Act.
                var result = Assert.Throws<Exception>(() =>
                {
                    browser.Get("/authenticate/callback", with =>
                    {
                        with.HttpRequest();
                        with.Query("providerkey", "google");
                        with.HostName("foo.com");
                    });
                });

                // Assert.
                result.ShouldNotBe(null);
                result.InnerException.ShouldNotBe(null);
                result.InnerException.InnerException.ShouldNotBe(null);
                result.InnerException.InnerException.Message.ShouldBe(
                    "No State value was found which generally means that a Cross Site Request Forgery attempt might be made. A 'State' value is generated by the server when a client prepares to rediect to an Authentication Provider and passes that generated state value to that Provider. The provider then passes that state value back, which proves that the client (ie. that's -you-) have actually authenticated against a provider. Otherwise, anyone can just hit the callback Url and impersonate another user, bypassing the authentication stage. So what's the solution: make sure you call the 'RedirectToProvider' endpoint *before* you hit the 'AuthenticateCallbackAsync' callback endpoint.");
            }
        }
    }
}