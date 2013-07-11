using System;
using System.Diagnostics;
using System.Web.Mvc;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers;
using SimpleAuthentication.Core.Tracing;

namespace SimpleAuthentication.Mvc
{
    public class SimpleAuthenticationController : Controller
    {
        private const string SessionKeyAccessToken = "SimpleAuthentication.Session.AccessToken";
        private const string SessionKeyState = "SimpleAuthentication.Session.StateToken";
        private const string SessionKeyRedirectToUrl = "SimpleAuthentication.Session.RedirectToUrl";
        private const string SessionKeyRedirectToProviderUrl = "SimpleAuthentication.Session.";

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

            if (redirectToAuthenticateSettings == null)
            {
                // We failed to determine where to go. A classic example of this is with OpenId and a bad OpenId endpoint.
                const string errorMessage =
                    "No redirect to authencate settings retrieved. This means we don't know where to go. A classic example of this is with OpenId and a bad OpenId endpoint. Please check the data you are providing to the Controller. Otherwise, you will need to debug the individual provider class you are trying use to connect with.";
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

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
                    "ProviderKey value missing. You need to supply a valid provider key so we know where to redirect the user Eg. providerkey=google.");
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

            // Dictionary keys are case sensitive.
            var key = providerKey.ToLowerInvariant();

            if (_authenticationProviderFactory.AuthenticationProviders.ContainsKey(key))
            {
                TraceSource.TraceVerbose("Found registered provider: " + key);
                provider = _authenticationProviderFactory.AuthenticationProviders[key];
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
    }
}