using System;
using System.Web.Mvc;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Facebook;
using WorldDomination.Web.Authentication.Google;
using WorldDomination.Web.Authentication.Twitter;
using WorldDomination.Web.IntegrationTest.Mvc.Models;

namespace WorldDomination.Web.Application.Controllers
{
    public class AuthenticationController : Controller
    {
        private const string FacebookAppId = "SomeId";
        private const string FacebookAppSecret = "SomeAppSecret";
        private const string TwitterConsumerKey = "SomeKey";
        private const string TwitterConsumerSecret = "SomeSecret";
        private const string GoogleConsumerKey = "SomeKey";
        private const string GoogleConsumerSecret = "SomeSecret";
        private const string SessionStateKey = "SomeKey";

        private readonly AuthenticationService _authenticationService;

        // STEP 1: Define which Authentication Providers you wish to offer.
        public AuthenticationController()
        {
            var facebookProvider = new FacebookProvider(FacebookAppId, FacebookAppSecret,
                                                        new Uri(
                                                            "http://localhost:1337/home/AuthenticateCallback?providerKey=facebook"));

            var twitterProvider = new TwitterProvider(TwitterConsumerKey, TwitterConsumerSecret,
                                                      new Uri(
                                                          "http://localhost:1337/home/AuthenticateCallback?providerKey=twitter"));

            var googleProvider = new GoogleProvider(GoogleConsumerKey, GoogleConsumerSecret,
                                                    new Uri(
                                                        "http://localhost:1337/home/AuthenticateCallback?providerKey=google"));

            _authenticationService = new AuthenticationService();
            _authenticationService.AddProvider(facebookProvider);
            _authenticationService.AddProvider(twitterProvider);
            _authenticationService.AddProvider(googleProvider);
        }

        public ActionResult Index()
        {
            return View();
        }

        // STEP #2: Redirect the client to an Authentication Provider (eg. Google, etc).
        public RedirectResult RedirectToAuthenticate(string providerKey)
        {
            // Keep the SessionId constant. 
            // Otherwise, you'll need to store some constant value in session .. and use that instead of the Session Id.
            var uri = _authenticationService.RedirectToAuthenticationProvider(providerKey);
            return Redirect(uri.AbsoluteUri);
        }

        // STEP #3: Handle the callback from the Authentication Provider. (eg. Google, etc).
        public ActionResult AuthenticateCallback(string providerKey)
        {
            if (string.IsNullOrEmpty(providerKey))
            {
                throw new ArgumentNullException("providerKey");
            }

            var model = new AuthenticateCallbackViewModel();
            try
            {
                // ProTip: It's possible that the session value could be null, here. Which is fine.
                //         I would be null if it wasn't created, such as with the 'simple' RedirectToAuthenticate method (above).
                model.AuthenticatedClient = _authenticationService.CheckCallback(providerKey, Request.Params);

                // Clean up after ourselves like a nice little boy/girl/monster we are.
                Session.Remove(SessionStateKey);
            }
            catch (Exception exception)
            {
                model.Exception = exception;
            }

            return View(model);
        }
    }
}