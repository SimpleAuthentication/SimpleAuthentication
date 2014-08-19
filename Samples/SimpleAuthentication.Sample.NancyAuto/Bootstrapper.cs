using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Config;
using SimpleAuthentication.ExtraProviders;

namespace SimpleAuthentication.Sample.NancyAuto
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            var gitHubProvider = new GitHubProvider(new ProviderParams("9403c7920a82689969d1",
                "e5b3807c7c97466634bdf21ddf9a179485f1fe60"));
            var authenticationProviderFactory = new AuthenticationProviderFactory(new ConfigService());

            authenticationProviderFactory.AddProvider(gitHubProvider);
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);
        }
    }
}