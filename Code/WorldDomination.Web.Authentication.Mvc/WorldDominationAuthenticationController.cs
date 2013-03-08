using System;
using System.Web;
using System.Web.Mvc;
using WorldDomination.Web.Authentication.Csrf;

namespace WorldDomination.Web.Authentication.Mvc
{
    public class WorldDominationAuthenticationController : Controller
    {
        private const string CookieName = "__WorldDomination.Web.Authentication.Mvc.CsrfToken";
        private readonly IAntiForgery _antiForgery;
        private string _cookieName;

        public WorldDominationAuthenticationController(IAuthenticationService authenticationService,
                                                       IAuthenticationCallbackProvider callbackProvider,
                                                       IAntiForgery antiForgery = null)
        {
            if (authenticationService == null)
            {
                throw new ArgumentNullException("authenticationService");
            }

            if (callbackProvider == null)
            {
                throw new ArgumentNullException("callbackProvider");
            }

            AuthenticationService = authenticationService;
            CallbackProvider = callbackProvider;

            // If no anti forgery class is provided, then we'll just use the default.
            _antiForgery = antiForgery ?? new AntiForgery();
        }

        protected IAuthenticationService AuthenticationService { get; private set; }
        public IAuthenticationCallbackProvider CallbackProvider { get; private set; }

        protected Uri RedirectUrl { get; set; }

        public string CsrfCookieName
        {
            get { return string.IsNullOrEmpty(_cookieName) ? CookieName : _cookieName; }
            set { _cookieName = value; }
        }

        public RedirectResult RedirectToProvider(string providerkey)
        {
            if (string.IsNullOrEmpty(providerkey))
            {
                throw new ArgumentException(
                    "You need to supply a valid provider key so we know where to redirect the user.");
            }

            // Grab the required Provider settings.
            var settings = AuthenticationService.GetAuthenticateServiceSettings(providerkey, Request.Url);

            // Generate the Csrf token. 
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
            var token = _antiForgery.CreateToken(extraData);
            settings.State = token;

            // Now serialize this token (so we can complete the Csrf, in the callback).
            SerializeToken(Response, token);

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
            var settings = AuthenticationService.GetAuthenticateServiceSettings(providerkey, Request.Url);

            // Make sure we use our 'previous' State value.
            var token = DeserializeToken(Request);
            settings.State = token;
            TokenData tokenData = null;
            if (!string.IsNullOrEmpty(token))
            {
                tokenData = _antiForgery.ValidateToken(token);
            }

            var model = new AuthenticateCallbackData();

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
            if (tokenData != null && !string.IsNullOrEmpty(tokenData.ExtraData))
            {
                model.RedirectUrl = new Uri(tokenData.ExtraData);
            }

            // Finally! We can hand over the logic to the consumer to do whatever they want.
            return CallbackProvider.Process(HttpContext, model);
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
}