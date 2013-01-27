using System;
using System.Web;
using System.Web.Mvc;
using WorldDomination.Web.Authentication.Facebook;
using WorldDomination.Web.Authentication.Google;
using WorldDomination.Web.Authentication.Samples.Mvc.Simple.Models;
using WorldDomination.Web.Authentication.Twitter;

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
            var facebookProvider = new FacebookProvider(FacebookAppId, FacebookAppSecret);
            var twitterProvider = new TwitterProvider(TwitterConsumerKey, TwitterConsumerSecret);
            var googleProvider = new GoogleProvider(GoogleConsumerKey, GoogleConsumerSecret);

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
            // Determine the callback Uri based on the server details.
            var callBackUri = new Uri(ToAbsoluteUrl(Url.Action("AuthenticateCallback", new {providerKey})));

            // Determine the full redirect uri.
            var uri = AuthenticationService.RedirectToAuthenticationProvider(providerKey, callBackUri);

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
                model.AuthenticatedClient = AuthenticationService.GetAuthenticatedClient(providerKey, Request.Params);
            }
            catch (Exception exception)
            {
                model.Exception = exception;
            }

            return View(model);
        }

        // Based upon StackOverflow Q: http://stackoverflow.com/questions/3681052/get-absolute-url-from-relative-path-refactored-method
        private string ToAbsoluteUrl(string relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl))
            {
                return relativeUrl;
            }

            if (HttpContext == null)
            {
                return relativeUrl;
            }

            if (relativeUrl.StartsWith("/"))
            {
                relativeUrl = relativeUrl.Insert(0, "~");
            }
            if (!relativeUrl.StartsWith("~/"))
            {
                relativeUrl = relativeUrl.Insert(0, "~/");
            }

            var url = HttpContext.Request.Url;
            var port = url.Port != 80 ? (":" + url.Port) : string.Empty;

            return string.Format("{0}://{1}{2}{3}",
                                 url.Scheme, url.Host, port, VirtualPathUtility.ToAbsolute(relativeUrl));
        }
    }
}