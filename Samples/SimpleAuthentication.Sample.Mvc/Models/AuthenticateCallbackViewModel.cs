using System;
using SimpleAuthentication.Core;

namespace SimpleAuthentication.Sample.Mvc.Models
{
    public class AuthenticateCallbackViewModel
    {
        public IAuthenticatedClient AuthenticatedClient { get; set; }
        public Exception Exception { get; set; }
        public string ReturnUrl { get; set; }
    }
}