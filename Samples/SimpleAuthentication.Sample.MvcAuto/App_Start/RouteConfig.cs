using System.Web.Mvc;
using System.Web.Routing;
using SimpleAuthentication.Mvc;

namespace SimpleAuthentication.Sample.MvcAuto.App_Start
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //SimpleAuthenticationRouteConfig.RegisterRoutes(routes);

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new {controller = "Home", action = "Index", id = UrlParameter.Optional}
                );
        }
    }
}