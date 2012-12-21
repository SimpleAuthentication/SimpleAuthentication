using System;
using System.Collections.Generic;
using System.Configuration;
using WorldDomination.Web.Authentication.Config;
using Xunit;

namespace WorldDomination.Web.Authentication.Test.UnitTest
{
    // ReSharper disable InconsistentNaming

    public class ConfigurationFacts
    {
        [Fact]
        public void GivenTheKeyFacebook_ShouldFindConfigurationSection_AndReturnCorrectKeyAndSecret()
        {
            var providers = (ProviderConfiguration) ConfigurationManager.GetSection("authProviders");

            var facebookProvider = providers.Providers[ProviderType.Facebook];

            Assert.Equal("testKey", facebookProvider.Key);
        }

        [Fact]
        public void GivenTheKeyTwitter_ShouldNotFindConfigSection_ShouldBeNull()
        {
            var providers = (ProviderConfiguration)ConfigurationManager.GetSection("authProviders");

            var facebookProvider = providers.Providers[ProviderType.Twitter];

            Assert.Null(facebookProvider);
        }

        [Fact]
        public void GivenTheKeyTwitter_UsingTheConfigHelper_ShouldThrow()
        {
            Assert.Throws<KeyNotFoundException>(() =>
            {
                ProviderConfigHelper.UseConfig().For(ProviderType.Twitter);
            });
        }

        [Fact]
        public void GivenAnyKey_WhenFileDoesntExist_ShouldThrow()
        {
            Assert.Throws<ApplicationException>(() =>
            {
                ProviderConfigHelper.UseConfig("TestFile.config").For(ProviderType.Twitter);
            });
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
            Assert.Equal(providerCount, ((IList<IAuthenticationProvider>)authenticationService.Providers).Count);
        }
    } 

    // ReSharper restore InconsistentNaming
}
