using System;
using Nancy.Cookies;
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

        public string this[string key]
        {
            get
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentNullException("key");
                }

                return _context.Request.Cookies[key];
            }
            set
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentNullException("key");
                }

                _context.Response.Cookies.Add(new NancyCookie(key, value));
            }
        }

        public void Initialize()
        {
            // Not used.
        }
    }
}