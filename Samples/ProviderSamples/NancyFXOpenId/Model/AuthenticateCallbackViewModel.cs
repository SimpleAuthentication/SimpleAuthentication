using System;
using WorldDomination.Web.Authentication;

namespace NancyFXOpenId.Model
{
    public class AuthenticateCallbackViewModel
    {
        public IAuthenticatedClient AuthenticatedClient { get; set; }
        public Exception Exception { get; set; }
    }
}