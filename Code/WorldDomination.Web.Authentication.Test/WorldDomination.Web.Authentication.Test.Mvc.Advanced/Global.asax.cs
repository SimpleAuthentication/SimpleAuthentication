using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WorldDomination.Web.Authentication.Test.Mvc.Advanced.App_Start;

namespace WorldDomination.Web.Authentication.Test.Mvc.Advanced
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