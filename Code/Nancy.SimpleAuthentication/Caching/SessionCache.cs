using Nancy.Session;
using SimpleAuthentication.Core;

namespace Nancy.SimpleAuthentication.Caching
{
    public class SessionCache : ICache
    {
        private readonly ISession _session;

        public SessionCache(ISession session)
        {
            _session = session;
        }

        public void Add(string key, object data)
        {
            _session[key] = data;
        }

        public object Get(string key)
        {
            return _session[key];
        }
    }
}