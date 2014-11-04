using System.Collections.Generic;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Config;
using SimpleAuthentication.Core.Providers;

namespace SimpleAuthentication.Tests
{
    public static class TestHelpers
    {
        public static Configuration ConfigurationWithGoogleProvider
        {
            get
            {
                var providers = new[]
                {
                    new Provider
                    {
                        Name = "Google",
                        Key = "some ** key",
                        Secret = "some secret"
                    }
                };

                return new Configuration
                {
                    Providers = providers
                };
            }
        }

        public static IDictionary<string, IAuthenticationProvider> AuthenticationProvidersWithGoogle
        {
            get
            {
                return new Dictionary<string, IAuthenticationProvider>
                {
                    {"google", new GoogleProvider(new ProviderParams("some ** key", "some secret"))}
                };
            }
        }
    }
}