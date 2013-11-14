namespace SimpleAuthentication.Core.Config
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    public static class AppSettingsParser
    {
        private const string KeyPrefix = "sa.";
        private const string RedirectRouteKey = "redirectroute";
        private const string CallbackRouteKey = "callbackroute";

        public static Configuration ParseAppSettings(this NameValueCollection settings)
        {
            var configuration = new Configuration();

            for (int i = 0; i < settings.Count; i++)
            {
                var key = settings.GetKey(i).ToLower();

                // Example key: <app key="sa.github" value="secret:aaa;key:bbb;scopes:ccc..."/>

                // If it doesn't start with our specific prefix then we ignore it.
                if (!key.StartsWith(KeyPrefix))
                {
                    continue;
                }

                var provider = key.Replace(KeyPrefix.ToLowerInvariant(), string.Empty);
                var value = settings[i];

                // First we need to check for our specific Keys. Otherwise, we assume it's a provider key.
                switch (provider)
                {
                    case RedirectRouteKey:
                        configuration.RedirectRoute = value;
                        break;
                    case CallbackRouteKey:
                        configuration.CallBackRoute = value;
                        break;
                    default:
                        // Fallback - we're assuming the key is a provider...
                        if (configuration.Providers == null)
                        {
                            configuration.Providers = new List<Provider>();
                        }

                        configuration.Providers.Add(ParseValueForProviderData(provider, value));
                        break;
                }
            }

            return configuration;
        }

        private static string GetValue(this IEnumerable<dynamic> values, string key)
        {
            var value = values.FirstOrDefault(x => (string) x.Key == key);

            if (value == null)
            {
                return string.Empty;
            }

            return value.Value;
        }

        private static Provider ParseValueForProviderData(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(key);
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            
            var values = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(x =>
                              {
                                  var split = x.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                                  return new
                                  {
                                      Key = split[0].ToLower(),
                                      Value = split[1]
                                  };
                              }).ToList();

            return new Provider
            {
                Name = key,
                Secret = values.GetValue("secret"),
                Key = values.GetValue("key"),
                Scopes = values.GetValue("scopes")
            };
        }
    }
}