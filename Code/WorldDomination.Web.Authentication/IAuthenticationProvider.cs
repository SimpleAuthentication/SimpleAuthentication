using System.Collections.Specialized;
using System.Web.Mvc;

namespace WorldDomination.Web.Authentication
{
    public interface IAuthenticationProvider
    {
        RedirectResult RedirectToAuthenticate(string state);
        IAuthenticatedClient AuthenticateClient(NameValueCollection parameters, string existingState);
    }
}