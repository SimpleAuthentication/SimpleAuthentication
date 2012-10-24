using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WorldDomination.Web.IntegrationTest.Mvc.App_Start;

namespace WorldDomination.Web.IntegrationTest.Mvc
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