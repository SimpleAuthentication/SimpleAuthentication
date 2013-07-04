using System.Web.Mvc;

namespace SimpleAuthentication.Mvc
{
    public static class UrlHelperExtensions
    {
        public static string RedirectToOAuthProvider(this UrlHelper url, string providerName)
        {
            return url.Action("RedirectToProvider", "SimpleAuthentication", new { providerKey = providerName });
        }

        public static string CallbackFromOAuthProvider(this UrlHelper url)
        {
            return url.Action("AuthenticateCallback", "SimpleAuthentication");
        }
    }
}