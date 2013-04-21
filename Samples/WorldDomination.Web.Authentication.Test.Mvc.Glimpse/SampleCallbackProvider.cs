using System.Web;
using System.Web.Mvc;
using WorldDomination.Web.Authentication.Mvc;

namespace WorldDomination.Web.Authentication.Samples.Mvc.Glimpse
{
    public class SampleCallbackProvider : IAuthenticationCallbackProvider
    {
        public ActionResult Process(HttpContextBase context, AuthenticateCallbackData model)
        {
            return new ViewResult
            {
                ViewName = "AuthenticateCallback",
                ViewData = new ViewDataDictionary(model)
            };
        }
    }
}