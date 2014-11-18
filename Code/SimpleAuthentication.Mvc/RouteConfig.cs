using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SimpleAuthentication.Mvc;

[assembly: PreApplicationStartMethod(typeof (RouteConfig), "RegisterRoutes")]

namespace SimpleAuthentication.Mvc
{
    public static class RouteConfig
    {
        public const string RedirectToProviderRouteName = "SimpleAuthentication.Mvc-Redirect";
        public const string CallbackRouteName = "SimpleAuthentication.Mvc-Callback";
        public const string UserInformationRouteName = "SimpleAuthentication.Mvc-UserInformation";

        public static void RegisterRoutes()
        {
            RegisterRoutes(SimpleAuthenticationController.DefaultRedirectRoute,
                SimpleAuthenticationController.DefaultCallbackRoute,
                SimpleAuthenticationController.DefaultUserInformationRoute);
        }

        public static void RegisterRoutes(string redirectRoute,
            string callbackRoute,
            string userInformationRoute)
        {
            // If someone provides a null or empty route parameter, then we assuming
            // they want the default routes.

            // NOTE: This needs to be registered before the other one, else this will never get caught.
            RouteTable.Routes.MapOAuthCallback(string.IsNullOrWhiteSpace(callbackRoute)
                ? SimpleAuthenticationController.DefaultCallbackRoute
                : callbackRoute);

            RouteTable.Routes.MapOAuthUserInformation(string.IsNullOrWhiteSpace(userInformationRoute)
                ? SimpleAuthenticationController.DefaultUserInformationRoute
                : userInformationRoute);

            RouteTable.Routes.MapOAuthRedirect(string.IsNullOrWhiteSpace(redirectRoute)
                ? SimpleAuthenticationController.DefaultRedirectRoute
                : redirectRoute);
        }

        /// <summary>
        /// Maps the OAuth Redirect route (for redirecting out to a provider).
        /// </summary>
        /// <param name="routes">The <see cref="RouteCollection"/> to add mappings to.</param>
        /// <param name="route">A prefix for the URL. "/{providerName}" will be added to the end.</param>
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
                defaults: new { controller = "SimpleAuthentication", action = "AuthenticateCallbackAsync" }
                );
        }

        /// <summary>
        /// Maps the OAuth UserInformation route - data that calls 'me'/'you'.
        /// </summary>
        /// <param name="routes">The <see cref="RouteCollection"/> to add mappings to.</param>
        /// <param name="route">A prefix for the URL. "/{providerName}" will be added to the end.</param>
        public static void MapOAuthUserInformation(this RouteCollection routes, string route)
        {
            routes.MapRoute(
                name: UserInformationRouteName,
                url: route,
                defaults: new { controller = "SimpleAuthentication", action = "AuthenticateMeAsync" }
                );
        }
    }
}