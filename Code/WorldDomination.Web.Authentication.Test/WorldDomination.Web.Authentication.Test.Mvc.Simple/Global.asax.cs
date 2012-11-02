using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WorldDomination.Web.Authentication.Test.Mvc.Simple.App_Start;

namespace WorldDomination.Web.Authentication.Test.Mvc.Simple
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}