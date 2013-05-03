using System;
using WorldDomination.Web.Authentication;

namespace WorldDomination.Sample.MvcManual.Models
{
    public class AuthenticateCallbackViewModel
    {
        public IAuthenticatedClient AuthenticatedClient { get; set; }
        public Exception Exception { get; set; }
        public Uri RedirectUrl { get; set; }
    }

    public class RedirectToProviderInputModel
    {
        public string ProviderKey { get; set; }
        public string Identifier { get; set; }
    }
}