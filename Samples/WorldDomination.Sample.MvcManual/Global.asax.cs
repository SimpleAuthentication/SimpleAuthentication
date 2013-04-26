using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WorldDomination.Sample.MvcManual.App_Start;

namespace WorldDomination.Sample.MvcManual
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