using System;
using System.Collections.Generic;
using System.Text;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Config;
using SimpleAuthentication.Core.Providers;

namespace SimpleAuthentication.Tests
{
    public static class TestHelpers
    {
        public static string ToEncodedString(this List<KeyValuePair<string, string>> value)
        {
            var result = new StringBuilder();
            foreach (var keyValuePair in value)
            {
                if (result.Length > 0)
                {
                    result.Append("&");
                }

                result.AppendFormat("{0}={1}", 
                    Uri.EscapeDataString(keyValuePair.Key), 
                    Uri.EscapeDataString(keyValuePair.Value));
            }

            return result.ToString();
        }

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