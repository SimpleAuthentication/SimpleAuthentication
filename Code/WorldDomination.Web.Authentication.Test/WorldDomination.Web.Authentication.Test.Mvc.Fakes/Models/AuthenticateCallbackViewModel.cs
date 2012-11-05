using System;

namespace WorldDomination.Web.Authentication.Test.Mvc.Fakes.Models
{
    public class AuthenticateCallbackViewModel
    {
        public IAuthenticatedClient AuthenticatedClient { get; set; }
        public Exception Exception { get; set; }
    }
}