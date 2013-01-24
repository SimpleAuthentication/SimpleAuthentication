using System;
using System.Web;
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
            var settings = _authenticationService.GetAuthenticateServiceSettings(providerKey);

            // Provide the callBack instead of using the config file entry (for the use of this demo).
            settings.CallBackUri = new Uri(ToAbsoluteUrl(Url.Action("AuthenticateCallback", new {providerKey})));

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
                CallBackUri =
                    new Uri(
                                                                                  ToAbsoluteUrl(
                                                                                      Url.Action(
                                                                                          "AuthenticateCallback",
                                                                                          new {providerKey = "facebook"}))),
                State =
                    Session[SessionStateKey].ToString(),
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
                // It's possible that a person might hit this resource directly, before any session value
                // has been set. As such, we should just fake some state up, which will not match the
                // CSRF check.
                var state = (Guid) (Session[SessionStateKey] ?? Guid.NewGuid());

                // Complete the authentication process by retrieving the UserInformation from the provider.
                model.AuthenticatedClient = _authenticationService.GetAuthenticatedClient(providerKey, Request.Params,
                                                                                          state.ToString());

                // Clean up after ourselves like a nice little boy/girl/monster we are.
                Session.Remove(SessionStateKey);
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