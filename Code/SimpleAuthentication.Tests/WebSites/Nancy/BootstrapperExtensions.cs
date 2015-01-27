using System.Collections.Generic;
using Nancy.Session;
using Nancy.Testing;

namespace SimpleAuthentication.Tests.WebSites.Nancy
{
    public static class BootstrapperExtensions
    {
        public static void Sessions(
            this ConfigurableBootstrapper.ConfigurableBootstrapperConfigurator configurableBootstrapperConfigurator,
            IEnumerable<IDictionary<string, object>> sessions)
        {
            configurableBootstrapperConfigurator.RequestStartup(
                (containter, pipelines, context) => pipelines.BeforeRequest.AddItemToEndOfPipeline(ctx =>
                {
                    foreach (var session in sessions)
                    {
                        ctx.Request.Session = new Session(session);
                    }
                    return null;
                }));
        }

        public static void Session(
            this ConfigurableBootstrapper.ConfigurableBootstrapperConfigurator configurableBootstrapperConfigurator,
            IDictionary<string, object> session)
        {
            Sessions(configurableBootstrapperConfigurator, new[] { session });
        }

        public static void Session(
            this ConfigurableBootstrapper.ConfigurableBootstrapperConfigurator configurableBootstrapperConfigurator,
            string key, string value)
        {
            Session(configurableBootstrapperConfigurator, new Dictionary<string, object> { { key, value } });
        }
    }
}