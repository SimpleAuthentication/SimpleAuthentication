using System.Web.Mvc;
using System.Web.Routing;
using WorldDomination.Web.Authentication.Mvc;

namespace WorldDomination.Sample.MvcAuto.App_Start
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            WorldDominationRouteConfig.RegisterRoutes(routes);

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new {controller = "Home", action = "Index", id = UrlParameter.Optional}
                );
        }
    }
}