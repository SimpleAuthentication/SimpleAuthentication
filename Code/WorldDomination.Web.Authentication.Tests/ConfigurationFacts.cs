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
            Assert.Equal(new Uri("http://www.mywebsite.com/authenticationCallback").AbsoluteUri,
                         authenticationProviders.CallbackUri);
            Assert.Equal("providerKey", authenticationProviders.CallbackQuerystringKey);
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
        public void GivenAMissingConfigFile_UseConfig_ThrowsAnApplicationException()
        {
            // Arrange.
            const string fileName = "TestFile.config";

            // Act & Assert.
            Assert.Throws<ApplicationException>(
                () => { ProviderConfigHelper.UseConfig(fileName).For("twitter"); });
        }

        [Fact]
        public void GivenAValidProviderConfigurationWithSomeKeys_NewAuthenticationService_HasSomeProvidersAdded()
        {
            // Arrange.
            var providerConfiguration = ProviderConfigHelper.UseConfig();
            var providerCount = providerConfiguration.Providers.Count;

            // Act.
            var authenticationService = new AuthenticationService();

            // Assert.
            Assert.NotNull(authenticationService);
            Assert.Equal(providerCount, authenticationService.AuthenticationProviders.Count());
            var firstProvider = authenticationService.AuthenticationProviders.First();
            Assert.NotNull(firstProvider);
            Assert.Equal("Facebook", firstProvider.Value.Name);
        }
    }

    // ReSharper restore InconsistentNaming
}