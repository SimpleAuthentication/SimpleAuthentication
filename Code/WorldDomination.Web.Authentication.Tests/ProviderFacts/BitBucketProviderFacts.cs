//using System;
//using Xunit;

//namespace WorldDomination.Web.Authentication.Tests
//{
//    // ReSharper disable InconsistentNaming

//    public class BitBucketProviderFacts
//    {
//        public class RedirectToAuthenticateFacts
//        {
//            [Fact]
//            public void
//                GivenASomething_RedirectToAuthenticateGivenAValidRequestToken_RedirectToAuthenticate_ReturnsARedirectResult
//                ()
//            {
//                var bitBucketProvider = new BitBucketProvider(new ProviderParams { Key = "xXHJtx5cxYEsXqa8jK", Secret = "7KYfuV7f8Xkr7EG7wTdw2SLWgY5VfUjr"});
//                var authenticationServiceSettings = new BitBucketAuthenticationServiceSettings
//                {
//                    CallBackUri = new Uri("http://localhost:2183")
//                };
//                var result = bitBucketProvider.RedirectToAuthenticate(authenticationServiceSettings);

//                // Assert.
//                Assert.NotNull(result);
//            }
//        }
//    }

//    // ReSharper restore InconsistentNaming
//}