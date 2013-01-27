using System;

namespace WorldDomination.Web.Authentication.Samples.NancyFX2.Model
{
    public class AuthenticateCallbackViewModel
    {
        public IAuthenticatedClient AuthenticatedClient { get; set; }
        public Exception Exception { get; set; }
    }
}