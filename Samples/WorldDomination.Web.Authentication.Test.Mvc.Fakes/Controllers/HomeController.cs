using System;
using System.Web;
using System.Web.Mvc;
using WorldDomination.Web.Authentication.Providers.Facebook;
using WorldDomination.Web.Authentication.Providers.Google;
using WorldDomination.Web.Authentication.Providers.Twitter;
using WorldDomination.Web.Authentication.Samples.Mvc.Fakes.Models;

namespace WorldDomination.Web.Authentication.Samples.Mvc.Fakes.Controllers
{
    public class HomeController : Controller
    {
        private const string StateKey = "WorldDomination-StateKey-cf92a651-d638-4ce4-a393-f612d3be4c3a";

        private readonly AuthenticationService _authenticationService;

        private readonly AuthenticationService _authenticationServiceThatErrors;

        public HomeController()
        {
            var facebookProvider =
                new FakeFacebookProvider(new Uri("http://localhost:1338/home/AuthenticateCallback?providerKey=facebook"));
            var twitterProvider =
                new FakeTwitterProvider(new Uri("http://localhost:1338/home/AuthenticateCallback?providerKey=twitter"));
            var googleProvider =
                new FakeGoogleProvider(new Uri("http://localhost:1338/home/AuthenticateCallback?providerKey=google"));

            _authenticationService = new AuthenticationService();
            _authenticationService.AddProvider(facebookProvider);
            _authenticationService.AddProvider(twitterProvider);
            _authenticationService.AddProvider(googleProvider);

            // Some providers that error.
            var facebookProviderThatErrors =
                new FakeFacebookProvider(
                    new Uri("http://localhost:1338/home/AuthenticateCallbackThatErrors?providerKey=facebook"))
                {
                    AuthenticateClientExceptionMessage =
                        "ZOMG! Something nasty has occured! ID10T Error!1!1!1. -le sad panda-"
                };
            var twitterProviderThatErrors =
                new FakeTwitterProvider(
                    new Uri("http://localhost:1338/home/AuthenticateCallbackThatErrors?providerKey=twitter"))
                {
                    AuthenticateClientExceptionMessage =
                        "ZOMG! Something nasty has occured! ID10T Error!1!1!1. -le sad panda-"
                };
            var googleProviderThatErrors =
                new FakeGoogleProvider(
                    new Uri("http://localhost:1338/home/AuthenticateCallbackThatErrors?providerKey=google"))
                {
                    AuthenticateClientExceptionMessage =
                        "ZOMG! Something nasty has occured! ID10T Error!1!1!1. -le sad panda-"
                };

            _authenticationServiceThatErrors = new AuthenticationService();
            _authenticationServiceThatErrors.AddProvider(facebookProviderThatErrors);
            _authenticationServiceThatErrors.AddProvider(twitterProviderThatErrors);
            _authenticationServiceThatErrors.AddProvider(googleProviderThatErrors);
        }

        public ActionResult Index()
        {
            return View();
        }

        public RedirectResult RedirectToAuthenticate(string providerKey)
        {
            // Grab the required Provider settings.
            var settings = _authenticationService.GetAuthenticateServiceSettings(providerKey, Request.Url);

            // Remember the State value (for CSRF protection).
            Session[StateKey] = settings.State;

            // Determine the provider's end point Url we need to redirect to.
            var uri = _authenticationService.RedirectToAuthenticationProvider(settings);

            // Kthxgo!
            return Redirect(uri.AbsoluteUri);
        }

        public RedirectResult RedirectToAuthenticateWithError(string providerKey)
        {
            // Grab the required Provider settings.
            var settings = _authenticationServiceThatErrors.GetAuthenticateServiceSettings(providerKey, Request.Url);

            // Remember the State value (for CSRF protection).
            Session[StateKey] = settings.State;

            // Determine the provider's end point Url we need to redirect to.
            var uri = _authenticationServiceThatErrors.RedirectToAuthenticationProvider(settings);

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
            var settings = _authenticationService.GetAuthenticateServiceSettings(providerKey, Request.Url);

            // Make sure we use our 'previous' State value.
            settings.State = (Session[StateKey] as string) ?? string.Empty;

            var model = new AuthenticateCallbackViewModel();

            try
            {
                // Grab the authenticated client information.
                model.AuthenticatedClient = _authenticationService.GetAuthenticatedClient(settings, Request.QueryString);
                Session.Remove(StateKey);
            }
            catch (Exception exception)
            {
                model.Exception = exception;
            }

            return View(model);
        }

        public ActionResult AuthenticateCallbackThatErrors(string providerKey)
        {
            if (string.IsNullOrEmpty(providerKey))
            {
                throw new ArgumentNullException("providerKey");
            }

            // Determine which settings we need, based on the Provider.
            var settings = _authenticationServiceThatErrors.GetAuthenticateServiceSettings(providerKey, Request.Url);

            // Make sure we use our 'previous' State value.
            settings.State = (Session[StateKey] as string) ?? string.Empty;

            var model = new AuthenticateCallbackViewModel();

            try
            {
                // Grab the authenticated client information.
                model.AuthenticatedClient = _authenticationServiceThatErrors.GetAuthenticatedClient(settings, Request.QueryString);
                Session.Remove(StateKey);
            }
            catch (Exception exception)
            {
                model.Exception = exception;
            }

            return View("AuthenticateCallback", model);
        }
    }
}