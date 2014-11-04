using System.Collections.Generic;

namespace SimpleAuthentication.Core
{
    public interface IAuthenticationProviderFactory
    {
        IDictionary<string, IAuthenticationProvider> AuthenticationProviders { get; }
    }
}