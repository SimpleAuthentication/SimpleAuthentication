using System;

namespace SimpleAuthentication.Core
{
    public class ConfigurationOptions : IConfigurationOptions
    {
        public Uri BasePath { get; set; }
    }
}