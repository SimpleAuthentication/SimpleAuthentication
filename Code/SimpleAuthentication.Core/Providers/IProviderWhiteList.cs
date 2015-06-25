using System;
using System.Collections.Generic;

namespace SimpleAuthentication.Core.Providers
{
    public interface IProviderWhiteList
    {
        ICollection<Type> DefaultProviders { get; }
        ICollection<Type> ProvidersToAllow { get; set; }
    }
}