using StructureMap.Configuration.DSL;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Csrf;

namespace MvcAutomatedSample.DependencyResolution
{
    public class AuthenticationRegistry : Registry
    {
        public AuthenticationRegistry(IAuthenticationProvider facebookProvider,
                                      IAuthenticationProvider googleProvider,
                                      IAuthenticationProvider twitterProvider)
        {
            var authenticationService = new AuthenticationService();

            if (facebookProvider != null)
            {
                authenticationService.AddProvider(facebookProvider);
            }

            if (googleProvider != null)
            {
                authenticationService.AddProvider(googleProvider);
            }

            if (twitterProvider != null)
            {
                authenticationService.AddProvider(twitterProvider);
            }

            For<IAuthenticationService>()
                .Use(authenticationService);

            For<IAntiForgery>()
                .Use<AntiForgery>();
        }
    }
}