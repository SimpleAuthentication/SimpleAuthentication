using System.Web.Mvc;

namespace SimpleAuthentication.Mvc
{
    public static class UrlHelperExtensions
    {
        public static string RedirectToProvider(this UrlHelper url, string providerName, string returnUrl = null)
        {
            return url.Action("RedirectToProvider", "SimpleAuthentication", new { providerName, returnUrl });
        }
    }
}