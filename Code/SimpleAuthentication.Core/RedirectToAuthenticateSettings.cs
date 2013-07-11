using System;

namespace SimpleAuthentication.Core
{
    public class RedirectToAuthenticateSettings
    {
        public string State { get; set; }
        public Uri RedirectUri { get; set; }
    }
}