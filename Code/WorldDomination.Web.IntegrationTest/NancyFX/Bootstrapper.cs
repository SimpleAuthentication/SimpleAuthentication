using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Session;
using TinyIoC;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Twitter;

namespace WorldDomination.Web.IntegrationTest.NancyFX
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private const string TwitterConsumerKey = "Rb7qNNPUPsRSYkznFTbF6Q";
        private const string TwitterConsumerSecret = "pP1jBdYOlmCzo08QFJjGIHY4YSyPdGLPO2m1q47hu9c";

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            var twitterProvider = new TwitterProvider(TwitterConsumerKey, TwitterConsumerSecret,
                                                    new Uri("http://localhost:49409/AuthenticateCallback?providerKey=Twitter"));

            var authenticationService = new AuthenticationService();
            authenticationService.AddProvider(twitterProvider);

            container.Register<IAuthenticationService>(authenticationService);

            base.ApplicationStartup(container, pipelines);

            CookieBasedSessions.Enable(pipelines);
        }
    }
}