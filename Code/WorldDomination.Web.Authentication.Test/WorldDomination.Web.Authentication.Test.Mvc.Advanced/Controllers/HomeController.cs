using System;
using System.Web.Mvc;
using CuttingEdge.Conditions;
using WorldDomination.Web.Authentication.Facebook;
using WorldDomination.Web.Authentication.Google;
using WorldDomination.Web.Authentication.Test.Mvc.Advanced.Models;
using WorldDomination.Web.Authentication.Twitter;

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
            // Keep the SessionId constant. 
            // Otherwise, you'll need to store some constant value in session .. and use that instead of the Session Id.
            var uri = _authenticationService.RedirectToAuthenticationProvider(providerKey);
            return Redirect(uri.AbsoluteUri);
        }

        public RedirectResult RedirectToFacebookMobile()
        {
            // Keep the SessionId constant. 
            // Otherwise, you'll need to store some constant value in session .. and use that instead of the Session Id.
            Session.Add(SessionStateKey, "whatcha-talkin-bout-willis?");
            var uri = _authenticationService.RedirectToAuthenticationProvider(new FacebookAuthenticationServiceSettings
                                                                              {
                                                                                  State = Session[SessionStateKey] as string,
                                                                                  IsMobile = true
                                                                              });
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
                // ProTip: It's possible that the session value could be null, here. Which is fine.
                //         I would be null if it wasn't created, such as with the 'simple' RedirectToAuthenticate method (above).
                model.AuthenticatedClient = _authenticationService.CheckCallback(providerKey, Request.Params,
                                                                                 Session[SessionStateKey] as string);

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