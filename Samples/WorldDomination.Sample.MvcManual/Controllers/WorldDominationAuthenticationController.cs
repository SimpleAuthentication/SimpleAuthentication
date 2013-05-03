using System;
using System.Web;
using System.Web.Mvc;
using WorldDomination.Sample.MvcManual.Models;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Csrf;

namespace WorldDomination.Sample.MvcManual.Controllers
{
    public class WorldDominationAuthenticationController : Controller
    {
        private string _cookieName;

        public WorldDominationAuthenticationController(IAuthenticationService authenticationService,
                                                       IAntiForgery antiForgery = null)
        {
            if (authenticationService == null)
            {
                throw new ArgumentNullException("authenticationService");
            }

            AuthenticationService = authenticationService;
            AntiForgery = antiForgery;
        }

        protected IAntiForgery AntiForgery { get; set; }
        protected IAuthenticationService AuthenticationService { get; private set; }
        protected Uri RedirectUrl { get; set; }

        public string CsrfCookieName
        {
            get { return _cookieName ?? (_cookieName = AntiForgery.DefaultCookieName); }
            set { _cookieName = value; }
        }

        public RedirectResult RedirectToProvider(RedirectToProviderInputModel inputModel)
        {
            if (!ModelState.IsValid)
            {
                throw new ArgumentException(
                    "Some binding errors occured. This means at least one Request value (eg. form post or querystring parameter) provided is invalid. Generally, we need a ProviderName as a string.");
            }

            if (string.IsNullOrEmpty(inputModel.ProviderKey))
            {
                throw new ArgumentException(
                    "ProviderKey value missing. You need to supply a valid provider key so we know where to redirect the user Eg. google.");
            }

            // Grab the required Provider settings.
            var settings = AuthenticationService.GetAuthenticateServiceSettings(inputModel.ProviderKey,
                                                                                Request.Url,
                                                                                Url.CallbackFromOAuthProvider());

            // An OpenId specific settings provided?
            if (!string.IsNullOrEmpty(inputModel.Identifier) &&
                settings is IOpenIdAuthenticationServiceSettings)
            {
                Uri identifier;
                if (!Uri.TryCreate(inputModel.Identifier, UriKind.RelativeOrAbsolute, out identifier))
                {
                    throw new ArgumentException(
                        "Indentifier value was not in the correct Uri format. Eg. http://myopenid.com or https://yourname.myopenid.com");
                }
                ((IOpenIdAuthenticationServiceSettings) settings).Identifier = identifier;
            }

            // Our convention is to remember some redirect url once we are finished in the callback.
            // NOTE: If no redirectUrl data has been provided, then default to the Referrer, if one exists.
            string extraData = null;
            if (RedirectUrl != null &&
                !string.IsNullOrEmpty(RedirectUrl.AbsoluteUri))
            {
                // We have extra state information we will need to retrieve.
                extraData = RedirectUrl.AbsoluteUri;
            }
            else if (Request != null &&
                     Request.UrlReferrer != null &&
                     !string.IsNullOrEmpty(Request.UrlReferrer.AbsoluteUri))
            {
                extraData = Request.UrlReferrer.AbsoluteUri;
            }

            // Generate a token pair.
            var token = AntiForgery.CreateToken(extraData);

            // Put the "ToSend" value in the state parameter to send along to the OAuth Provider.
            settings.State = token.ToSend;

            // Serialize the ToKeep value in the cookie.
            SerializeToken(Response, token.ToKeep);

            // Determine the provider's end point Url we need to redirect to.
            var uri = AuthenticationService.RedirectToAuthenticationProvider(settings);

            // Kthxgo!
            return Redirect(uri.AbsoluteUri);
        }

        public ActionResult AuthenticateCallback(string providerkey)
        {
            if (string.IsNullOrEmpty(providerkey))
            {
                throw new ArgumentException("No provider key was supplied on the callback.");
            }

            // Determine which settings we need, based on the Provider.
            var settings = AuthenticationService.GetAuthenticateServiceSettings(providerkey, Request.Url,
                                                                                Url.CallbackFromOAuthProvider());

            // Pull the "ToKeep" token from the cookie and the "ToSend" token from the query string
            var keptToken = DeserializeToken(Request);
            var recievedToken = Request.QueryString["state"];
            if (string.IsNullOrEmpty(recievedToken))
            {
                throw new InvalidOperationException(
                    "No state/recievedToken was retrieved from the provider. Are you sure you passed any state/token data to provider .. and .. that the provider can send it back to us? We need this to prevent any Cross site request forgery.");
            }

            // Validate the token against the recieved one and grab extra data
            string extraData = AntiForgery.ValidateToken(keptToken, recievedToken);

            var model = new AuthenticateCallbackViewModel();

            try
            {
                // Grab the authenticated client information.
                model.AuthenticatedClient = AuthenticationService.GetAuthenticatedClient(settings, Request.QueryString);
            }
            catch (Exception exception)
            {
                model.Exception = exception;
            }

            // If we have a redirect Url, lets grab this :)
            // NOTE: We've implimented the extraData part of the tokenData as the redirect url.
            if (!string.IsNullOrEmpty(extraData))
            {
                model.RedirectUrl = new Uri(extraData);
            }

            // Finally! We can hand over the logic to the consumer to do whatever they want.
            return View("AuthenticateCallback", model);
        }

        private void SerializeToken(HttpResponseBase response, string token)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException("token");
            }

            // Create a cookie.
            var cookie = CreateAYummyCookie(token);
            response.Cookies.Add(cookie);
        }

        private string DeserializeToken(HttpRequestBase request)
        {
            if (request == null)
            {
                throw new ArgumentNullException();
            }

            // Try and read in the cookie value.
            var existingCookie = request.Cookies[CsrfCookieName];
            var token = existingCookie != null ? existingCookie.Value : null;

            // Lets clean up.
            // To remove a cookie (from the client), we need to reset it's time to the past.
            var cookie = CreateAYummyCookie(null, DateTime.UtcNow.AddDays(-7));
            Response.Cookies.Add(cookie);

            return token;
        }

        private HttpCookie CreateAYummyCookie(string token, DateTime? expiryDate = null)
        {
            // Note: Token - can be null.

            // Create a cookie.
            var cookie = new HttpCookie(CsrfCookieName)
            {
                Value = token,
                HttpOnly = true
            };

            if (expiryDate.HasValue)
            {
                cookie.Expires = expiryDate.Value;
            }

            return cookie;
        }
    }

    public static class UrlHelperExtensions
    {
        public static string RedirectToOAuthProvider(this UrlHelper url, string providerName)
        {
            return url.Action("RedirectToProvider", "WorldDominationAuthentication", new { providerKey = providerName });
        }

        public static string CallbackFromOAuthProvider(this UrlHelper url)
        {
            return url.Action("AuthenticateCallback", "WorldDominationAuthentication");
        }
    }
}