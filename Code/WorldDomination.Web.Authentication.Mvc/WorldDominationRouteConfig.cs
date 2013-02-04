using System.Web.Mvc;
using System.Web.Routing;

namespace WorldDomination.Web.Authentication.Mvc
{
    public class WorldDominationRouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                name: "WorldDominationAutomatedMvc-Redirect",
                url: "authentication/redirect/{providerkey}",
                defaults: new { controller = "WorldDominationAuthentication", action = "RedirectToProvider" }
            );
            routes.MapRoute(
                name: "WorldDominationAutomatedMvc-AuthenticateCallback",
                url: "authentication/authenticatecallback",
                defaults: new { controller = "WorldDominationAuthentication", action = "AuthenticateCallback" }
            );
        }
    }
}