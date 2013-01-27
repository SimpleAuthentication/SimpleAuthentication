using System;
using System.Web.Mvc;
using WorldDomination.Web.Authentication.Facebook;
using WorldDomination.Web.Authentication.Google;
using WorldDomination.Web.Authentication.Samples.Mvc.Fakes.Models;
using WorldDomination.Web.Authentication.Twitter;

namespace WorldDomination.Web.Authentication.Samples.Mvc.Fakes.Controllers
{
    public class HomeController : Controller
    {
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
            var uri = _authenticationService.RedirectToAuthenticationProvider(providerKey);
            return Redirect(uri.AbsoluteUri);
        }

        public RedirectResult RedirectToAuthenticateWithError(string providerKey)
        {
            var uri = _authenticationServiceThatErrors.RedirectToAuthenticationProvider(providerKey);
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
                model.AuthenticatedClient = _authenticationService.GetAuthenticatedClient(providerKey, Request.Params);
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

            var model = new AuthenticateCallbackViewModel();
            try
            {
                model.AuthenticatedClient = _authenticationServiceThatErrors.GetAuthenticatedClient(providerKey,
                                                                                                    Request.Params);
            }
            catch (Exception exception)
            {
                model.Exception = exception;
            }

            return View("AuthenticateCallback", model);
        }
    }
}