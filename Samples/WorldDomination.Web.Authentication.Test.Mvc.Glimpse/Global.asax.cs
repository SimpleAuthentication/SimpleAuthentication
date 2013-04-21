using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using WorldDomination.Web.Authentication.Extensions.Glimpse;
using WorldDomination.Web.Authentication.Mvc;
using WorldDomination.Web.Authentication.Samples.Mvc.Glimpse.App_Start;

namespace WorldDomination.Web.Authentication.Samples.Mvc.Glimpse {

    public class MvcApplication : System.Web.HttpApplication {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WorldDominationRouteConfig.RegisterRoutes(RouteTable.Routes);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var builder = new ContainerBuilder();
            builder.RegisterType<SampleCallbackProvider>().As<IAuthenticationCallbackProvider>();
            builder.Register<ILoggingService>(x => new GlimpseLogger()).SingleInstance();
            builder.Register<IAuthenticationService>(x => new AuthenticationService());
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterControllers(typeof(WorldDominationAuthenticationController).Assembly);
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}