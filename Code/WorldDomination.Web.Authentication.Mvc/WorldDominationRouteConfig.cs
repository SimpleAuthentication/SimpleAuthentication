using System.Web.Mvc;
using System.Web.Routing;

namespace WorldDomination.Web.Authentication.Mvc
{
    public static class WorldDominationRouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapOAuthRedirect("authentication/redirect");
            routes.MapOAuthCallback("authentication/authenticatecallback");
        }

        /// <summary>
        /// Maps the OAuth Redirect route (for redirecting out to a provider)
        /// </summary>
        /// <param name="routes">The <see cref="RouteCollection"/> to add mappings to</param>
        /// <param name="prefix">A prefix for the URL. "/{providerKey}" will be added to the end</param>
        public static void MapOAuthRedirect(this RouteCollection routes, string prefix)
        {
            routes.MapRoute(
                name: "WorldDominationAutomatedMvc-Redirect",
                url: prefix + "/{providerkey}",
                defaults: new { controller = "WorldDominationAuthentication", action = "RedirectToProvider" }
            );
        }

        /// <summary>
        /// Maps the OAuth Callback route (the URL the provider sends the user back to)
        /// </summary>
        /// <param name="routes">The <see cref="RouteCollection"/> to add mappings to</param>
        /// <param name="url">The URL to use</param>
        public static void MapOAuthCallback(this RouteCollection routes, string url)
        {
            routes.MapRoute(
                name: "WorldDominationAutomatedMvc-Callback",
                url: url,
                defaults: new { controller = "WorldDominationAuthentication", action = "AuthenticateCallback" }
            );
        }
    }
}