using System;
using System.Web;
using System.Web.Mvc;
using WorldDomination.Web.Authentication.Providers;
using WorldDomination.Web.Authentication.Samples.Mvc.Simple.Models;

namespace WorldDomination.Web.Authentication.Samples.Mvc.Simple.Controllers
{
    public class HomeController : Controller
    {
        private const string FacebookAppId = "113220502168922";
        private const string FacebookAppSecret = "b09592a5904746646f3d402178ce9c0f";
        private const string TwitterConsumerKey = "Rb7qNNPUPsRSYkznFTbF6Q";
        private const string TwitterConsumerSecret = "pP1jBdYOlmCzo08QFJjGIHY4YSyPdGLPO2m1q47hu9c";
        private const string GoogleConsumerKey = "587140099194.apps.googleusercontent.com";
        private const string GoogleConsumerSecret = "npk1_gx-gqJmLiJRPFooxCEY";

        private static readonly AuthenticationService AuthenticationService;

        static HomeController()
        {
            // For the purpose of this example we just made the service static in 
            // a static constructor, normally you would do this using dependency injection
            // but for the take of simplicity we added it it here. Please refer
            // to the Advanced sample for the DI version. Don't use a static constructor
            // like this in your project, please. :)
            var facebookProvider = new FacebookProvider(new ProviderParams { Key = FacebookAppId, Secret = FacebookAppSecret });
            var twitterProvider = new TwitterProvider(new ProviderParams { Key = TwitterConsumerKey, Secret = TwitterConsumerSecret });
            var googleProvider = new GoogleProvider(new ProviderParams { Key = GoogleConsumerKey, Secret = GoogleConsumerSecret });

            AuthenticationService = new AuthenticationService();
            AuthenticationService.AddProvider(facebookProvider);
            AuthenticationService.AddProvider(twitterProvider);
            AuthenticationService.AddProvider(googleProvider);
        }

        public ActionResult Index()
        {
            return View();
        }

        public RedirectResult RedirectToAuthenticate(string providerKey)
        {
            if (string.IsNullOrEmpty(providerKey))
            {
                throw new ArgumentNullException("providerKey");
            }

            // Grab the required Provider settings.
            var settings = AuthenticationService.GetAuthenticateServiceSettings(providerKey, Request.Url, "home/authenticatecallback");

            // Do use a state key.
            settings.State = null; 

            // Determine the provider's end point Url we need to redirect to.
            var uri = AuthenticationService.RedirectToAuthenticationProvider(settings);

            // Kthxgo!
            return Redirect(uri.AbsoluteUri);
        }

        public ActionResult AuthenticateCallback(string providerKey)
        {
            if (string.IsNullOrEmpty(providerKey))
            {
                throw new ArgumentNullException("providerKey");
            }

            // Determine which settings we need, based on the Provider.
            var settings = AuthenticationService.GetAuthenticateServiceSettings(providerKey, Request.Url, "home/authenticatecallback");

            // Don't check for somet State.
            settings.State = null;

            var model = new AuthenticateCallbackViewModel();

            try
            {
                // Grab the authenticated client information.
                model.AuthenticatedClient = AuthenticationService.GetAuthenticatedClient(settings, Request.QueryString);
            }
            catch (Exception exception)
            {
                model.Exception = exception;
            }

            return View(model);
        }
    }
}