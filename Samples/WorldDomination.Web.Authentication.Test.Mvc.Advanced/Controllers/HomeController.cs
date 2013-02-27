using System;
using System.Web.Mvc;
using CuttingEdge.Conditions;
using WorldDomination.Web.Authentication.Facebook;
using WorldDomination.Web.Authentication.Samples.Mvc.Advanced.Models;

namespace WorldDomination.Web.Authentication.Samples.Mvc.Advanced.Controllers
{
    public class HomeController : Controller
    {
        private const string SessionStateKey = "SomeKey";

        private readonly IAuthenticationService _authenticationService;

        public HomeController(IAuthenticationService authenticationService)
        {
            Condition.Requires(authenticationService).IsNotNull();

            _authenticationService = authenticationService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public RedirectResult RedirectToAuthenticate(string providerKey)
        {
            // Which provider are we after?
            // NOTE: We don't want to use the default callback route, so we're specifying our own route, here.
            var settings = _authenticationService.GetAuthenticateServiceSettings(providerKey, Request.Url,
                                                                                 "home/authenticatecallback");

            // We need to remember the state for some CRSF protection.
            Session[SessionStateKey] = settings.State;

            // Determine the provider's end point Url we need to redirect to.
            var uri = _authenticationService.RedirectToAuthenticationProvider(settings);

            // Kthxgo!
            return Redirect(uri.AbsoluteUri);
        }

        public RedirectResult RedirectToFacebookMobile()
        {
            // Which provider are we after?
            // NOTE: We don't want to use the default callback route, so we're specifying our own route, here.
            var settings = _authenticationService.GetAuthenticateServiceSettings("facebook", Request.Url,
                                                                                 "home/authenticatecallback");

            // We need to remember the state for some CRSF protection.
            Session[SessionStateKey] = settings.State;

            // Set the IsMobile facebook provider specific settings.
            ((FacebookAuthenticationServiceSettings) settings).IsMobile = true;

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

            var model = new AuthenticateCallbackViewModel();
            try
            {
                // Determine which settings we need, based on the Provider.
                // NOTE: We don't want to use the default callback route, so we're specifying our own route, here.
                var settings = _authenticationService.GetAuthenticateServiceSettings(providerKey, Request.Url,
                                                                                     "home/authenticatecallback");

                // Make sure we use our 'previous' State value.
                settings.State = (Session[SessionStateKey] as string) ?? string.Empty;

                // Grab the authenticated client information.
                model.AuthenticatedClient = _authenticationService.GetAuthenticatedClient(settings, Request.QueryString);

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