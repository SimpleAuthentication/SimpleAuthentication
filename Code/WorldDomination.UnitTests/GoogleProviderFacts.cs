using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Google;
using Xunit;

namespace WorldDomination.UnitTests
{
    // ReSharper disable InconsistentNaming

    public class GoogleProviderFacts
    {
        public class RedirectToAuthenticateFacts
        {
            [Fact]
            public void GivenSomeState_RedirectToAuthenticate_ReturnsAUri()
            {
                // Arrange.
                var googleProvider = new GoogleProvider("aa", "bb", new Uri("http://wwww.google.com"));

                // Act.
                var result = googleProvider.RedirectToAuthenticate("bleh");

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("https://accounts.google.com/o/oauth2/auth?client_id=aa&redirect_uri=http://wwww.google.com/&response_type=code&state=bleh&scope=https://www.googleapis.com/auth/userinfo.profile%20https://www.googleapis.com/auth/userinfo.email", result.AbsoluteUri);
            }
        }

        public class AuthenticateClientFacts
        {
            [Fact]
            public void GivenADifferentStateValue_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var googleProvider = new GoogleProvider("aa", "bb", new Uri("http://wwww.google.com"));

                // Act.
                //var result = Assert.Throws<AuthenticationException>(() => googleProvider.AuthenticateClient())

                // Assert.
            }
        }
    }

    // ReSharper restore InconsistentNaming
}
