using System;
using System.Diagnostics;
using WorldDomination.Web.Authentication.Tracing;

namespace WorldDomination.Web.Authentication.Providers
{
    public abstract class BaseProvider
    {
        protected BaseProvider()
        {
            RestClientFactory = new RestClientFactory();
            TraceManager = new Lazy<TraceManager>(() => new TraceManager()).Value;
        }

        public IRestClientFactory RestClientFactory { get; set; }
        public ITraceManager TraceManager { set; protected get; }
        protected abstract TraceSource TraceSource { get; }
    }
}