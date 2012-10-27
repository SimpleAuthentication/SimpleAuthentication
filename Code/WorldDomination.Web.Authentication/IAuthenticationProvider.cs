using System;
using System.Collections.Specialized;

namespace WorldDomination.Web.Authentication
{
    public interface IAuthenticationProvider
    {
        string Name { get; }
        Uri RedirectToAuthenticate(IAuthenticationServiceSettings authenticationServiceSettings);
        IAuthenticatedClient AuthenticateClient(NameValueCollection parameters, string existingState);
        IAuthenticationServiceSettings DefaultAuthenticationServiceSettings { get; }
    }
}