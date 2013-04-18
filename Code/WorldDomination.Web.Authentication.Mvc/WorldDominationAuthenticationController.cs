using System;
using System.Web;
using System.Web.Mvc;
using WorldDomination.Web.Authentication.Csrf;

namespace WorldDomination.Web.Authentication.Mvc
{
    public class WorldDominationAuthenticationController : Controller
    {
        private readonly IAntiForgery _antiForgery;
        private readonly LoggingWrapper _log;
        private string _cookieName;

        public WorldDominationAuthenticationController(IAuthenticationService authenticationService,
                                                       IAuthenticationCallbackProvider callbackProvider,
                                                       IAntiForgery antiForgery = null,
                                                       ILoggingService loggingService = null)
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
            _antiForgery = antiForgery ?? new AspNetAntiForgery();

            // NOTE: loggingService can be null, which means we don't care about logging.
            _log = new LoggingWrapper(loggingService);
        }

        protected IAuthenticationService AuthenticationService { get; private set; }
        public IAuthenticationCallbackProvider CallbackProvider { get; private set; }

        protected Uri RedirectUrl { get; set; }

        public string CsrfCookieName
        {
            get { return _cookieName ?? (_cookieName = _antiForgery.DefaultCookieName); }
            set { _cookieName = value; }
        }

        public RedirectResult RedirectToProvider(RedirectToProviderInputModel inputModel)
        {
            _log.Debug("RedirectToProvider: starting to redirect to the provider.");

            if (!ModelState.IsValid)
            {
                const string errorMessage = "Some binding errors occured. This means at least one Request value (eg. form post or querystring parameter) provided is invalid. Generally, we need a ProviderName as a string.";;
                _log.Error(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            if (string.IsNullOrEmpty(inputModel.ProviderKey))
            {
                const string errorMessage = "ProviderKey value missing. You need to supply a valid provider key so we know where to redirect the user Eg. google.";
                _log.Error(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            // Grab the required Provider settings.
            var settings = AuthenticationService.GetAuthenticateServiceSettings(inputModel.ProviderKey,
                                                                                Request.Url,
                                                                                Url.CallbackFromOAuthProvider());

            _log.Info(string.Format("RedirectToProvider: Provider [{0}].", settings.ProviderName));
            _log.Info(string.Format("RedirectToProvider: Callback Url [{0}].", settings.CallBackUri));

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
            var token = _antiForgery.CreateToken(extraData);

            // Put the "ToSend" value in the state parameter to send along to the OAuth Provider.
            settings.State = token.ToSend;

            _log.Info(string.Format("RedirectToProvider: State [{0}].", settings.State));

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
            string extraData = _antiForgery.ValidateToken(keptToken, recievedToken);

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
            if (!string.IsNullOrEmpty(extraData))
            {
                model.RedirectUrl = new Uri(extraData);
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