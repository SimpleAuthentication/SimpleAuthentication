using System.Web;
using SimpleAuthentication.Core;

namespace SimpleAuthentication.Mvc.Caching
{
    public class CookieCache : ICache
    {
        private const string DefaultCookieName = "SimpleAuthentication.MvcCache.NomNomNoms";

        private HttpContext httpContext;

        public CookieCache()
        {
            Name = DefaultCookieName;
        }

        public string Name { get; set; }

        public string this[string key]
        {
            get
            {
                var cookie = httpContext.Request.Cookies[Name];
                return cookie == null ? null : cookie.Values[key];
            }

            set
            {
                var cookie = httpContext.Request.Cookies[Name] ?? new HttpCookie(Name);

                cookie.Values[key] = value;

                httpContext.Response.AppendCookie(cookie);
            }
        }

        public void Initialize()
        {
            if (HttpContext.Current != null)
            {
                httpContext = HttpContext.Current;
            }
        }
    }
}