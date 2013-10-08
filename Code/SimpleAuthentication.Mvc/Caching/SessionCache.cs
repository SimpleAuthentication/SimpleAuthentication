using System;
using System.Web;
using SimpleAuthentication.Core;

namespace SimpleAuthentication.Mvc.Caching
{
    public class SessionCache : ICache
    {
        private readonly HttpSessionStateBase _session;

        public SessionCache(HttpSessionStateBase session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            _session = session;
        }

        public void Add(string key, object data)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Required: a 'key' value is required.", "key");
            }

            if (data == null)
            {
                throw new ArgumentNullException("data", "Required: an instance of some object is required.");
            }

            _session[key] = data;
        }

        public object Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Required: a 'key' value is required.", "key");
            }

            return _session[key];
        }
    }
}