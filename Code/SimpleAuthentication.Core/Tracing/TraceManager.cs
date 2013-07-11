//
// Based upon the SignalR code: https://github.com/SignalR/SignalR/blob/master/src/Microsoft.AspNet.SignalR.Core/Tracing/TraceManager.cs
//

using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SimpleAuthentication.Core.Tracing
{
    public class TraceManager : ITraceManager
    {
        private readonly ConcurrentDictionary<string, TraceSource> _sources =
            new ConcurrentDictionary<string, TraceSource>(StringComparer.OrdinalIgnoreCase);

        public TraceSource this[string name]
        {
            get { return _sources.GetOrAdd(name, key => new TraceSource(key)); }
        }
    }
}