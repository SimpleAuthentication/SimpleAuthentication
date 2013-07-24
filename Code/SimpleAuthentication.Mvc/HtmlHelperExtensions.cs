using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace SimpleAuthentication.Mvc
{
    public static class HtmlHelperExtensions
    {
        //public static IHtmlString RedirectToProvider(this HtmlHelper htmlHelper,
        //                                             string providerName,
        //                                             string imagePath,
        //                                             string imageAlternativeText)
        //{
        //    return RedirectToProvider(htmlHelper, providerName, imagePath, imageAlternativeText, null, null, null);
        //}

        //public static IHtmlString RedirectToProvider(this HtmlHelper htmlHelper,
        //                                             string providerName,
        //                                             string imagePath,
        //                                             string imageAlternativeText = null,
        //                                             string returnUrl = null,
        //                                             IDictionary<string, object> imageHtmltmlAttributes = null,
        //                                             IDictionary<string, object> htmlAttributes = null
        //    )
        //{
        //    if (string.IsNullOrEmpty(providerName))
        //    {
        //        throw new ArgumentNullException("providerName",
        //                                        "Missing a providerName value. Please provide one so we know what route to generate.");
        //    }

        //    if (string.IsNullOrEmpty(imagePath))
        //    {
        //        throw new ArgumentNullException("imagePath",
        //            "Missing an imagePath value. Please provide one so we know which image to display. Eg. \"Content/google.png\"");
        //    }

        //    // Lets generate a link.
        //    var tagBuilder = new TagBuilder("img");

        //    // Image src="xxxx" attribute.
        //    var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
        //    var url = urlHelper.Content(imagePath);
        //    tagBuilder.MergeAttribute("src", url);

        //    // 'Alt' attribute.
        //    if (!string.IsNullOrEmpty(imageAlternativeText))
        //    {
        //        tagBuilder.MergeAttribute("alt", imageAlternativeText);
        //    }

        //    // Merge any optional attributes. For example, class values, etc.
        //    if (htmlAttributes != null)
        //    {
        //        tagBuilder.MergeAttributes(imageHtmltmlAttributes);
        //    }

        //    string imageHtml = tagBuilder.ToString(TagRenderMode.SelfClosing);

        //    return RedirectToProvider(htmlHelper, providerName, imageHtml, returnUrl, htmlAttributes);
        //}

        public static IHtmlString RedirectToProvider(this HtmlHelper htmlHelper,
                                                     string providerName,
                                                     string innerHtml,
                                                     string returnUrl = null,
                                                     IDictionary<string, object> htmlAttributes = null)
        {
            if (string.IsNullOrEmpty(providerName))
            {
                throw new ArgumentNullException("providerName",
                                                "Missing a providerName value. Please provide one so we know what route to generate.");
            }

            if (string.IsNullOrEmpty(innerHtml))
            {
                throw new ArgumentNullException("innerHtml",
                                                "Missing an innerHtml value. We need to display some link text or image to be able to click on - so please provide some html. eg. <img src=\"/ContentResult/someButton.png\" alt=\"click me\"/>");
            }

            // Start with an <a /> element.
            var tagBuilder = new TagBuilder("a")
                             {
                                 InnerHtml = innerHtml
                             };

            // Merge any optional attributes. For example, class values, etc.
            if (htmlAttributes != null)
            {
                tagBuilder.MergeAttributes(htmlAttributes);
            }

            // Determine the route.
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var url = urlHelper.RedirectToProvider(providerName, returnUrl);

            // Set the route.
            tagBuilder.MergeAttribute("href", url);

            return new HtmlString(tagBuilder.ToString(TagRenderMode.Normal));
        }
    }
}