using System.Web;
using SimpleAuthentication.Core;

namespace SimpleAuthentication.Mvc.Caching
{
    public class CookieCache : ICache
    {
        private const string DefaultCookieName = "SimpleAuthentication.MvcCache.NomNomNoms";

        private HttpContext _httpContext;

        public CookieCache()
        {
            Name = DefaultCookieName;
        }

        public string Name { get; set; }

        public string this[string key]
        {
            get
            {
                var cookie = _httpContext.Request.Cookies[Name];
                return cookie == null ? null : cookie.Values[key];
            }

            set
            {
                var cookie = _httpContext.Request.Cookies[Name] ?? new HttpCookie(Name);

                cookie.Values[key] = value;

                _httpContext.Response.AppendCookie(cookie);
            }
        }

        public void Initialize()
        {
            if (HttpContext.Current != null &&
                HttpContext.Current.Session != null)
            {
                _httpContext = HttpContext.Current;
            }
        }
    }
}