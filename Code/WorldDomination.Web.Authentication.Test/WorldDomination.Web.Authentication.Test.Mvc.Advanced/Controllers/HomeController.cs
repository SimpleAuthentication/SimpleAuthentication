using System;
using System.Web.Mvc;
using CuttingEdge.Conditions;
using WorldDomination.Web.Authentication.Facebook;
using WorldDomination.Web.Authentication.Test.Mvc.Advanced.Models;

namespace WorldDomination.Web.Authentication.Test.Mvc.Advanced.Controllers
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
            var settings = AuthenticationServiceSettingsFactory.GetAuthenticateServiceSettings(providerKey);

            // We need to remember the state for some XSS protection.
            Session[SessionStateKey] = Guid.NewGuid();
            settings.State = Session[SessionStateKey].ToString();

            // Grab the Uri we need redirect to.
            var uri = _authenticationService.RedirectToAuthenticationProvider(settings);

            // Redirect!
            return Redirect(uri.AbsoluteUri);
        }

        public RedirectResult RedirectToFacebookMobile()
        {
            // We need to remember the state for some XSS protection.
            Session[SessionStateKey] = Guid.NewGuid();

            // Grab the Uri we need redirect to.
            var uri = _authenticationService.RedirectToAuthenticationProvider(new FacebookAuthenticationServiceSettings
                                                                              {
                                                                                  State = Session[SessionStateKey].ToString(),
                                                                                  IsMobile = true
                                                                              });

            // Redirect!
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
                // Retrieve the state for the XSS check.
                var state = Session[SessionStateKey] == null ? null : Session[SessionStateKey].ToString();
                
                // Complete the authentication process by retrieving the UserInformation from the provider.
                model.AuthenticatedClient = _authenticationService.CheckCallback(providerKey, Request.Params, state);

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