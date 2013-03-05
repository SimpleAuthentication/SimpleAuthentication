using System;
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
            public void GivenNoExtraData_CreateToken_ReturnsAGuid()
            {
                // Arrange.
                var antiForgery = new AntiForgery();

                // Act.
                var result = antiForgery.CreateToken();

                // Assert.
                Assert.NotNull(result);
                Guid guid;
                Guid.TryParse(result, out guid);
                Assert.IsType<Guid>(guid);
            }

            [Fact]
            public void GivenSomeExtraData_CreateToken_ReturnsAFunkyString()
            {
                // Arrage.
                const string extraData = "http://2p1s.com";
                var antiForgery = new AntiForgery();

                // Act.
                var result = antiForgery.CreateToken(extraData);

                // Assert.
                Assert.NotNull(result);
                Assert.True(result.Contains("|"));
                Assert.Equal("aAB0AHQAcAA6AC8ALwAyAHAAMQBzAC4AYwBvAG0A",
                             result.Substring(result.IndexOf("|", StringComparison.Ordinal) + 1));
            }
        }

        public class ValidateTokenFacts
        {
            [Fact]
            public void GivenSomeTokenWhichIsJustAGuid_ValidateToken_ReturnsATokenData()
            {
                // Arrange.
                var antiForgery = new AntiForgery();
                var guid = Guid.NewGuid();
                var token = guid.ToString();

                // Act.
                var result = antiForgery.ValidateToken(token);

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(guid.ToString(), result.State);
                Assert.Null(result.ExtraData);
            }

            [Fact]
            public void GivenSomeTokenWhichHasAGuidAndExtraData_ValidateToken_ReturnsATokenData()
            {
                // Arrange.
                var antiForgery = new AntiForgery();
                var guid = Guid.NewGuid();
                var token = guid.ToString();

                // Act.
                var result = antiForgery.ValidateToken(token);

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(guid.ToString(), result.State);
                Assert.Null(result.ExtraData);
            }

            [Fact]
            public void GivenSomeBadExtraData_ValidateToken_ReturnsABaddaBingBaddaBoom()
            {
                // Arrange.
                var antiForgery = new AntiForgery();
                const string badToken = "MultiPass|Bzzzzzt";

                // Act.
                var result = Assert.Throws<FormatException>(() => antiForgery.ValidateToken(badToken));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Invalid length for a Base-64 char array or string.", result.Message);
            }
        }
    }

    // ReSharper restore InconsistentNaming
}