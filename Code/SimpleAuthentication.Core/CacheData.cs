using System;

namespace SimpleAuthentication.Core
{
    public class CacheData
    {
        public CacheData(string providerKey,
            string state,
            string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(providerKey))
            {
                throw new ArgumentNullException("providerKey");
            }

            if (string.IsNullOrWhiteSpace(state))
            {
                throw new ArgumentNullException("state");
            }

            ProviderKey = providerKey;
            State = state;

            // Optionals.
            ReturnUrl = returnUrl;
        }

        public string ProviderKey { get; set; }
        public string State { get; set; }
        public string ReturnUrl { get; set; }

        public override string ToString()
        {
            return string.Format("Provider: {0}. State: {1}. ReturnUrl: {2}.",
                string.IsNullOrWhiteSpace(ProviderKey)
                    ? "--no provider key--"
                    : ProviderKey,
                string.IsNullOrWhiteSpace(State)
                    ? "--no state--"
                    : State,
                string.IsNullOrWhiteSpace(ReturnUrl)
                    ? "--no return url--"
                    : ReturnUrl);
        }
    }
}