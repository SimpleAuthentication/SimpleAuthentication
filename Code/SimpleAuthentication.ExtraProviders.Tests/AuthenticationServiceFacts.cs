using System;
using System.Collections.Specialized;
using WorldDomination.Web.Authentication.Exceptions;
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
                var result = Assert.Throws<InvalidOperationException>(
                    () => authenticationService.AddProvider(facebookProvider, false));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("The provider 'Facebook' already exists and cannot be overridden, either set `replaceExisting` to `true`, or remove the provider first.", result.Message);
            }
        }

        public class GetAuthenticatedClientFacts
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

            [Fact]
            public void GivenAFakeProviderKey_GetAuthenticatedClient_ReturnsADefaultAuthenticatedClient()
            {
                // Arrange.
                const string name = "FakeProvider";
                const string state = "this-is-some-state";
                var authenticationServiceSettings = new FakeAuthencationServiceSettings(name);
                var authenticationService = new AuthenticationService();
                var querystringParameters = new NameValueCollection {{"state", state}};

                // Act.
                var authenticatedClient = authenticationService.GetAuthenticatedClient(authenticationServiceSettings, querystringParameters);
            
                // Assert.
                Assert.NotNull(authenticatedClient);
                Assert.Equal(name, authenticatedClient.ProviderName);
                Assert.NotNull(authenticatedClient.AccessToken);
                Assert.NotNull(authenticatedClient.UserInformation);
                Assert.Equal("FakeId-ABCDEFGHIJKLMNOPQRSTUVWXYZ", authenticatedClient.UserInformation.Id);
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
                Assert.Equal("No 'aaa' provider details have been added/provided. Maybe you forgot to add the name/key/value data into your web.config? Eg. in your web.config configuration/authenticationProviders/providers section add the following (if you want to offer Google authentication): <add name=\"Google\" key=\"someNumber.apps.googleusercontent.com\" secret=\"someSecret\" />", result.Message);
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

            [Fact]
            public void GivenAValidFakeProviderKeyWhichDoesntExist_RedirectToAuthenticate_ReturnsAUri()
            {
                // Arrange.
                var authenticationService = new AuthenticationService();

                // Act.
                var result = authenticationService.RedirectToAuthenticationProvider("FakeBlahBlah",
                                                                                    new Uri(
                                                                                        "http://www.pleasecomehome.com?provider=foo"));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("http://www.pleasecomehome.com/?provider=foo&state=This%20is%20some%20fake%20state", result.AbsoluteUri);
            }

            [Fact]
            public void GivenAFakeProviderKeyWhichDoesExist_RedirectToAuthenticate_ReturnsAUri()
            {
                // Arrange.
                const string name = "FakeProviderWoot";
                var fakeProvider = new FakeProvider(name)
                {
                    AccessToken = new AccessToken
                    {
                        ExpiresOn = DateTime.Now.AddMinutes(100),
                        PublicToken = "I.Am.A.Public.Token"
                    },
                    UserInformation = new UserInformation
                    {
                        Email = "new.email@somewhere.com",
                        Gender = GenderType.Male,
                        Name = "Fake Name",
                        Id = "1234567890",
                        UserName = "Fake.UserName"
                    }
                };

                var authenticationService = new AuthenticationService();
                authenticationService.AddProvider(fakeProvider);

                // Act.
                var result = authenticationService.RedirectToAuthenticationProvider(name,
                                                                       new Uri(
                                                                           "http://i.want.to.go.home.com/click/feet/together?provider=google"));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("http://i.want.to.go.home.com/click/feet/together?provider=google&state=This%20is%20some%20fake%20state", result.AbsoluteUri);
            }
        }
    }

    // ReSharper restore InconsistentNaming
}