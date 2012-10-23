using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Session;
using TinyIoC;

namespace WorldDomination.Web.IntegrationTest.NancyFX
{
    public class BootStrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            CookieBasedSessions.Enable(pipelines);
        }
    }
}