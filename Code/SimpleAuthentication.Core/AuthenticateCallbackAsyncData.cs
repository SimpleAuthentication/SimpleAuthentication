using System;
using System.Collections.Generic;

namespace SimpleAuthentication.Core
{
    public class AuthenticateCallbackAsyncData
    {
        public AuthenticateCallbackAsyncData(Uri requestUrl,
            CacheData cacheData,
            dynamic controllerOrModule,
            IDictionary<string, string> queryStringKeyValues)
        {
            if (requestUrl == null)
            {
                throw new ArgumentNullException("requestUrl");
            }

            if (cacheData == null)
            {
                throw new ArgumentNullException("cacheData");
            }

            if (controllerOrModule == null)
            {
                throw new ArgumentNullException("controllerOrModule");
            }

            RequestUrl = requestUrl;
            CacheData = cacheData;

            // Can be null.
            QueryStringKeyValues = queryStringKeyValues;
        }

        public CacheData CacheData { get; private set; }
        public dynamic ControllerOrModule { get; set; }
        public IDictionary<string, string> QueryStringKeyValues { get; set; }
        public Uri RequestUrl { get; private set; }
    }
}