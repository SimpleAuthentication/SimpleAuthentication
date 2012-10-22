using System;
using System.Web.Mvc;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Facebook;
using WorldDomination.Web.Authentication.Google;
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
        private const string GoogleConsumerKey = "587140099194.apps.googleusercontent.com";
        private const string GoogleConsumerSecret = "npk1_gx-gqJmLiJRPFooxCEY";
        

        public HomeController()
        {
            var facebookProvider = new FacebookProvider(FacebookAppId, FacebookAppSecret,
                                                    new Uri("http://localhost:1337/home/AuthenticateCallback?providerKey=facebook"));

            var twitterProvider = new TwitterProvider(TwitterConsumerKey, TwitterConsumerSecret,
                                                      new Uri("http://localhost:1337/home/AuthenticateCallback?providerKey=twitter"));

            var googleProvider = new GoogleProvider(GoogleConsumerKey, GoogleConsumerSecret,
                                                      new Uri("http://localhost:1337/home/AuthenticateCallback?providerKey=google"));

            _authenticationService = new AuthenticationService();
            _authenticationService.AddProvider(facebookProvider);
            _authenticationService.AddProvider(twitterProvider);
            _authenticationService.AddProvider(googleProvider);
        }

        private readonly AuthenticationService _authenticationService;

        public ActionResult Index()
        {
            return View();
        }

        public RedirectResult RedirectToAuthenticate(string providerKey)
        {
            // Keep the SessionId constant. 
            // Otherwise, you'll need to store some constant value in session .. and use that instead of the Session Id.
            Session.Add("SomeKey", "whatcha-talkin-bout-willis?"); 
            var uri = _authenticationService.RedirectToAuthenticationProvider(providerKey, Session.SessionID);
            return Redirect(uri.AbsoluteUri);
        }

        public ActionResult AuthenticateCallback(string providerKey)
        {
            if (string.IsNullOrEmpty(providerKey))
            {
                throw new ArgumentNullException("providerKey");
            }

            var model = new AuthenticateCallbackViewModel();
            try
            {
                model.AuthenticatedClient = _authenticationService.CheckCallback(providerKey, Request, Session.SessionID);
            }
            catch (Exception exception)
            {
                model.Exception = exception;
            }
            
            return View(model);
        }
    }
}