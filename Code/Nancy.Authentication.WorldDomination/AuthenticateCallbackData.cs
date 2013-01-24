using System;
using WorldDomination.Web.Authentication;

namespace Nancy.Authentication.WorldDomination
{
    public class AuthenticateCallbackData
    {
        public IAuthenticatedClient AuthenticatedClient { get; set; }
        public Exception Exception { get; set; }
    }
}