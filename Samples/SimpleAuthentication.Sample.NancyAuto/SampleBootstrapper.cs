
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using Nancy.SimpleAuthentication.Caching;
using SimpleAuthentication.Core;

namespace SimpleAuthentication.Sample.NancyAuto
{
    public class SampleBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            pipelines.BeforeRequest += (ctx) =>
            {
                CookieCache cache = new CookieCache(ctx);
                //or if you want the session cache, use
                //ICache sessionCache = new SessionCache(ctx.Request.Session);
                return null;
            };
        }

    }
}