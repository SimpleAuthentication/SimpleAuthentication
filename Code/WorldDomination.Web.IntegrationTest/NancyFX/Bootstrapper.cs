using System;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Session;
using TinyIoC;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Facebook;
using WorldDomination.Web.Authentication.Google;
using WorldDomination.Web.Authentication.Twitter;

namespace WorldDomination.Web.IntegrationTest.NancyFX
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private const string TwitterConsumerKey = "Rb7qNNPUPsRSYkznFTbF6Q";
        private const string TwitterConsumerSecret = "pP1jBdYOlmCzo08QFJjGIHY4YSyPdGLPO2m1q47hu9c";
        private const string FacebookAppId = "159181340893340";
        private const string FacebookAppSecret = "97c4e4d0fa548232cf8f9c68a7adcff9";
        private const string GoogleConsumerKey = "587140099194.apps.googleusercontent.com";
        private const string GoogleConsumerSecret = "npk1_gx-gqJmLiJRPFooxCEY";

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            var twitterProvider = new TwitterProvider(TwitterConsumerKey, TwitterConsumerSecret,
                                                    new Uri("http://localhost:6969/AuthenticateCallback?providerKey=Twitter"));

            var facebookProvider = new FacebookProvider(FacebookAppId, FacebookAppSecret,
                                                       new Uri(
                                                           "http://localhost:6969/AuthenticateCallback?providerKey=facebook"));

            var googleProvider = new GoogleProvider(GoogleConsumerKey, GoogleConsumerSecret,
                                                    new Uri(
                                                        "http://localhost:6969/AuthenticateCallback?providerKey=google"));

            var authenticationService = new AuthenticationService();
            authenticationService.AddProvider(twitterProvider);
            authenticationService.AddProvider(facebookProvider);
            authenticationService.AddProvider(googleProvider);

            container.Register<IAuthenticationService>(authenticationService);

            base.ApplicationStartup(container, pipelines);

            CookieBasedSessions.Enable(pipelines);
        }
    }
}