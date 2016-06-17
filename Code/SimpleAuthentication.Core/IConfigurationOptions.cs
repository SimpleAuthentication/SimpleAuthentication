using System;

namespace SimpleAuthentication.Core
{
    public interface IConfigurationOptions
    {
        Uri BasePath { get; set; }
    }
}