using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shouldly;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers;
using Xunit;

namespace SimpleAuthentication.Tests.Providers
{
    public class FakeProviderFacts
    {
        public class AuthenticateClientAsyncFacts
        {
            [Fact]
            public async Task GivenAValidQuerystring_AuthenticateClientAsync_ReturnsAnAuthenticatedUser()
            {
                // Arrange.
                const string providerName = "FakePewPewProvider";
                var provider = new FakeProvider(providerName);
                const string state = "0A06683F-7008-42B4-B0D2-95935BEA5122";
                var querystring = new Dictionary<string, string>
                {
                    {"state", state}
                };

                // Act.
                var result = await provider.AuthenticateClientAsync(querystring,
                    state,
                    new Uri("http://www.pewpew.com/a/b/c/d"));

                // Assert.
                result.ProviderName.ShouldBe(providerName);
                result.UserInformation.Email.ShouldBe("sturm.brightblade@knights-of-the-rose.krynn");
                result.UserInformation.Gender.ShouldBe(GenderType.Male);
                result.UserInformation.Id.ShouldBe("FakeId-ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                result.UserInformation.Picture.Length.ShouldBeGreaterThan(0);
                result.UserInformation.UserName.ShouldBe("Sturm.Brightblade");
                result.UserInformation.Name.ShouldBe("Sturm Brightblade");
                result.RawUserInformation.ShouldNotBe(null);
            }

            [Fact]
            public void GivenAnExceptionMessage_AuthenticateClientAsync_ThrowsAnException()
            {
                // Arrange.
                const string exceptionMessage = "ro-ruh. something bad has happened again!";
                const string providerName = "FakePewPewProvider";
                var provider = new FakeProvider(providerName)
                {
                    AuthenticateClientAsyncExceptionMessage = exceptionMessage
                };

                // Act.
                var exception = Should.Throw<AuthenticationException>(async () =>
                    await provider.AuthenticateClientAsync(new Dictionary<string, string>(),
                        "pew pew",
                        new Uri("http://www.pewpew.com/a/b/c/d")));

                // Assert.
                exception.Message.ShouldBe(exceptionMessage);
            }

            [Fact]
            public async Task GivenAUserInformationAndAccessToken_AuthenticateClientAsync_ReturnsAnAuthenticatedUser()
            {
                // Arrange.
                var userInformation = new UserInformation
                {
                    Email = "Tasslehoff.Burrfoot@kendermore.krynn",
                    Gender = GenderType.Male,
                    Id = "Tas786^&hjdds",
                    Locale = "en-au",
                    Name = "Tasslehoff Burrfoot",
                    UserName = "Tasslehoff.Burrfoot"
                };
                var accessToken = new AccessToken
                {
                    Token = "ABCDE 1234",
                    ExpiresOn = DateTime.UtcNow.AddDays(5)
                };

                const string providerName = "FakePewPewProvider";
                var provider = new FakeProvider(providerName)
                {
                    UserInformation = userInformation,
                    AccessToken = accessToken
                };

                // Act.
                var result = await provider.AuthenticateClientAsync(new Dictionary<string, string>(),
                    "pewpew",
                    new Uri("http://www.pewpew.com/a/b/c/d"));

                // Assert.
                result.ProviderName.ShouldBe(providerName);
                result.UserInformation.Email.ShouldBe(userInformation.Email);
                result.UserInformation.Gender.ShouldBe(GenderType.Male);
                result.UserInformation.Id.ShouldBe(userInformation.Id);
                result.UserInformation.Picture.ShouldBe(null);
                result.UserInformation.UserName.ShouldBe(userInformation.UserName);
                result.UserInformation.Name.ShouldBe(userInformation.Name);
            }
        }

        public class GetRedirectToAuthenticateSettingsAsyncFacts
        {
            [Fact]
            public async Task
                GivenARequestUri_GetRedirectToAuthenticateSettingsAsync_ReturnsSomeRedirectToAuthenticateSettings()
            {
                // Arrange.
                var provider = new FakeProvider("FakePewPewProvider");
                var requestUri = new Uri("http://www.pewpew.com/a/b/c?provider=fakePewPewProvider");

                // Act.
                var result = await provider.GetRedirectToAuthenticateSettingsAsync(requestUri);

                // Assert.
                result.State.ShouldNotBeNullOrEmpty();
                var redirectUrl = string.Format("http://www.pewpew.com/a/b/c?provider=fakePewPewProvider&state={0}",
                    result.State);
                result.RedirectUri.AbsoluteUri.ShouldBe(redirectUrl);
            }

            [Fact]
            public void GivenAnExceptionMessage_GetRedirectToAuthenticateSettingsAsync_ThrowsAnException()
            {
                // Arrange.
                const string exceptionMessage = "Something bad has happened. Ru-roh.";
                var provider = new FakeProvider("FakeewPewProvider")
                {
                    RedirectToAuthenticateAsyncExceptionMessage = exceptionMessage
                };
                var requestUri = new Uri("http://www.pewpew.com/a/b/c?provider=fakePewPewProvider");

                // Act.
                var exception = Should.Throw<AuthenticationException>(async () =>
                    await provider.GetRedirectToAuthenticateSettingsAsync(requestUri));

                // Assert.
                exception.Message.ShouldBe(exceptionMessage);
            }
        }
    }
}