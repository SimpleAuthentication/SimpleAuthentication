namespace SimpleAuthentication.Core.Config
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    public static class AppSettingsParser
    {
        private const string KeyPrefix = "sa.";

        public static IEnumerable<ProviderKey> ParseAppSettings(this NameValueCollection settings)
        {
            for (int i = 0; i < settings.Count; i++)
            {
                var key = settings.GetKey(i).ToLower();

                //If it starts with the prefix then we can only assume its ours...
                if (key.StartsWith(KeyPrefix))
                {
                    var provider = key.Replace(KeyPrefix, string.Empty);
                    var value = settings[i];
                    var values = value.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(x =>
                                      {
                                          var split = x.Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries);

                                          return new
                                          {
                                              Key = split[0].ToLower(),
                                              Value = split[1]
                                          };
                                      }).ToList();

                    yield return new ProviderKey
                    {
                        ProviderName = provider,
                        Secret = values.GetValue("secret"),
                        Key = values.GetValue("key"),
                        Scope = values.GetValue("scopes")
                    };
                }
            }
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

        public class ProviderKey
        {
            public string ProviderName { get; set; }
            public string Secret { get; set; }
            public string Key { get; set; }
            public string Scope { get; set; }
        }
    }
}