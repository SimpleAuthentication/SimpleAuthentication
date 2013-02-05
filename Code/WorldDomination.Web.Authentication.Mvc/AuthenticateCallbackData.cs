using System;

namespace WorldDomination.Web.Authentication.Mvc
{
    public class AuthenticateCallbackData
    {
        public IAuthenticatedClient AuthenticatedClient { get; set; }
        public Exception Exception { get; set; }
    }
}