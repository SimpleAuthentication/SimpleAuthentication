using System;

namespace SimpleAuthentication.Core
{
    public class RedirectToProviderResult
    {
        public RedirectToProviderResult(Uri redirectUrl,
            CacheData cacheData)
        {
            if (redirectUrl == null)
            {
                throw new ArgumentNullException("redirectUrl");
            }

            if (cacheData == null)
            {
                throw new ArgumentNullException("cacheData");
            }

            RedirectUrl = redirectUrl;
            CacheData = cacheData;
        }

        public Uri RedirectUrl { get; private set; }
        public CacheData CacheData { get; private set; }
    }
}