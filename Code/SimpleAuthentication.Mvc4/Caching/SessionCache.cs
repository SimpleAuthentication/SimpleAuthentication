using System;
using System.Web;
using System.Web.SessionState;
using SimpleAuthentication.Core;

namespace SimpleAuthentication.Mvc.Caching
{
    public class SessionCache : ICache
    {
        private HttpSessionState _session;

        public string this[string key]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new ArgumentException("Required: a 'key' value is required.", "key");
                }

                return _session[key] as string;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new ArgumentException("Required: a 'key' value is required.", "key");
                }

                if (value == null)
                {
                    throw new ArgumentNullException("value", "Required: an instance of some object is required.");
                }

                _session[key] = value;
            }
        }

        public void Initialize()
        {
            if (HttpContext.Current != null &&
                HttpContext.Current.Session != null)
            {
                _session = HttpContext.Current.Session;
            }
        }
    }
}