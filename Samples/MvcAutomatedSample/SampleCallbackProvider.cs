using System.Web;
using System.Web.Mvc;
using WorldDomination.Web.Authentication.Mvc;

namespace MvcAutomatedSample
{
public class SampleCallbackProvider : IAuthenticationCallbackProvider
{
    public ActionResult Process(HttpContextBase nancyModule, AuthenticateCallbackData model)
    {
        return new ViewResult
        {
            ViewName = "AuthenticateCallback",
            ViewData = new ViewDataDictionary(model)
        };
    }
}
}