using System;

namespace SimpleAuthentication
{
    public class RedirectToAuthenticateInputModel
    {
        public string ProviderName { get; set; }
        public string Identity { get; set; }
        public Uri CallbackUri { get; set; }
    }
}