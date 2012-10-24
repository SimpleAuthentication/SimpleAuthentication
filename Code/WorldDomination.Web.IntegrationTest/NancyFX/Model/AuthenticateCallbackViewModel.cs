using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WorldDomination.Web.Authentication;

namespace WorldDomination.Web.IntegrationTest.NancyFX.Model
{
    public class AuthenticateCallbackViewModel
    {
        public IAuthenticatedClient AuthenticatedClient { get; set; }
        public Exception Exception { get; set; }
    }
}