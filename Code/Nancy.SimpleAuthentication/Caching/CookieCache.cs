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
        private readonly INancyCookie _cookie;
        private NancyModule _module;

        public CookieCache(NancyModule module)
        {
            //if (cookie == null)
            //{
            //    throw new ArgumentNullException("cookie");
            //}

            //_cookie = cookie;

            _module = module;
        }

        public void Add(string key, object data)
        {
            _module.Context.Response.Cookies.Add(new NancyCookie(key, data));
        }

        public object Get(string key)
        {
            _module.Context.Request.Cookies[key];
        }
    }
}
