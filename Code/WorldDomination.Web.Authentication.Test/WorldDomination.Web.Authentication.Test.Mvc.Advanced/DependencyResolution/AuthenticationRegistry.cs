using StructureMap.Configuration.DSL;

namespace WorldDomination.Web.Authentication.Test.Mvc.Advanced.DependencyResolution
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
                .Use(authenticationService)
                .Named("Authentication Service.");
        }
    }
}