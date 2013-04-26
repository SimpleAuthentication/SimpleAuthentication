using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using WorldDomination.Sample.MvcAuto.App_Start;
using WorldDomination.Sample.MvcAuto.Controllers;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Csrf;
using WorldDomination.Web.Authentication.Mvc;

namespace WorldDomination.Sample.MvcAuto
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var builder = new ContainerBuilder();

            builder.RegisterType<AntiForgery>().As<IAntiForgery>();
            builder.RegisterType<SampleAuthenticationCallbackProvider>().As<IAuthenticationCallbackProvider>();
            builder.RegisterType<AuthenticationService>().As<IAuthenticationService>().SingleInstance();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterControllers(typeof(WorldDominationAuthenticationController).Assembly);

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}