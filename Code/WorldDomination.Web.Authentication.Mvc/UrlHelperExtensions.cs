using System.Web.Mvc;

namespace WorldDomination.Web.Authentication.Mvc
{
    public static class UrlHelperExtensions
    {
        public static string RedirectToOAuthProvider(this UrlHelper url, string providerName)
        {
            return url.Action("RedirectToProvider", "WorldDominationAuthentication", new {providerKey = providerName});
        }

        public static string CallbackFromOAuthProvider(this UrlHelper url)
        {
            return url.Action("AuthenticateCallback", "WorldDominationAuthentication");
        }
    }
}