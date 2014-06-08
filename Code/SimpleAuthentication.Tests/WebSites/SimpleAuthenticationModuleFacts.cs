using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsQuery;
using FakeItEasy;
using Nancy;
using Nancy.SimpleAuthentication;
using Nancy.Testing;
using Shouldly;
using SimpleAuthentication.Core.Config;
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

                MappedDependencies = new List<Tuple<Type, object>>
                {
                    new Tuple<Type, object>(typeof(IAuthenticationCallbackProvider), authenticationCallbackProvider),
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

                MappedDependencies = new List<Tuple<Type, object>>
                {
                    new Tuple<Type, object>(typeof(IAuthenticationCallbackProvider), authenticationCallbackProvider),
                };

                var browser = Browser();

                // Act.
                var result = browser.Get("/authenticate/google", with =>
                {
                    with.HttpRequest();
                    with.Query("identifier", "http://www.foo.com/authenticate");
                });

                // Assert.
                result.ShouldNotBe(null);
                result.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            }

            [Fact]
            public void GivenAValidProviderKeyAndAReferrerHeader_GetRedirectToProvider_ReturnsARedirectResponse()
            {
                throw new NotImplementedException();
            }
        }
    }
}
