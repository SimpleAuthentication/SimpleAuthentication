//
// Copied from the SignalR code: https://github.com/SignalR/SignalR/blob/master/src/Microsoft.AspNet.SignalR.Core/Tracing/TraceSourceExtensions.cs
// SignalR license (Apache License, Version 2.0): https://github.com/SignalR/SignalR/blob/master/LICENSE.md   
//

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System.Diagnostics;

namespace SimpleAuthentication.Core.Tracing
{
    public static class TraceSourceExtensions
    {
        public static void TraceVerbose(this TraceSource traceSource, string message)
        {
            Trace(traceSource, TraceEventType.Verbose, message);
        }

        public static void TraceVerbose(this TraceSource traceSource, string format, params object[] args)
        {
            Trace(traceSource, TraceEventType.Verbose, format, args);
        }

        public static void TraceWarning(this TraceSource traceSource, string message)
        {
            Trace(traceSource, TraceEventType.Warning, message);
        }

        public static void TraceWarning(this TraceSource traceSource, string format, params object[] args)
        {
            Trace(traceSource, TraceEventType.Warning, format, args);
        }

        public static void TraceError(this TraceSource traceSource, string message)
        {
            Trace(traceSource, TraceEventType.Error, message);
        }

        public static void TraceError(this TraceSource traceSource, string format, params object[] args)
        {
            Trace(traceSource, TraceEventType.Error, format, args);
        }

        private static void Trace(TraceSource traceSource, TraceEventType eventType, string message)
        {
            traceSource.TraceEvent(eventType, 0, message);
        }

        private static void Trace(TraceSource traceSource, TraceEventType eventType, string format, params object[] args)
        {
            traceSource.TraceEvent(eventType, 0, format, args);
        }
    }
}
