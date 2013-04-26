using System;
using WorldDomination.Web.Authentication;

namespace WorldDomination.Sample.MvcAuto.Models
{
    public class AuthenticateCallbackViewModel
    {
        public IAuthenticatedClient AuthenticatedClient { get; set; }
        public Exception Exception { get; set; }
    }
}