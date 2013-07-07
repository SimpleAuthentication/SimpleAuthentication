using System;
using System.Diagnostics;
using System.Web.Mvc;
using SimpleAuthentication.Providers;
using SimpleAuthentication.Tracing;

namespace SimpleAuthentication.Mvc
{
    public class SimpleAuthenticationController : Controller
    {
        private const string SessionKeyAccessToken = "SimpleAuth.Session.AccessToken";
        private const string SessionKeyState = "SimpleAuth.Session.StateToken";
        private const string SessionKeyRedirectToUrl = "SimpleAuth.Session.RedirectToUrl";
        private const string SessionKeyRedirectToProviderUrl = "SimpleAuth.Session.";

        private readonly AuthenticationProviderFactory _authenticationProviderFactory;

        public SimpleAuthenticationController(IAuthenticationCallbackProvider callbackProvider)
        {
            _authenticationProviderFactory = new AuthenticationProviderFactory();

            if (callbackProvider == null)
            {
                throw new ArgumentNullException("callbackProvider");
            }

            CallbackProvider = callbackProvider;

            // Lazyily setup our TraceManager.
            TraceManager = new Lazy<ITraceManager>(() => new TraceManager()).Value;
        }

        /// <summary>
        /// Your custom callback code which is used to handle whatever you want to do with the UserInformation and Access Tokens.
        /// </summary>
        public IAuthenticationCallbackProvider CallbackProvider { get; private set; }

        public ITraceManager TraceManager { set; private get; }

        private TraceSource TraceSource
        {
            get { return TraceManager["SimpleAuthentication.Mvc.SimpleAuthenticationController"]; }
        }

        public RedirectResult RedirectToProvider(RedirectToProviderInputModel inputModel)
        {
            #region Input Model Validation

            if (!ModelState.IsValid)
            {
                throw new ArgumentException(
                    "Some binding errors occured. This means at least one Request value (eg. form post or querystring parameter) provided is invalid. Generally, we need a ProviderName as a string.");
            }

            if (string.IsNullOrEmpty(inputModel.ProviderName))
            {
                throw new ArgumentException(
                    "ProviderKey value missing. You need to supply a valid provider key so we know where to redirect the user Eg. google.");
            }

            Uri identifier = null;
            if (!string.IsNullOrEmpty(inputModel.Identifier) &&
                !Uri.TryCreate(inputModel.Identifier, UriKind.Absolute, out identifier))
            {
                throw new ArgumentException("The Identifier value [" + inputModel.Identifier +
                                            "] is not a valid Uri. Please fix it up. eg. http://goto.some.website/authenticate/");
            }

            #endregion

            // Grab the Provider.
            var provider = GetAuthenticationProvider(inputModel.ProviderName);

            // Most providers don't need any pre-setup crap, to redirect to authenticate.
            // But of course, there's always one - OpenId. We have no idea WHERE we want to
            // redirect to, so we need to do a particular check here.
            // Of course, any value here could be used for any other provider. But that would be weird.
            // TODO: Confirm this is not a security threat / open to abuse in some way.
            if (identifier != null)
            {
                provider.AuthenticateRedirectionUrl = identifier;
            }

            // Where do we return to, after we've authenticated?
            var callbackUri = GenerateCallbackUri(provider.Name);

            // Determine where we need to redirect to.
            var redirectToAuthenticateSettings = provider.RedirectToAuthenticate(callbackUri);

            // Remember any important information for after we've come back.
            Session[SessionKeyState] = redirectToAuthenticateSettings.State;
            Session[SessionKeyRedirectToUrl] = Request.UrlReferrer;
            Session[SessionKeyRedirectToProviderUrl] = redirectToAuthenticateSettings.RedirectUri.AbsoluteUri;

            // Now redirect :)
            return Redirect(redirectToAuthenticateSettings.RedirectUri.AbsoluteUri);
        }

        public ActionResult AuthenticateCallback(AuthenticateCallBackInputModel inputModel)
        {
            #region Input Model Validation

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

            #endregion

            var previousRedirectUrl = string.IsNullOrEmpty((string) Session[SessionKeyRedirectToProviderUrl])
                                          ? "N.A."
                                          : (string) Session[SessionKeyRedirectToProviderUrl];
            TraceSource.TraceInformation("Previous Redirect Url: " + previousRedirectUrl);

            #region Deserialize Tokens, etc.

            // Retrieve any (previously) serialized access token stuff. (eg. public/private keys and state).
            // TODO: Check if this is an access token or an auth token thingy-thing.
            TraceSource.TraceVerbose("Retrieving (local serializaed) AccessToken, State and RedirectToUrl.");
            var accessToken = Session[SessionKeyAccessToken] as AccessToken;
            var state = Session[SessionKeyState] as string;
            var redirectToUrl = Session[SessionKeyRedirectToUrl] as Uri;

            #endregion

            // Lets now start to setup the view model.
            var model = new AuthenticateCallbackData();

            #region Retrieve the User Information

            try
            {
                // Which provider did we just authenticate with?
                var provider = GetAuthenticationProvider(inputModel.ProviderKey);

                // Where do we return to, after we've authenticated?
                var callbackUri = GenerateCallbackUri(provider.Name);

                // Grab the user information.
                model.AuthenticatedClient = provider.AuthenticateClient(Request.QueryString, state, callbackUri);
            }
            catch (Exception exception)
            {
                TraceSource.TraceError(exception.Message);
                model.Exception = exception;
            }

            #endregion

            // Do we have an optional redirect resource? Usually a previous referer?
            if (redirectToUrl != null)
            {
                TraceSource.TraceVerbose("Found redirectToUrl: " + redirectToUrl);
                model.RedirectUrl = redirectToUrl;
            }

            // Finally! We can hand over the logic to the consumer to do whatever they want.
            TraceSource.TraceVerbose("About to execute your custom callback provider logic.");
            return CallbackProvider.Process(HttpContext, model);
        }

