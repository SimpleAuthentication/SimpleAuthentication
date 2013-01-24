using System;
using System.Collections.Generic;
using System.Configuration;
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
            Assert.Throws<ApplicationException>(
                () => { ProviderConfigHelper.UseConfig(fileName).For(ProviderType.Twitter); });
        }

        [Fact]
        public void GivenAValidProviderConfigurationWithSomeKeys_NewAuthenticationService_HasSomeProvidersAdded()
        {
            // Arrange.
            var providerConfiguration = ProviderConfigHelper.UseConfig();
            var providerCount = providerConfiguration.Providers.Count;

            // Act.
            var authenticationService = new AuthenticationService(providerConfiguration);

            // Assert.
            Assert.NotNull(authenticationService);
            Assert.Equal(providerCount, authenticationService.Providers.Count());
            var firstProvider = authenticationService.Providers.First();
            Assert.NotNull(firstProvider);
            Assert.Equal("Facebook", firstProvider.Name);
        }
    }

    // ReSharper restore InconsistentNaming
}