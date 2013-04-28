﻿using System;
using System.Collections.Specialized;
using WorldDomination.Web.Authentication.Providers;
using Xunit;

namespace WorldDomination.Web.Authentication.Tests
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
                authenticationService.AddProvider(new TwitterProvider(new ProviderParams { Key = "a", Secret = "b"}));

                // Assert.
                var providers = authenticationService.AuthenticationProviders;
                Assert.NotNull(providers);
                Assert.Equal(3, providers.Count);
                Assert.NotNull(providers["twitter"]);
            }

            [Fact]
            public void GivenAnExistingProvider_AddProvider_ThrowsAnException()
            {
                // Arrange.
                var authenticationService = new AuthenticationService();
                var facebookProvider = new FacebookProvider(new ProviderParams { Key = "a", Secret = "b" });

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => authenticationService.AddProvider(facebookProvider));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Trying to add a facebook provider, but one already exists.", result.Message);
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
                var result =
                    Assert.Throws<ArgumentNullException>(
                        () => authenticationService.GetAuthenticatedClient(null, null));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Value cannot be null.\r\nParameter name: authenticationServiceSettings", result.Message);
            }

            [Fact]
            public void GivenANullIAuthenticationServiceSettings_CheckCallback_ThrowsAnException()
            {
                // Arrange.
                var querystringParams = new NameValueCollection {{"a", "b"}};
                var authenticationService = new AuthenticationService();

                // Act and Assert.
                var result = Assert.Throws<ArgumentNullException>(
                    () => authenticationService.GetAuthenticatedClient(null, querystringParams));

                Assert.NotNull(result);
                Assert.Equal("Value cannot be null.\r\nParameter name: authenticationServiceSettings", result.Message);
            }
        }

        public class RedirectToAuthenticationProviderFacts
        {
            [Fact]
            public void GivenAnInvalidProviderKey_RedirectToAuthenticationProvider_ThrowsAnException()
            {
                // Arrange.
                const string providerKey = "aaa";
                var authenticationService = new AuthenticationService();

                // Act and Assert.
                var result = Assert.Throws<AuthenticationException>(
                    () => authenticationService.RedirectToAuthenticationProvider(providerKey));

                Assert.NotNull(result);
                Assert.Equal("No 'aaa' provider has been added.", result.Message);
            }

            [Fact]
            public void GivenAValidProviderKeyWithNoState_RedirectToAuthenticate_ReturnsAUri()
            {
                // Arrange.
                var authenticationService = new AuthenticationService();

                // Act.
                var result = authenticationService.RedirectToAuthenticationProvider("Facebook",
                                                                                    new Uri("http://www.whatever.com"));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "https://www.facebook.com/dialog/oauth?client_id=testKey&scope=email&redirect_uri=http://www.whatever.com/",
                    result.AbsoluteUri);
            }

            [Fact]
            public void GivenAValidProviderKeyWithState_RedirectToAuthenticate_ReturnsAUriWithState()
            {
                // Arrange.
                var authenticationService = new AuthenticationService();

                // Act.
                var authenticationServiceSettings = authenticationService.GetAuthenticateServiceSettings("facebook", new Uri("http://www.whatever.com"));
                authenticationServiceSettings.State = "pewpew"; // Override, otherwise it's always random.
                var result = authenticationService.RedirectToAuthenticationProvider(authenticationServiceSettings);

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "https://www.facebook.com/dialog/oauth?client_id=testKey&state=pewpew&scope=email&redirect_uri=http://www.whatever.com/authentication/authenticatecallback?providerkey=facebook",
                    result.AbsoluteUri);
            }
        }
    }

    // ReSharper restore InconsistentNaming
}