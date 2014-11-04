using System;
using SimpleAuthentication.Core;

namespace SimpleAuthentication.Sample.Nancy.Models
{
    public class AuthenticationViewModel
    {
        public IAuthenticatedClient AuthenticatedClient { get; set; }
        public string ReturnUrl { get; set; }
        public string ErrorMessage { get; set; }
        public Exception Exception { get; set; }
    }
}