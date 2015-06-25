using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Providers;
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

            var providerWhiteList = new ProviderWhiteList
            {
                ProvidersToAllow = new List<Type>
                {
                    typeof (FacebookProvider),
                    typeof (GoogleProvider),
                    typeof (TwitterProvider),
                    typeof (WindowsLiveProvider),
                    typeof (ExtraProviders.GitHubProvider),
                    typeof (ExtraProviders.AmazonProvider),
                    typeof (ExtraProviders.InstagramProvider),
                    typeof (ExtraProviders.LinkedInProvider),
                    typeof (ExtraProviders.OpenIdProvider),
                    typeof (ExtraProviders.ThirtySevenSignalsProvider)
                }
            };
            builder.RegisterInstance<IProviderWhiteList>(providerWhiteList);

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}