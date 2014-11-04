using Nancy;
using SimpleAuthentication.Core.Tracing;

namespace SimpleAuthentication.Sample.Nancy.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            var traceManager = new TraceManager();
            var traceSource = traceManager["SimpleAuthentication.Sample.Nancy.Modules"];
            traceSource.TraceVerbose("Hi There! Lets test this out :)");

            Get["/"] = _ => View["index"];
        }
    }
}