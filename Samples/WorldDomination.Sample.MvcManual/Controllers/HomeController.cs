using System;
using System.Web.Mvc;
using WorldDomination.Sample.MvcManual.Models;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Providers;

namespace WorldDomination.Sample.MvcManual.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        
        public HomeController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
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
            var settings = _authenticationService.GetAuthenticateServiceSettings(providerKey, Request.Url, "home/authenticatecallback");

            // Do use a state key.
            settings.State = null; 

            // Determine the provider's end point Url we need to redirect to.
            var uri = _authenticationService.RedirectToAuthenticationProvider(settings);

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
            var settings = _authenticationService.GetAuthenticateServiceSettings(providerKey, Request.Url, "home/authenticatecallback");

            // Don't check for somet State.
            settings.State = null;

            var model = new AuthenticateCallbackViewModel();

            try
            {
                // Grab the authenticated client information.
                model.AuthenticatedClient = _authenticationService.GetAuthenticatedClient(settings, Request.QueryString);
            }
            catch (Exception exception)
            {
                model.Exception = exception;
            }

            return View(model);
        }
    }
}