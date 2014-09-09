using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Nancy;
using Nancy.Session;
using Nancy.Testing;

namespace SimpleAuthentication.Tests.WebSites.Nancy
{
    public class NancyModuleTestBase<TModule> where TModule : NancyModule
    {
        protected Browser Browser(object dependency)
        {
            return Browser(dependency != null
                ? new[] {dependency}
                : null);
        }

        protected Browser Browser(IEnumerable<object> dependencies = null,
            IDictionary<string, object> session = null)
        {
            var bootstrapper = new ConfigurableBootstrapper(with =>
            {
                // The nancy module we're testing against.
                with.Module<TModule>();

                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

                if (MappedDependencies != null)
                {
                    with.MappedDependencies(MappedDependencies);
                }

                if (dependencies != null)
                {
                    with.Dependencies(dependencies);
                }
            });

            if (session != null)
            {
                bootstrapper.BeforeRequest.AddItemToEndOfPipeline(ctx =>
                {
                    ctx.Request.Session = new Session(session);
                    return null;
                });
            }

            return new Browser(bootstrapper);
        }

        protected IEnumerable<Tuple<Type, object>> MappedDependencies { get; set; }
    }
}