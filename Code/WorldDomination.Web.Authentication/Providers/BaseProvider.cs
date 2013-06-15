using System;
using System.Diagnostics;
using WorldDomination.Web.Authentication.Tracing;

namespace WorldDomination.Web.Authentication.Providers
{
    public abstract class BaseProvider
    {
        protected BaseProvider()
        {
            TraceManager = new Lazy<ITraceManager>(() => new TraceManager()).Value;
        }

        public ITraceManager TraceManager { set; protected get; }
        protected abstract TraceSource TraceSource { get; }
    }
}