using System.Web.Routing;
using SimpleAuthentication.Mvc;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof (SimpleAuthenticationConfig), "RegisterRoutes")]

namespace SimpleAuthentication.Mvc
{
    public static class SimpleAuthenticationConfig
    {
        public static void RegisterRoutes()
        {
            SimpleAuthenticationRouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}