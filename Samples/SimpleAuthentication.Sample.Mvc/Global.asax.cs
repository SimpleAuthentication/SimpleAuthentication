using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Config;
using SimpleAuthentication.Mvc;
using SimpleAuthentication.Sample.Mvc.Helpers;

namespace SimpleAuthentication.Sample.Mvc
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // Register Simple Authentication stuff.
            builder.RegisterType<SampleMvcAutoAuthenticationCallbackProvider>().As<IAuthenticationProviderCallback>();
            builder.RegisterType<ConfigService>().As<IConfigService>();
            builder.RegisterControllers(typeof(SimpleAuthenticationController).Assembly);

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}