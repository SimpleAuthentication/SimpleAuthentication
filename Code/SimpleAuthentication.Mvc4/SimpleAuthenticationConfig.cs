using System.Web.Routing;
using SimpleAuthentication.Core;
using SimpleAuthentication.Mvc;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof (SimpleAuthenticationConfig), "RegisterRoutes")]

namespace SimpleAuthentication.Mvc
{
    public static class SimpleAuthenticationConfig
    {
        public static void RegisterRoutes()
        {
            // Do we have any custom routes defined?
            if (AuthenticationProviderFactory.Configuration.Value == null)
            {
                SimpleAuthenticationRouteConfig.RegisterDefaultRoutes(RouteTable.Routes);
            }
            else
            {
                SimpleAuthenticationRouteConfig.RegisterDefaultRoutes(RouteTable.Routes,
                                                                      AuthenticationProviderFactory.Configuration.Value
                                                                                                   .RedirectRoute,
                                                                      AuthenticationProviderFactory.Configuration.Value
                                                                                                   .CallBackRoute);
            }
        }
    }
}