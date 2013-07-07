using System;
using System.Text;
using WorldDomination.Web.Authentication.Csrf;
using Xunit;

namespace WorldDomination.Web.Authentication.Tests
{
    // ReSharper disable InconsistentNaming

    public class CsrfFacts
    {
        public class CreateTokenFacts
        {
            [Fact]
            public void GivenNoExtraData_CreateToken_ReturnsAGuidForBoth()
            {
                // Arrange.
                var antiForgery = new AntiForgery();

                // Act.
                var result = antiForgery.CreateToken(existingToKeepToken: "don't care!");

                // Assert.
                Assert.NotNull(result);
                Guid toKeep;
                Guid toSend;
                Assert.True(Guid.TryParse(result.ToKeep, out toKeep));
                Assert.True(Guid.TryParse(result.ToSend, out toSend));
                Assert.Equal(toKeep, toSend);
            }

            [Fact]
            public void GivenSomeExtraData_CreateToken_ReturnsAFunkyStringInToKeep()
            {
                // Arrage.
                const string extraData = "http://2p1s.com";
                var antiForgery = new AntiForgery();

                // Act.
                var result = antiForgery.CreateToken("dont't care!", extraData);

                // Assert.
                Assert.NotNull(result);
                Assert.True(result.ToKeep.Contains("|"));
                Assert.Equal("aHR0cDovLzJwMXMuY29t",
                             result.ToKeep.Substring(result.ToKeep.IndexOf("|", StringComparison.Ordinal) + 1));
            }

            [Fact]
            public void GivenSomeExtraData_CreateToken_ReturnsTheGuidOnlyInToSend()
            {
                // Arrage.
                const string extraData = "http://2p1s.com";
                var antiForgery = new AntiForgery();

                // Act.
                var result = antiForgery.CreateToken(extraData);

                // Assert.
                Assert.NotNull(result);
                Guid guid;
                Assert.True(Guid.TryParse(result.ToSend, out guid));
            }
        }

        public class ValidateTokenFacts
        {
            [Fact]
            public void GivenSomeTokenWithNoExtraData_ReturnsNullIfTokenValid()
            {
                // Arrange.
                var antiForgery = new AntiForgery();
                var guid = Guid.NewGuid();
                var token = guid.ToString();

                // Act.
                var result = antiForgery.ValidateToken(token, token);

                // Assert.
                Assert.Null(result);
            }

            [Fact]
            public void GivenSomeTokenWithNoExtraData_ThrowsIfTokenInvalid()
            {
                // Arrange.
                var antiForgery = new AntiForgery();
                var guid = Guid.NewGuid();
                var token = guid.ToString();

                // Act/Assert.
                Assert.Throws<AuthenticationException>(() => antiForgery.ValidateToken(token, "YOU'VE BEEN HAXED SUCKA!"));
            }

            [Fact]
            public void GivenSomeTokenWithExtraData_ReturnsExtraDataIfTokenValid()
            {
                // Arrange.
                const string expectedExtraData = "/abc/123";
                var antiForgery = new AntiForgery();
                var guid = Guid.NewGuid();
                var token = guid.ToString();
                string kept = String.Format("{0}|{1}", token, Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedExtraData)));

                // Act.
                var actualExtraData = antiForgery.ValidateToken(kept, token);
 
                 // Assert.
                Assert.Equal(expectedExtraData, actualExtraData);
            }

            [Fact]
            public void GivenSomeTokenWithExtraData_ThrowsIfTokenInvalid()
            {
                // Arrange.
                const string expectedExtraData = "/abc/123";
                var antiForgery = new AntiForgery();
                var guid = Guid.NewGuid();
                var token = guid.ToString();
                string kept = String.Format("{0}|{1}", token, Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedExtraData)));

                // Act/Assert.
                Assert.Throws<AuthenticationException>(() => antiForgery.ValidateToken(token, "YOU'VE BEEN HAXED SUCKA!"));
             }

            [Fact]
            public void GivenSomeBadExtraData_ValidateToken_ReturnsABaddaBingBaddaBoom()
            {
                // Arrange.
                var antiForgery = new AntiForgery();
                const string badToken = "MultiPass|Bzzzzzt";

                // Act.
                var result = Assert.Throws<FormatException>(() => antiForgery.ValidateToken(badToken, "MultiPass"));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Invalid length for a Base-64 char array or string.", result.Message);
            }
        }
    }

    // ReSharper restore InconsistentNaming
}