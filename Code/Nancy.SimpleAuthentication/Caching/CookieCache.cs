using System;
using SimpleAuthentication.Core;

namespace Nancy.SimpleAuthentication.Caching
{
    public class CookieCache : ICache
    {
        private readonly NancyContext _context;

        public CookieCache(NancyContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            _context = context;
        }

        public CacheData this[string key]
        {
            get
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentNullException("key");
                }

                throw new NotImplementedException();
                //return _context.Request.Cookies[key];
            }
            set
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentNullException("key");
                }

                throw new NotImplementedException();
                //_context.Response.Cookies.Add(new NancyCookie(key, value));
            }
        }
    }
}