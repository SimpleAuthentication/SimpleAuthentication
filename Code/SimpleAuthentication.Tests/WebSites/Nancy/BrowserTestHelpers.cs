using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Security;
using Nancy.Testing;

namespace SimpleAuthentication.Tests.WebSites.Nancy
{
    public static class BrowserTestHelpers
    {
        public static Browser Browser<TModule>(object dependency) where TModule : NancyModule
        {
            return Browser<TModule>(new[] { dependency });
        }

        public static Browser Browser<TModule>(IEnumerable<object> dependencies,
            INancyBootstrapper bootstrapper = null,
            IEnumerable<KeyValuePair<Type, object>> mappedDependencies = null,
            IUserIdentity userIdentity = null,
            IList<BeforePipeline> beforeRequests = null,
            IEnumerable<Type> statusCodeHandlers = null,
            IRootPathProvider rootPathProvider = null,
            IEnumerable<IDictionary<string, object>> sessions = null) where TModule : NancyModule
        {
            return new Browser(with =>
            {
                // The nancy module we're testing against.
                with.Module<TModule>();

                // Any sessions?
                if (sessions != null)
                {
                    with.Sessions(sessions);
                }

                // Make sure everything is in invariant. 
                // eg. We don't want some comma's in any european POST's for large numbers.
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

                // Now we can wire dependencies.
                foreach (var dependency in dependencies)
                {
                    with.Dependency(dependency);
                }

                if (mappedDependencies != null)
                {
                    with.MappedDependencies(mappedDependencies.Select(md => Tuple.Create(md.Key, md.Value)));
                }

                // Wire up any custom status code handlers. For example, all 500 Internal Server Error
                // errors might return the error result as json.
                if (statusCodeHandlers != null)
                {
                    with.StatusCodeHandlers(statusCodeHandlers.ToArray());
                }

                // Do we have a Views folder we need to tell the test about?
                if (rootPathProvider != null)
                {
                    with.RootPathProvider(rootPathProvider);
                }
            });
        }
    }
}