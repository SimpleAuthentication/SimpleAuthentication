using FakeItEasy;
using Shouldly;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Config;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers;
using Xunit;

namespace SimpleAuthentication.Tests
{
    public class AuthenticationProviderFactoryFacts
    {

        public class AuthenticationProviderConstructorFacts
        {
            [Fact]
            public void GivenAConfigServiceWithNoProviders_Constructor_ThrowsAnException()
            {
                // Arrange.
                var configService = A.Fake<IConfigService>();
                // NOTE: no Providers are set, here. This emulates that no
                //       providers were found in the appSettings.
                A.CallTo(() => configService.GetConfiguration()).Returns(new Configuration());

                var providerScanner = A.Fake<IProviderScanner>();

                // Act.
                var exception = Should.Throw<AuthenticationException>(() =>
                    new AuthenticationProviderFactory(configService, providerScanner));

                // Assert.
                exception.Message.ShouldBe("There needs to be at least one Authentication Provider's detail's in the configService.Provider's collection. Otherwise, how else are we to set the available Authentication Providers?");
            }

            [Fact]
            public void GivenAProviderScannerWithNoDiscoveredProviders_Constructor_ThrowsAnException()
            {
                // Arrange.
                var configService = A.Fake<IConfigService>();
                A.CallTo(() => configService.GetConfiguration()).Returns(TestHelpers.ConfigurationWithGoogleProvider);

                // NOTE: This provider scanner defaults to returning a null Type list.
                var providerScanner = A.Fake<IProviderScanner>();

                // Act.
                var exception = Should.Throw<AuthenticationException>(() =>
                    new AuthenticationProviderFactory(configService, providerScanner));

                // Assert.
                exception.Message.ShouldBe("No discovered providers were found by the Provider Scanner. We need at least one IAuthenticationProvider type to exist so we can attempt to map the authentication data (from the configService) to the found Provider.");
            }

            [Fact]
            public void GivenAConfigProviderThanDoesntExistInADiscoveredProviders_Constructor_ThrowsAnException()
            {
                // Arrange.
                var configService = A.Fake<IConfigService>();
                A.CallTo(() => configService.GetConfiguration()).Returns(TestHelpers.ConfigurationWithGoogleProvider);

                // NOTE: This provider scanner defaults to returning a null Type list.
                var discoveredProviders = new[] {typeof (FacebookProvider)};
                var providerScanner = A.Fake<IProviderScanner>();
                A.CallTo(() => providerScanner.GetDiscoveredProviders()).Returns(discoveredProviders);

                // Act.
                var exception = Should.Throw<AuthenticationException>(() =>
                    new AuthenticationProviderFactory(configService, providerScanner));

                // Assert.
                exception.Message.ShouldBe("Unable to find the provider [google]. Is there a provider dll available? Is there a typo in the provider name? Solution suggestions: Check to make sure the correct dll's are in the 'bin' directory and/or check the name to make sure there's no typo's in there. Example: If you're trying include the GitHub provider, make sure the name is 'github' (any case) and that the ExtraProviders dll exists in the 'bin' directory or make sure you've downloaded the package via NuGet -> install-package SimpleAuthentication.ExtraProviders.");
            }
        }
    }
}