using System.Web;
using System.Web.Mvc;
using WorldDomination.Sample.MvcAuto.Models;
using WorldDomination.Web.Authentication.Mvc;

namespace WorldDomination.Sample.MvcAuto.Controllers
{
    public class SampleAuthenticationCallbackProvider : IAuthenticationCallbackProvider
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
    }
}