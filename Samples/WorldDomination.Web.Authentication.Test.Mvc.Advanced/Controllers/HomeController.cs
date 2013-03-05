using System;
using System.Web;
using System.Web.Mvc;
using CuttingEdge.Conditions;
using WorldDomination.Web.Authentication.Csrf;
using WorldDomination.Web.Authentication.Facebook;
using WorldDomination.Web.Authentication.Samples.Mvc.Advanced.Models;

namespace WorldDomination.Web.Authentication.Samples.Mvc.Advanced.Controllers
{
    public class HomeController : Controller
    {
        private const string CookieName = "__WorldDomination.Web.Authentication.Mvc.CsrfToken";
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

            // Create teh CRSF Token.
            var token = _antiForgery.CreateToken(referrer);
            settings.State = token;

            // Remember this token for when we are handling the callback.
            var cookie = new HttpCookie(CookieName) { Value = token };
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

            // Create teh CRSF Token.
            var token = _antiForgery.CreateToken(referrer);
            settings.State = token;

            // Remember this token for when we are handling the callback.
            var cookie = new HttpCookie(CookieName) { Value = token };
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
                var existingCookie = Request.Cookies[CookieName];
                var token = existingCookie != null ? existingCookie.Value : null;
                settings.State = token;

                // Lets clean up.
                Request.Cookies.Remove(CookieName);

                // Grab the authenticated client information.
                model.AuthenticatedClient = _authenticationService.GetAuthenticatedClient(settings, Request.QueryString);

                var tokenData = _antiForgery.ValidateToken(token);
                if (tokenData != null && !string.IsNullOrEmpty(tokenData.ExtraData))
                {
                    model.Referrer = new Uri(tokenData.ExtraData);
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