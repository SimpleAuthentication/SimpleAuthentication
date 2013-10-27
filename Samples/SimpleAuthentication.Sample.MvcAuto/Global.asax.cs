using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using SimpleAuthentication.Core;
using SimpleAuthentication.Mvc;
using SimpleAuthentication.Mvc.Caching;
using SimpleAuthentication.Sample.MvcAuto.App_Start;
using SimpleAuthentication.Sample.MvcAuto.Controllers;

namespace SimpleAuthentication.Sample.MvcAuto
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var builder = new ContainerBuilder();

            builder.RegisterType<SampleMvcAutoAuthenticationCallbackProvider>().As<IAuthenticationCallbackProvider>();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterControllers(typeof(SimpleAuthenticationController).Assembly);
            builder.RegisterType<CookieCache>().As<ICache>();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}