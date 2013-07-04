using System;

namespace WorldDomination.Web.Authentication
{
    public class RedirectToAuthenticationProviderSettings
    {
        private const string DefaultCallbackPath = "/authentication/authenticatecallback";
        private string _callbackPath;

        public RedirectToAuthenticationProviderSettings(string providerName,
            Uri requestUri, string callbackPath)
        {
            if (string.IsNullOrEmpty(providerName))
            {
                throw new ArgumentNullException("providerName");
            }

            if (requestUri == null ||
                string.IsNullOrEmpty(requestUri.AbsoluteUri))
            {
                throw new ArgumentNullException("requestUri");
            }

            if (string.IsNullOrEmpty(callbackPath))
            {
                throw new ArgumentNullException(callbackPath);
            }

            ProviderName = providerName;
            RequestUri = requestUri;
            CallbackPath = callbackPath;
        }

        /// <summary>
        /// Which provider to redirect to.
        /// </summary>
        public string ProviderName { get; private set; }

        /// <summary>
        /// The current request information. 
        /// </summary>
        public Uri RequestUri { get; private set; }

        /// <summary>
        /// The route to callback to the server once you've finished authenticating with the provider.
        /// </summary>
        public string CallbackPath
        {
            get { return string.IsNullOrEmpty(_callbackPath) ? DefaultCallbackPath : _callbackPath; }
            private set { _callbackPath = value; }
        }
    }
}