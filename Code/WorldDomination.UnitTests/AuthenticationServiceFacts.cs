using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;
using Moq;
using RestSharp;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Facebook;
using Xunit;

namespace WorldDomination.UnitTests
{
    // ReSharper disable InconsistentNaming

    public class AuthenticationServiceFacts
    {
        public class AddProviderFacts
        {
            [Fact]
            public void GivenANewProvider_AddProvider_AddsTheProviderToTheProviderCollection()
            {
                // Arrange.
                var authenticationService = new AuthenticationService();

                // Act.
                authenticationService.AddProvider(new FacebookProvider("a", "b", new Uri("http://www.google.com")));

                // Assert.
                var providers = authenticationService.AuthenticationProviders;
                Assert.NotNull(providers);
                Assert.Equal(1, providers.Count);
                Assert.NotNull(providers["Facebook"]);
            }

            [Fact]
            public void GivenAnExistingProvider_AddProvider_ThrowsAnException()
            {
                // Arrange.
                var authenticationService = new AuthenticationService();
                var facebookProvider = new FacebookProvider("a", "b", new Uri("http://www.google.com"));
                // Act.
                authenticationService.AddProvider(facebookProvider);
                var result = Assert.Throws<AuthenticationException>( 
                    () => authenticationService.AddProvider(facebookProvider));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Trying to add a Facebook provider, but one already exists.", result.Message);
            }
        }

        public class RedirectToAuthenticationProviderFacts
        {
            [Fact]
            public void GivenAnInvalidProviderKey_RedirectToAuthenticationProvider_ThrowsAnException()
            {
                // Arrange.
                const string providerKey = "aaa";
                const string state = "asd";
                var authenticationService = new AuthenticationService();

                // Act and Assert.
                var result = Assert.Throws<AuthenticationException>(
                    () => authenticationService.RedirectToAuthenticationProvider(providerKey, state));

                Assert.NotNull(result);
                Assert.Equal("No 'aaa' provider has been added.", result.Message);
            }

            [Fact]
            public void GivenAValidProviderKey_RedirectToAuthenticate_ReturnsAUri()
            {
                // Arrange.
                var authenticationService = new AuthenticationService();
                authenticationService.AddProvider(new FacebookProvider("aa", "bb", new Uri("http://www.google.com")));

                // Act.
                var result = authenticationService.RedirectToAuthenticationProvider("Facebook", "abc");

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(result.AbsoluteUri, "https://www.facebook.com/dialog/oauth?client_id=aa&redirect_uri=http://www.google.com/&state=abc");
            }
        }

        public class CheckCallbackFacts
        {
            [Fact]
            public void GiveAMissingProviderKeyQuerystringValue_CheckCallback_ThrowsAnException()
            {
                // Arrange.
                var authenticationService = new AuthenticationService();

                // Act.
                var result = Assert.Throws<ArgumentNullException>(() => authenticationService.CheckCallback(null, null, null));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("value should not be null or an empty string.\r\nParameter name: value", result.Message);
            }

            [Fact]
            public void GivenAnInvalidProviderKey_CheckCallback_ThrowsAnException()
            {
                // Arrange.
                const string providerKey = "aaa";
                const string state = "asd";
                var querystringParams = new NameValueCollection();
                var authenticationService = new AuthenticationService();

                // Act and Assert.
                var result = Assert.Throws<AuthenticationException>(
                    () => authenticationService.CheckCallback(providerKey, querystringParams, state));

                Assert.NotNull(result);
                Assert.Equal("No 'aaa' provider has been added.", result.Message);
            }
        }
    }

    // ReSharper restore InconsistentNaming
}