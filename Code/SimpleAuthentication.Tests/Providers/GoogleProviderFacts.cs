using Shouldly;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SimpleAuthentication.Tests.Providers
{
    public class GoogleProviderFacts
    {
        public class GetRedirectToAuthenticateSettingsFacts
        {
            [Fact]
            public void GivenSomeStuff_GetRedirectToAuthenticateSettings_ReturnsSomeRedirectToAuthenticateSettings()
            {
                // Arrange.
                var providerParams = new ProviderParams
                {
                    PublicApiKey = "some public api key",
                    SecretApiKey = "some secret api key"
                };
                var provider = new GoogleProvider(providerParams);
                var callBackUrl = new Uri("Http://www.foo.com/callback");
                
                // Act.
                var settings = provider.GetRedirectToAuthenticateSettings(callBackUrl);

                // Assert.
                settings.ShouldNotBe(null);
                settings.State.ShouldNotBeNullOrEmpty();
                Guid.Parse(settings.State);
                settings.RedirectUri.ShouldNotBe(null);
                settings.RedirectUri.AbsoluteUri.ShouldStartWith("https://accounts.google.com/o/oauth2/auth?client_id=some%20public%20api%20key&redirect_uri=http://www.foo.com/callback&response_type=code&scope=https://www.googleapis.com/auth/userinfo.profile%20https://www.googleapis.com/auth/userinfo.email&state=");
            }
        }
    }
}
