using Nancy;
using Nancy.Bootstrapper;
using Nancy.Session;
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
            var instagramProvider = new InstagramProvider(
                new ProviderParams("76310ae10207427f878c23001dc10d2a",
                    "b43c02181f754debac0cf4e38b59fd7f"));

            var authenticationProviderFactory = new AuthenticationProviderFactory(new ConfigService());

            authenticationProviderFactory.AddProvider(gitHubProvider);
            authenticationProviderFactory.AddProvider(instagramProvider);

            CookieBasedSessions.Enable(pipelines);
        }
    }
}