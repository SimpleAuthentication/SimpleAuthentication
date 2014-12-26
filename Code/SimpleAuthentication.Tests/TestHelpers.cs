using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shouldly;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Config;
using SimpleAuthentication.Core.Providers;
using SimpleAuthentication.ExtraProviders;

namespace SimpleAuthentication.Tests
{
    public static class TestHelpers
    {
        private static IList<Provider> _providers;
        public const string ConfigProviderKey = "some ** key";
        public const string ConfigProviderSecret = "some secret";

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

        public static Provider InstagramProvider
        {
            get { return Providers.Single(x => x.Name == "instagram"); }
        }
        
        public static Provider GitHubProvider
        {
            get { return Providers.Single(x => x.Name == "github"); }
        }

        public static Configuration ConfigurationProviders
        {
            get
            {
                return new Configuration
                {
                    Providers = Providers
                };
            }
        }

        public static Configuration FilteredConfigurationProviders(string filterKey)
        {
            var configuration = ConfigurationProviders;
            configuration.Providers = configuration.Providers
                .Where(x => x.Name == filterKey)
                .ToList();

            return configuration;
        }

        public static IDictionary<string, IAuthenticationProvider> AuthenticationProviders
        {
            get
            {
                var providerParams = new ProviderParams(ConfigProviderKey, ConfigProviderSecret);
                return new Dictionary<string, IAuthenticationProvider>
                {
                    {
                        "google",
                        new GoogleProvider(providerParams)
                    },
                    {
                        "facebook",
                        new FacebookProvider(providerParams)
                    },
                    {
                        InstagramProvider.Name,
                        new InstagramProvider(providerParams)
                    },
                    {
                        GitHubProvider.Name,
                        new GitHubProvider(providerParams)
                    }
                };
            }
        }

        private static IList<Provider> Providers
        {
            get
            {
                return _providers ?? (_providers = new List<Provider>
                {
                    CreateProvider("google"),
                    CreateProvider("instagram"),
                    CreateProvider("github")
                });
            }
        }

        private static Provider CreateProvider(string name)
        {
            name.ShouldNotBeNullOrEmpty();

            return new Provider
            {
                Name = name,
                Key = ConfigProviderKey,
                Secret = ConfigProviderSecret
            };
        }
    }
}