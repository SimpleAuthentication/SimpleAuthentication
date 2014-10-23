using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SimpleAuthentication.Mvc;

[assembly: PreApplicationStartMethod(typeof(RouteConfig), "RegisterRoutes")]

namespace SimpleAuthentication.Mvc
{
    public static class RouteConfig
    {
        public const string DefaultRedirectRoute = "authenticate/{providerKey}";
        public const string DefaultCallbackRoute = "authenticate/callback";
        public const string RedirectToProviderRouteName = "SimpleAuthentication.Mvc-Redirect";
        public const string CallbackRouteName = "SimpleAuthentication.Mvc-Callback";

        public static void RegisterRoutes()
        {
            RegisterDefaultRoutes(RouteTable.Routes);
        }

        public static void RegisterDefaultRoutes(RouteCollection routes,
            string redirectRoute = DefaultRedirectRoute,
            string callbackRoute = DefaultCallbackRoute)
        {
            routes.MapOAuthRedirect(string.IsNullOrWhiteSpace(redirectRoute)
                ? DefaultRedirectRoute
                : redirectRoute);

            routes.MapOAuthCallback(string.IsNullOrWhiteSpace(callbackRoute)
                ? DefaultCallbackRoute
                : callbackRoute);
        }

        /// <summary>
        /// Maps the OAuth Redirect route (for redirecting out to a provider).
        /// </summary>
        /// <param name="routes">The <see cref="RouteCollection"/> to add mappings to.</param>
        /// <param name="prefix">A prefix for the URL. "/{providerName}" will be added to the end.</param>
        public static void MapOAuthRedirect(this RouteCollection routes, string route)
        {
            routes.MapRoute(
                name: RedirectToProviderRouteName,
                url: route,
                defaults: new {controller = "SimpleAuthentication", action = "RedirectToProvider"}
                );
        }

        /// <summary>
        /// Maps the OAuth Callback route (the URL the provider sends the user back to).
        /// </summary>
        /// <param name="routes">The <see cref="RouteCollection"/> to add mappings to.</param>
        /// <param name="route">The route endpoint on your server that will receive the response from Authentication Provider.</param>
        public static void MapOAuthCallback(this RouteCollection routes, string route)
        {
            routes.MapRoute(
                name: CallbackRouteName,
                url: route,
                defaults: new {controller = "SimpleAuthentication", action = "AuthenticateCallback"}
                );
        }
    }
}