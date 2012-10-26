using System;
using System.Collections.Specialized;

namespace WorldDomination.Web.Authentication
{
    public interface IAuthenticationProvider
    {
        string Name { get; }
        Uri RedirectToAuthenticate(string state, params string[] optionalParameters);
        IAuthenticatedClient AuthenticateClient(NameValueCollection parameters, string existingState);
    }
}