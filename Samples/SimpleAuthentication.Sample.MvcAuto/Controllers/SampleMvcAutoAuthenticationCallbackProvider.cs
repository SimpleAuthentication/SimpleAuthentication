using System.Web;
using System.Web.Mvc;
using SimpleAuthentication.Mvc;
using SimpleAuthentication.Sample.MvcAuto.Models;
using WorldDomination.Sample.MvcAuto.Models;

namespace SimpleAuthentication.Sample.MvcAuto.Controllers
{
    public class SampleMvcAutoAuthenticationCallbackProvider : IAuthenticationCallbackProvider
    {
        public ActionResult Process(HttpContextBase context, AuthenticateCallbackData model)
        {
            return new ViewResult
            {
                ViewName = "AuthenticateCallback",
                ViewData = new ViewDataDictionary(new AuthenticateCallbackViewModel
                {
                    AuthenticatedClient = model.AuthenticatedClient,
                    Exception = model.Exception
                })
            };
        }

        public ActionResult OnRedirectToAuthenticationProviderError(HttpContextBase context, string errorMessage)
        {
            return new ViewResult
            {
                ViewName = "AuthenticateCallback",
                ViewData = new ViewDataDictionary(new IndexViewModel
                {
                    ErrorMessage = errorMessage
                })
            };
        }
    }
}