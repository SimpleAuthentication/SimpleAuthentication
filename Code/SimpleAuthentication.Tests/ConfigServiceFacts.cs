using Shouldly;
using SimpleAuthentication.Core.Config;
using SimpleAuthentication.Core.Exceptions;
using Xunit;

namespace SimpleAuthentication.Tests
{
    public class ConfigServiceFacts
    {
        [Fact]
        public void GivenNoAppSettings_RedirectToProvider_ThrowsAnException()
        {
            // Arrange.
            var configService = new AppConfigService();

            // Act.
            var exception = Should.Throw<AuthenticationException>(() => configService.GetConfiguration());

            // Assert.
            exception.Message.ShouldBe("AppSettings section parsed and -no- provider's were found. At least one key/value is required in the <appSettings> section so we can authenticate against a provider. A sample key/value is: <add key=\"sa.Google\" value=\"key:blahblahblah.apps.googleusercontent.com;secret:pew-pew\" />");
        }
    }
}
