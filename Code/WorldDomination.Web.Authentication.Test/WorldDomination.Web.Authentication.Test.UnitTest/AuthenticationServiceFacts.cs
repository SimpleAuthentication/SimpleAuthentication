﻿using System;
using System.Collections.Specialized;
using WorldDomination.Web.Authentication.Facebook;
using Xunit;

namespace WorldDomination.Web.Authentication.Test.UnitTest
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
                authenticationService.AddProvider(new FacebookProvider("a", "b"));

                // Assert.
                var providers = authenticationService.AuthenticationProviders;
                Assert.NotNull(providers);
                Assert.Equal(1, providers.Count);
                Assert.NotNull(providers["facebook"]);
            }

            [Fact]
            public void GivenAnExistingProvider_AddProvider_ThrowsAnException()
            {
                // Arrange.
                var authenticationService = new AuthenticationService();
                var facebookProvider = new FacebookProvider("a", "b");
                // Act.
                authenticationService.AddProvider(facebookProvider);
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
                        () => authenticationService.GetAuthenticatedClient(null, null, null));

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
                    () => authenticationService.GetAuthenticatedClient(providerKey, querystringParams, state));

                Assert.NotNull(result);
                Assert.Equal("No 'aaa' provider has been added.", result.Message);
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
                authenticationService.AddProvider(new FacebookProvider("aa", "bb"));

                // Act.
                var result = authenticationService.RedirectToAuthenticationProvider("Facebook",
                                                                                    new Uri("http://www.whatever.com"));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "https://www.facebook.com/dialog/oauth?client_id=aa&scope=email&redirect_uri=http://www.whatever.com/",
                    result.AbsoluteUri);
            }

            [Fact]
            public void GivenAValidProviderKeyWithState_RedirectToAuthenticate_ReturnsAUriWithState()
            {
                // Arrange.
                var authenticationService = new AuthenticationService();
                authenticationService.AddProvider(new FacebookProvider("aa", "bb"));

                // Act.
                var authenticationServiceSettings = authenticationService.GetAuthenticateServiceSettings("facebook");
                authenticationServiceSettings.State = "pewpew";
                authenticationServiceSettings.CallBackUri = new Uri("http://www.whatever.com");
                var result = authenticationService.RedirectToAuthenticationProvider(authenticationServiceSettings);

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "https://www.facebook.com/dialog/oauth?client_id=aa&state=pewpew&scope=email&redirect_uri=http://www.whatever.com/",
                    result.AbsoluteUri);
            }
        }
    }

    // ReSharper restore InconsistentNaming
}