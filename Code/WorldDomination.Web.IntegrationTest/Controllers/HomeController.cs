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
            _facebookProvider = new FacebookProvider(FacebookAppId,
                                                    FacebookAppSecret,
                                                    new Uri("http://localhost:1337/home/authenticateCallback"));

            _twitterProvider = new TwitterProvider(TwitterConsumerKey, TwitterConsumerSecret);

            _authenticationService = new AuthenticationService(_facebookProvider, _twitterProvider);
        }

        private readonly FacebookProvider _facebookProvider;
        private readonly TwitterProvider _twitterProvider;
        private readonly AuthenticationService _authenticationService;

        public ActionResult Index()
        {
            return View();
        }

        public RedirectResult FacebookAuthentication()
        {
            Session.Add("a", "a"); // Keep the SessionId constant.
            return _authenticationService.RedirectToFacebookAuthentication(Session.SessionID);
        }

        public RedirectResult TwitterAuthentication()
        {
            return _authenticationService.RedirectToTwitterAuthentication("http://localhost:1337/home/AuthenticateCallback");
            
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