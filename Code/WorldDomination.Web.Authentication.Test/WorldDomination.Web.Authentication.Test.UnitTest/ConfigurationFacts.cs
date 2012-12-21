using System;
using System.Collections.Generic;
using System.Linq;
using WorldDomination.Web.Authentication.Config;
using Xunit;

namespace WorldDomination.Web.Authentication.Test.UnitTest
{
    // ReSharper disable InconsistentNaming

    public class ConfigurationFacts
    {
        [Fact]
        public void GivenAValidConfigSectionWithAFacebookKey_UseConfig_ReturnsAFacebookProviderWithACorrectKeyAndSecret()
        {
            // Arrange.

            // Act.
            var authenticationProviders = ProviderConfigHelper.UseConfig();
            var facebookProvider = authenticationProviders.Providers[ProviderType.Facebook];

            // Assert.
            Assert.Equal("testKey", facebookProvider.Key);
            Assert.Equal("testSecret", facebookProvider.Secret);
        }

        [Fact]
        public void GivenAValidConfigSectionWithAMisingProvider_ProvidersIndexer_ReturnsNull()
        {
            // Arrange.
            var authenticationProviders = ProviderConfigHelper.UseConfig();

            // Act.
            var facebookProvider = authenticationProviders.Providers[ProviderType.Twitter];

            // Assert.
            Assert.Null(facebookProvider);
        }

        [Fact]
        public void GivenAValidConfigSectionWithAMisingProvider_UsingConfigFor_ThrowsAKeyNotFoundException()
        {
            // Arrange.

            // Act & Assert.
            Assert.Throws<KeyNotFoundException>(() => { ProviderConfigHelper.UseConfig().For(ProviderType.Twitter); });
        }

        [Fact]
        public void GivenAMissingConfigFile_UseConfig_ThrowsAnApplicationException()
        {
            // Arrange.
            const string fileName = "TestFile.config";

            // Act & Assert.
            Assert.Throws<ApplicationException>(() => { ProviderConfigHelper.UseConfig(fileName).For(ProviderType.Twitter); });
        }

        [Fact]
        public void GivenAValidProviderConfigurationWithSomeKeys_NewAuthenticationService_HasSomeProvidersAdded()
        {
            // Arrange.
            var providerConfiguration = ProviderConfigHelper.UseConfig();
            var providerCount = providerConfiguration.Providers.Count;
            var redirectUri = new Uri("http://www.whatever.com/callback");

            // Act.
            var authenticationService = new AuthenticationService(providerConfiguration, redirectUri);

            // Assert.
            Assert.NotNull(authenticationService);
            Assert.Equal(providerCount, authenticationService.Providers.Count());
        }
    }

    // ReSharper restore InconsistentNaming
}