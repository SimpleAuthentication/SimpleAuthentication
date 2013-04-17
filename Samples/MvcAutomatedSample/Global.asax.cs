using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using MvcAutomatedSample.App_Start;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Config;
using WorldDomination.Web.Authentication.Mvc;

namespace MvcAutomatedSample
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WorldDominationRouteConfig.RegisterRoutes(RouteTable.Routes);
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var builder = new ContainerBuilder();
            builder.RegisterType<SampleCallbackProvider>().As<IAuthenticationCallbackProvider>();
            //builder.RegisterType<AuthenticationService>().As<IAuthenticationService>().SingleInstance();
            builder.Register<IAuthenticationService>(x => new AuthenticationService());
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterControllers(typeof(WorldDominationAuthenticationController).Assembly);
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

        }
    }
}