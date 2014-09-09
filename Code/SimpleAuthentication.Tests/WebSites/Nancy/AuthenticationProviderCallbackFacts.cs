using System;
using FakeItEasy;
using Nancy;
using Nancy.SimpleAuthentication;
using SimpleAuthentication.Core;
using Xunit;

namespace SimpleAuthentication.Tests.WebSites.Nancy
{
    public class AuthenticationProviderCallbackFacts
    {
        public class ProcessFacts
        {
            [Fact(Skip = "TODO")]
            public void GivenAnAuthenticatedClientAndNoReturnUrl_Process_ReturnsAView()
            {
                // Arrange.
                var nancyModule = A.Fake<INancyModule>();
                var accessToken = new AccessToken
                {
                    ExpiresOn = new DateTime(2020, 5, 23),
                    Token = "B687DAD0-8029-47DB-9BC3-1F2F40624377"
                };
                var userInformation = new UserInformation
                {
                    Email = "sturm.brightblade@KnightsOfTheRose.krynn",
                    Gender = GenderType.Male,
                    Id = "1234A-SB",
                    Name = "Sturm Brightblade",
                    UserName = "Sturm.Brightblade"
                };
                var authenticationProviderCallback = new FakeAuthenticationProviderCallback();

                var authenticationCallbackResult = new AuthenticateCallbackResult
                {
                    AuthenticatedClient = new AuthenticatedClient("Google",
                        accessToken,
                        userInformation,
                        "some raw info goes here pew pew pew")
                };

                // Act.
                var result = authenticationProviderCallback.Process(nancyModule, authenticationCallbackResult);

                // Assert.
                // Not sure how to do this yet.
            }

            [Fact(Skip = "TODO")]
            public void GivenAnAuthenticatedClientAndAReturnUrl_Process_ReturnsARedirection()
            {
            }

            [Fact(Skip = "TODO")]
            public void GivenNoAuthenticatedClientAndNoReturnUrl_Process_ReturnsARedirection()
            {
            }

            [Fact(Skip = "TODO")]
            public void GivenNoAuthenticatedClientAndAReturnUrl_Process_ReturnsARedirection()
            {
            }
        }
    }
}