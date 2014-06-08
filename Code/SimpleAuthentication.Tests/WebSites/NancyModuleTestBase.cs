using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Nancy;
using Nancy.Testing;

namespace SimpleAuthentication.Tests.WebSites
{
    public class NancyModuleTestBase<TModule> where TModule : NancyModule
    {
        protected Browser Browser(object dependency)
        {
            return Browser(dependency != null
                ? new[] {dependency}
                : null);
        }

        protected Browser Browser(IEnumerable<object> dependencies = null)
        {
            return new Browser(with =>
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
        }

        protected IEnumerable<Tuple<Type, object>> MappedDependencies { get; set; }
    }
}