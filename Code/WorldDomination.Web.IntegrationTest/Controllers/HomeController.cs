using System;
using System.Web.Mvc;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Facebook;
using WorldDomination.Web.Authentication.Twitter;
using WorldDomination.Web.IntegrationTest.Models;

namespace WorldDomination.Web.IntegrationTest.Controllers
{
    public class HomeController : Controller
    {
        private const string FacebookAppId = "113220502168922";
        private const string FacebookAppSecret = "b09592a5904746646f3d402178ce9c0f";
        private const string TwitterConsumerKey = "Rb7qNNPUPsRSYkznFTbF6Q";
        private const string TwitterConsumerSecret = "pP1jBdYOlmCzo08QFJjGIHY4YSyPdGLPO2m1q47hu9c";

        public HomeController()
        {
            var facebookProvider = new FacebookProvider(FacebookAppId,
                                                    FacebookAppSecret,
                                                    new Uri("http://localhost:1337/home/AuthenticateCallback"));

            var twitterProvider = new TwitterProvider(TwitterConsumerKey, TwitterConsumerSecret,
                                                      new Uri("http://localhost:1337/home/AuthenticateCallback"));

            _authenticationService = new AuthenticationService();
            _authenticationService.AddProvider(facebookProvider);
            _authenticationService.AddProvider(twitterProvider);
        }

        private readonly AuthenticationService _authenticationService;

        public ActionResult Index()
        {
            return View();
        }

        public RedirectResult FacebookAuthentication()
        {
            // Keep the SessionId constant. 
            // Otherwise, you'll need to store some constant value in session .. and use that instead of the Session Id.
            Session.Add("SomeKey", "whatcha-talkin-bout-willis?"); 
            return _authenticationService.RedirectToFacebookAuthentication(Session.SessionID);
        }

        public RedirectResult TwitterAuthentication()
        {
            // Note: Twitter doesn't use the state param. So it can be anything non-null.
            return _authenticationService.RedirectToTwitterAuthentication(Session.SessionID);
        }

        public ActionResult AuthenticateCallback()
        {
            var model = new AuthenticateCallbackViewModel();
            try
            {
                model.AuthenticatedClient = _authenticationService.CheckCallback(Request, Session.SessionID);
            }
            catch (Exception exception)
            {
                model.Exception = exception;
            }
            
            return View(model);
        }
    }
}