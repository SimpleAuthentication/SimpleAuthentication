using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace WorldDomination.Web.Authentication.Mvc
{
    public class WorldDominationAuthenticationController : Controller
    {
        public RedirectResult RedirectToProvider(string providerkey, string additionaldata = null)
        {
            return Redirect("");
        }

        //authentication/redirect
        public ActionResult AuthenticateCallback(string providerkey)
        {
            return Content("Stuffz");
        }
    }
}
