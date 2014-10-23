using System;
using System.Web.Mvc;

namespace SimpleAuthentication.Mvc
{
    public static class UrlHelperExtensions
    {
        public static string RedirectToProvider(this UrlHelper url, string providerName, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(providerName))
            {
                throw new ArgumentNullException("providerName",
                                                "Missing a providerName value. Please provide one (boom tish!) so we know what route to generate.");
            }

            return url.Action("RedirectToProvider", "SimpleAuthentication", new {providerName, returnUrl, area = "" });
        }
    }
}