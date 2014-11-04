using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FakeItEasy;
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
