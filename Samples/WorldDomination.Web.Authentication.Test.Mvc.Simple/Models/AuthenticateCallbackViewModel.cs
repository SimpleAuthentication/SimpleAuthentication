using System;

namespace WorldDomination.Web.Authentication.Samples.Mvc.Simple.Models
{
    public class AuthenticateCallbackViewModel
    {
        public IAuthenticatedClient AuthenticatedClient { get; set; }
        public Exception Exception { get; set; }
    }
}