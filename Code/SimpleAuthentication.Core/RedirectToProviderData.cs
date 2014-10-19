using System;

namespace SimpleAuthentication.Core
{
    public class RedirectToProviderData
    {
        public RedirectToProviderData(string providerKey,
            Uri requestUrl,
            string referer,
            string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(providerKey))
            {
                throw new ArgumentNullException("providerKey");
            }

            if (requestUrl == null)
            {
                throw new ArgumentNullException("requestUrl");
            }

            ProviderKey = providerKey;
            RequestUrl = requestUrl;

            // Can be nulls.
            Referer = referer;
            ReturnUrl = returnUrl;
        }

        public string ProviderKey { get; private set; }
        public Uri RequestUrl { get; private set; }
        public string ReturnUrl { get; private set; }
        public string Referer { get; private set; }
    }
}