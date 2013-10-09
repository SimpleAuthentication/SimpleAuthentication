using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy.Cookies;
using SimpleAuthentication.Core;

namespace Nancy.SimpleAuthentication.Caching
{
    public class CookieCache : ICache
    {
        private readonly NancyContext _context;

        public CookieCache(NancyContext context)
        {
            _context = context;
        }

        public void Add(string key, object data)
        {
            _context.Response.Cookies.Add(new NancyCookie(key, data.ToString()));
        }

        public object Get(string key)
        {
            return _context.Request.Cookies[key];
        }
    }
}
