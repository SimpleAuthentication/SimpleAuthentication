using System;

namespace WorldDomination.Web.Authentication.Samples.Mvc.Advanced.Models
{
    public class AuthenticateCallbackViewModel
    {
        public IAuthenticatedClient AuthenticatedClient { get; set; }
        public Uri Referrer { get; set; }
        public Exception Exception { get; set; }
    }
}