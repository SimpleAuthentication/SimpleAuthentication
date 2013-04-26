using System;
using System.Web;
using System.Web.Mvc;
using CuttingEdge.Conditions;
using WorldDomination.Web.Authentication.Csrf;
using WorldDomination.Web.Authentication.Providers.Facebook;
using WorldDomination.Web.Authentication.Samples.Mvc.Advanced.Models;

namespace WorldDomination.Web.Authentication.Samples.Mvc.Advanced.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAntiForgery _antiForgery;

        private readonly IAuthenticationService _authenticationService;

        public HomeController(IAuthenticationService authenticationService,
                              IAntiForgery antiForgery)
        {
            Condition.Requires(authenticationService).IsNotNull();
            Condition.Requires(antiForgery).IsNotNull();

            _authenticationService = authenticationService;
            _antiForgery = antiForgery;
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

            // For shits and giggles, we'll remember the referrer to highlight that we can
            // redirect back to where we started, if we want to.
            string referrer = null;
            if (Request != null &&
                Request.UrlReferrer != null &&
                !string.IsNullOrEmpty(Request.UrlReferrer.AbsoluteUri))
            {
                referrer = Request.UrlReferrer.AbsoluteUri;
            }

            // Create the CRSF Token.
            var token = _antiForgery.CreateToken(referrer);
            settings.State = token.ToSend;

            // Remember this token for when we are handling the callback.
            var cookie = new HttpCookie(_antiForgery.DefaultCookieName) { Value = token.ToKeep, HttpOnly = true };
            Response.Cookies.Add(cookie);

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

            // For shits and giggles, we'll remember the referrer to highlight that we can
            // redirect back to where we started, if we want to.
            string referrer = null;
            if (Request != null &&
                Request.UrlReferrer != null &&
                !string.IsNullOrEmpty(Request.UrlReferrer.AbsoluteUri))
            {
                referrer = Request.UrlReferrer.AbsoluteUri;
            }

            // Create the CRSF Token.
            var token = _antiForgery.CreateToken(referrer);
            settings.State = token.ToSend;

            // Remember this token for when we are handling the callback.
            var cookie = new HttpCookie(_antiForgery.DefaultCookieName) { Value = token.ToKeep, HttpOnly = true };
            Response.Cookies.Add(cookie);

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
                var existingCookie = Request.Cookies[_antiForgery.DefaultCookieName];
                var token = existingCookie != null ? existingCookie.Value : null;
                settings.State = token;

                // Lets clean up.
                Request.Cookies.Remove(_antiForgery.DefaultCookieName);

                // Validate Cookie
                var extraData = _antiForgery.ValidateToken(token, Request.QueryString["state"]);

                // Grab the authenticated client information.
                model.AuthenticatedClient = _authenticationService.GetAuthenticatedClient(settings, Request.QueryString);

                if (!string.IsNullOrEmpty(extraData))
                {
                    model.Referrer = new Uri(extraData);
                }
            }
            catch (Exception exception)
            {
                model.Exception = exception;
            }

            return View(model);
        }
    }
}