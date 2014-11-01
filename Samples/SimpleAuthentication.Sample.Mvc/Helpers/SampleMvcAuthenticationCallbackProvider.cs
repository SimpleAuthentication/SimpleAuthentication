using System;
using System.Web.Mvc;
using SimpleAuthentication.Core;
using SimpleAuthentication.Mvc;
using SimpleAuthentication.Sample.Mvc.Models;

namespace SimpleAuthentication.Sample.Mvc.Helpers
{
    public class SampleMvcAuthenticationCallbackProvider : IAuthenticationProviderCallback
    {
        public ActionResult Process(Controller controller, AuthenticateCallbackResult result)
        {
            return new ViewResult
            {
                ViewName = "AuthenticateCallback",
                ViewData = new ViewDataDictionary(new AuthenticateCallbackViewModel
                {
                    AuthenticatedClient = result.AuthenticatedClient,
                    ReturnUrl = result.ReturnUrl
                })
            };
        }

        public ActionResult OnRedirectToAuthenticationProviderError(Controller controller, Exception exception)
        {
            return new ViewResult
            {
                ViewName = "AuthenticateCallback",
                ViewData = new ViewDataDictionary(new IndexViewModel
                {
                    ErrorMessage = exception.Message
                })
            };
        }
    }
}