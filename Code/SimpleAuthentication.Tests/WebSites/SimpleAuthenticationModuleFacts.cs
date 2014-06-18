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
                result.Headers["Location"].ShouldStartWith("https://accounts.google.com/o/oauth2/auth?client_id=some%20key&redirect_uri=http://foo.com/authenticate/callback?providerkey=google&response_type=code&scope=https://www.googleapis.com/auth/userinfo.profile%20https://www.googleapis.com/auth/userinfo.email&state=");
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
                result.Headers["Location"].ShouldStartWith("https://accounts.google.com/o/oauth2/auth?client_id=some%20key&redirect_uri=http://www.pewpew.com/authenticate/callback?providerkey=google&response_type=code&scope=https://www.googleapis.com/auth/userinfo.profile%20https://www.googleapis.com/auth/userinfo.email&state=");
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
                result.Headers["Location"].ShouldStartWith("https://accounts.google.com/o/oauth2/auth?client_id=some%20key&redirect_uri=http://www.pewpew.com/authenticate/callback?providerkey=google&response_type=code&scope=https://www.googleapis.com/auth/userinfo.profile%20https://www.googleapis.com/auth/userinfo.email&state=");
            
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
                    .Process(A<NancyModule>._, A<AuthenticateCallbackData>._))
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

                const string accessTokenJson = "{\"access_token\" : \"ya29.AHES6ZRkbCe14R8ZgnsKEgWxcntWLxuYZ7Uy6Q8jWEVgiCHPKu0CYpVY\",\"token_type\" : \"Bearer\",\"expires_in\" : 3586, \"id_token\" : \"eyJhbGciOiJSUzI1NiIsImtpZCI6IjY5Y2EyM2FmYjhlZWIyMTM0NTMwYjExNDc1MzFmMzc0MTg3YTBiOWYifQ.eyJpc3MiOiJhY2NvdW50cy5nb29nbGUuY29tIiwidG9rZW5faGFzaCI6ImRrZkVpT0NWSDgwVjRaZXVRc0tma2ciLCJhdF9oYXNoIjoiZGtmRWlPQ1ZIODBWNFpldVFzS2ZrZyIsImF1ZCI6IjU4NzE0MDA5OTE5NC5hcHBzLmdvb2dsZXVzZXJjb250ZW50LmNvbSIsImNpZCI6IjU4NzE0MDA5OTE5NC5hcHBzLmdvb2dsZXVzZXJjb250ZW50LmNvbSIsImF6cCI6IjU4NzE0MDA5OTE5NC5hcHBzLmdvb2dsZXVzZXJjb250ZW50LmNvbSIsImlkIjoiMTE2NzEyMzIwMDUxMzQwNjY5MjMzIiwic3ViIjoiMTE2NzEyMzIwMDUxMzQwNjY5MjMzIiwidmVyaWZpZWRfZW1haWwiOiJ0cnVlIiwiZW1haWxfdmVyaWZpZWQiOiJ0cnVlIiwiaGQiOiJhZGxlci5jb20uYXUiLCJlbWFpbCI6Imp1c3RpbkBhZGxlci5jb20uYXUiLCJpYXQiOjEzNzI5NDMzNTAsImV4cCI6MTM3Mjk0NzI1MH0.rOdHq1FBi4Sfi1WfOThmDLruC38fD8u5Vq8AtDoVZSF3j8z05zv6JueWJyF2by4NGFS-T8FroLJJCz2U_WS420crmZcDuEEZkbjzmrZYaerVwfkAtGvjQUykxI5Imv0Bgnwl9v_CtM5uoejH9e7bzEnfs17Gz3pKnPETaGIf4tc\" }";
                var googleUserInfoResult = new Core.Providers.Google.UserInfoResult
                {
                    Email = "purekrome@pewpew.com",
                    FamilyName = "Krome",
                    Gender = "male",
                    GivenName = "Pure",
                    Id = "1234",
                    Link = "http://a.b.c.d/",
                    Locale = "en-au",
                    Name = "Pure Krome",
                    Picture = "http://www.imgur.com/pic.png",
                    VerifiedEmail = true
                };
                var userInformationJson = File.ReadAllText("Sample Data\\GoogleUserInfoResult.json");

                var accessTokenResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(accessTokenJson);
                var userInformationResponse = FakeHttpMessageHandler.GetStringHttpResponseMessage(userInformationJson);
                HttpClientFactory.MessageHandler = new FakeHttpMessageHandler(
                    new Dictionary<string, HttpResponseMessage>
                    {
                        {"http://foo.com/authenticate/callback?providerkey=google", accessTokenResponse},
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
                result.InnerException.InnerException.Message.ShouldBe("No State value was found which generally means that a Cross Site Request Forgery attempt might be made. A 'State' value is generated by the server when a client prepares to rediect to an Authentication Provider and passes that generated state value to that Provider. The provider then passes that state value back, which proves that the client (ie. that's -you-) have actually authenticated against a provider. Otherwise, anyone can just hit the callback Url and impersinate another user, bypassing the authentication stage. So what's the solution: make sure you call the redirect endpoint before you hit the callback endpoint.");
            }
        }
    }
}