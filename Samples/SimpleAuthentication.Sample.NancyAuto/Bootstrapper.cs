using Nancy;
using Nancy.TinyIoc;
using SimpleAuthentication.Core;

namespace SimpleAuthentication.Sample.NancyAuto
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);
        }
    }
}