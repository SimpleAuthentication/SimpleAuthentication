using System;
using Nancy.Session;
using SimpleAuthentication.Core;

namespace Nancy.SimpleAuthentication.Caching
{
    public class SessionCache : ICache
    {
        private readonly ISession _session;

        public SessionCache(ISession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            _session = session;
        }

        public string this[string key]
        {
            get
            {
                return _session[key] as string;
            }
            set
            {
                _session[key] = value;
            }
        }

        public void Initialize()
        {
            // Not used.
        }
    }
}