        private IAuthenticationProvider GetAuthenticationProvider(string providerKey)
        {
            if (string.IsNullOrEmpty(providerKey))
            {
                throw new ArgumentNullException("providerKey");
            }

            TraceSource.TraceVerbose("Trying to retrieve a provider for the given key: " + providerKey);

            IAuthenticationProvider provider = null;

            if (_authenticationProviderFactory.AuthenticationProviders.ContainsKey(providerKey))
            {
                TraceSource.TraceVerbose("Found registered provider: " + providerKey);
                provider = _authenticationProviderFactory.AuthenticationProviders[providerKey];
            }
            else if (providerKey.StartsWith("fake", StringComparison.InvariantCultureIgnoreCase))
            {
                // Ah-ha! We've been asked for a fake key :P
                TraceSource.TraceVerbose("Request for a *Fake* provider. Creating the fake provider: " + providerKey);
                provider = new FakeProvider(providerKey);
            }

            // So, did we get a real or fake key?
            if (provider == null)
            {
                var errorMessage = string.Format("There was no provider registered for the given key: {0}.", providerKey);
                TraceSource.TraceError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            TraceSource.TraceVerbose("Found - Provider: {0}.",
                                     string.IsNullOrEmpty(provider.Name)
                                         ? "-no provider name-"
                                         : provider.Name);

            return provider;
        }

        private Uri GenerateCallbackUri(string providerName)
        {
            return SystemHelpers.CreateCallBackUri(providerName, Request.Url,
                                                   Url.RouteUrl(SimpleAuthenticationRouteConfig.CallbackRouteName));
        }


        // ********************************************************************************************************


        /*






        protected Uri RedirectUrl { get; set; }

        public string CsrfCookieName
        {
            get { return _cookieName ?? (_cookieName = _antiForgery.DefaultCookieName); }
            set { _cookieName = value; }
        }

        public RedirectResult RedirectToProviderOLD(RedirectToProviderInputModel inputModel)
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
            var token = _antiForgery.CreateToken(extraData);

            // Put the "ToSend" value in the state parameter to send along to the OAuth Provider.
            //settings.State = token.ToSend;

            // Serialize the ToKeep value in the cookie.
            SerializeToken(Response, token.ToKeep);

            // Determine the provider's end point Url we need to redirect to.
            var providerSettings = new ProviderSettings
            {
                State = token.ToSend
            };

            var uri = AuthenticationService.RedirectToAuthenticationProvider(providerSettings);

            // Kthxgo!
            return Redirect(uri.AbsoluteUri);
        }

        public ActionResult AuthenticateCallbackOLD(string providerkey)
        {
            if (string.IsNullOrEmpty(providerkey))
            {
                const string errorMessage = "No provider key was supplied on the callback.";
                TraceSource.TraceError(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            // Determine which settings we need, based on the Provider.
            TraceSource.TraceVerbose("Trying to determine what provider we just came from, based upon some url parameters.");
            var settings = AuthenticationService.GetAuthenticateServiceSettings(providerkey, Request.Url,
                                                                                Url.CallbackFromOAuthProvider());
            TraceSource.TraceVerbose("Found - Provider: {0}. CallBackUri: {1}. State: {2}",
                                     string.IsNullOrEmpty(settings.ProviderName)
                                         ? "-no provider name-"
                                         : settings.ProviderName,
                                     settings.CallBackUri == null
                                         ? "-no callback uri-"
                                         : settings.CallBackUri.AbsoluteUri,
                                     string.IsNullOrEmpty(settings.State) ? "-no state-" : settings.State);

            // Pull the "ToKeep" token from the cookie and the "ToSend" token from the query string
            var keptToken = DeserializeToken(Request);
            var receivedToken = Request.QueryString["state"];
            if (string.IsNullOrEmpty(receivedToken))
            {
                const string errorMessage = "No state/recievedToken was retrieved from the provider. Are you sure you passed any state/token data to provider .. and .. that the provider can send it back to us? We need this to prevent any Cross site request forgery. Check to see that the callback url has a key/value pair: state/your-previous-state-value";
                TraceSource.TraceError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            // Validate the token against the recieved one and grab extra data
            string extraData = _antiForgery.ValidateToken(keptToken, receivedToken);
            TraceSource.TraceVerbose("Retrieved token data: KeptToken: {0} + ReceivedToken: {1} => ExtraData: {2}",
                                     string.IsNullOrEmpty(keptToken) ? "-no kept token-" : keptToken,
                                     string.IsNullOrEmpty(receivedToken) ? "-no received token-" : receivedToken,
                                     string.IsNullOrEmpty(extraData) ? "-no extra data-" : extraData);
            var model = new AuthenticateCallbackData();

            try
            {
                // Grab the authenticated client information.
                model.AuthenticatedClient = AuthenticationService.GetAuthenticatedClient(settings, Request.QueryString);
            }
            catch (Exception exception)
            {
                TraceSource.TraceError(exception.Message);
                model.Exception = exception;
            }

            // If we have a redirect Url, lets grab this :)
            // NOTE: We've implimented the extraData part of the tokenData as the redirect url.
            if (!string.IsNullOrEmpty(extraData))
            {
                model.RedirectUrl = new Uri(extraData);
            }

            // Finally! We can hand over the logic to the consumer to do whatever they want.
            TraceSource.TraceVerbose("About to execute your custom callback provider logic.");
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
         */
    }
}