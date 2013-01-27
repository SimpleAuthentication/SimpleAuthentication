using System;

namespace WorldDomination.Web.Authentication.Samples.Mvc.Fakes.Models
{
    public class AuthenticateCallbackViewModel
    {
        public IAuthenticatedClient AuthenticatedClient { get; set; }
        public Exception Exception { get; set; }
    }
}