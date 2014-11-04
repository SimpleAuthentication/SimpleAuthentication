using System;
using System.Collections.Generic;

namespace SimpleAuthentication.Core.Providers
{
    public interface IProviderScanner
    {
        IList<Type> GetDiscoveredProviders();
    }
}