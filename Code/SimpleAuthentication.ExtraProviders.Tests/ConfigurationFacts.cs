using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using WorldDomination.Web.Authentication.Config;
using Xunit;

namespace WorldDomination.Web.Authentication.Tests
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
            var facebookProvider = authenticationProviders.Providers["facebook"];

            // Assert.
            Assert.Equal("testKey", facebookProvider.Key);
            Assert.Equal("testSecret", facebookProvider.Secret);
        }

        [Fact]
        public void GivenAInvalidConfigurationSection_UseConfig_ThrowsAConfigurationErrorsException()
        {
            // Arrange.
            const string fileName = "InvalidApp.config";

            // Act & Assert.
            Assert.Throws<ConfigurationErrorsException>(
                () => { ProviderConfigHelper.UseConfig(fileName, "authenticationProviders"); });
        }

        [Fact]
        public void GivenAValidConfigSectionWithAMisingProvider_ProvidersIndexer_ReturnsNull()
        {
            // Arrange.
            var authenticationProviders = ProviderConfigHelper.UseConfig();

            // Act.
            var missingProvider = authenticationProviders.Providers["foo"];

            // Assert.
            Assert.Null(missingProvider);
        }

        [Fact]
        public void GivenAValidConfigSectionWithAMisingProvider_UsingConfigFor_ThrowsAKeyNotFoundException()
        {
            // Arrange.

            // Act & Assert.
            Assert.Throws<KeyNotFoundException>(() => { ProviderConfigHelper.UseConfig().For("twitter"); });
        }

        [Fact]
        public void GivenAMissingConfigFile_UseConfig_ReturnsNull()
        {
            // Arrange.

            // Act & Assert.
            var result = Assert.Throws<ApplicationException>( () => ProviderConfigHelper.UseConfig("app.config", "blarg"));

            // Assert.
            Assert.NotNull(result);
            Assert.Equal("Missing the config section [blarg] from your .config file", result.Message);
        }

        [Fact]
        public void GivenAValidProviderConfigurationWithSomeKeys_NewAuthenticationService_HasSomeProvidersAdded()
        {
            // Arrange.
            var providerConfiguration = ProviderConfigHelper.UseConfig();
            var providerCount = providerConfiguration.Providers.Count; // 2 hard-coded providers + 3 fakes autoloaded.

            // Act.
            var authenticationService = new AuthenticationService();

            // Assert.
            Assert.NotNull(authenticationService);
            var firstProvider = authenticationService.AuthenticationProviders.SingleOrDefault(x => x.Key == "facebook");
            Assert.NotNull(firstProvider);
            Assert.Equal("Facebook", firstProvider.Value.Name);
        }
    }

    // ReSharper restore InconsistentNaming
}