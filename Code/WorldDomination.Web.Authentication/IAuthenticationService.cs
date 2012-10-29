using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace WorldDomination.Web.Authentication
{
    public interface IAuthenticationService
    {
        IEnumerable<IAuthenticationProvider> Providers { get; }
        void AddProvider(IAuthenticationProvider authenticationProvider);
        Uri RedirectToAuthenticationProvider(string providerKey);
        Uri RedirectToAuthenticationProvider(IAuthenticationServiceSettings authenticationServiceSettings);
        IAuthenticatedClient CheckCallback(string providerKey, NameValueCollection requestParameters, string state);
    }
}