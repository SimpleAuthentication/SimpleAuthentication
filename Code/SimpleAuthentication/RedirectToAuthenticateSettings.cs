using System;

namespace SimpleAuthentication
{
    public class RedirectToAuthenticateSettings
    {
        public string State { get; set; }
        public Uri RedirectUri { get; set; }
    }
}