using System;
using System.Collections.Generic;
using System.Configuration;
using WorldDomination.Web.Authentication.Config;
using Xunit;

namespace WorldDomination.Web.Authentication.Test.UnitTest
{
    public class ConfigurationFacts
    {
        [Fact]
        public void GivenTheKeyFacebook_ShouldFindConfigurationSection_AndReturnCorrectKeyAndSecret()
        {
            var providers = (ProviderConfiguration) ConfigurationManager.GetSection("authProviders");

            var facebookProvider = providers.Providers[ProviderEnum.Facebook];

            Assert.Equal("testKey", facebookProvider.Key);
        }

        [Fact]
        public void GivenTheKeyTwitter_ShouldNotFindConfigSection_ShouldBeNull()
        {
            var providers = (ProviderConfiguration)ConfigurationManager.GetSection("authProviders");

            var facebookProvider = providers.Providers[ProviderEnum.Twitter];

            Assert.Null(facebookProvider);
        }

        [Fact]
        public void GivenTheKeyTwitter_UsingTheConfigHelper_ShouldThrow()
        {
            Assert.Throws<KeyNotFoundException>(() =>
            {
                ProviderConfigHelper.UseConfig().For(ProviderEnum.Twitter);
            });
        }

        [Fact]
        public void GivenAnyKey_WhenFileDoesntExist_ShouldThrow()
        {
            Assert.Throws<ApplicationException>(() =>
            {
                ProviderConfigHelper.UseConfig("TestFile.config").For(ProviderEnum.Twitter);
            });
        }
    }
}
