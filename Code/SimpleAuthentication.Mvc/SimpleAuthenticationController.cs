using System;
using System.Diagnostics;
using System.Web.Mvc;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers;
using SimpleAuthentication.Core.Tracing;
using SimpleAuthentication.Mvc.Caching;

namespace SimpleAuthentication.Mvc
{
    public class SimpleAuthenticationController : Controller
    {
        private const string SessionKeyState = "SimpleAuthentication.Session.StateToken";
        private const string SessionKeyReturnToUrl = "SimpleAuthentication.Session.RedirectToUrl";
        private const string SessionKeyRedirectToProviderUrl = "SimpleAuthentication.Session.";
        private readonly Lazy<ITraceManager> _traceManager = new Lazy<ITraceManager>(() => new TraceManager());

        private readonly AuthenticationProviderFactory _authenticationProviderFactory;
        private readonly IAuthenticationCallbackProvider _callbackProvider;
        private string _returnToUrlParameterKey;
        private ICache _cache;

        public SimpleAuthenticationController()
        {
            // We don't have the bare minimum requirements - so lets help the developer.
            const string errorMessage =
                "Please use the SimpleAuthenticationController(IAuthenticationCallbackProvider)" +
                " constructor. This is because we need to know what to do AFTER we've retrieved the" +
                " User Information from the Authentication Provider. This is normally done by" +
                " leveraging the ASP.NET MVC Dependency Resolver. So please pick a form of Dependency" +
                " Injection and inject an IAuthenticationCallbackProvider into this constructor. For" +
                " more information about ASP.NET MVC's Dependency Resolver:" +
                " http://www.asp.net/mvc/tutorials/hands-on-labs/aspnet-mvc-4-dependency-injection";

            TraceSource.TraceError(errorMessage);

            throw new NotImplementedException(errorMessage);
        }

        public SimpleAuthenticationController(IAuthenticationCallbackProvider callbackProvider,
            ICache cache)
        {
            if (callbackProvider == null)
            {
                throw new ArgumentNullException("callbackProvider");
            }

            _callbackProvider = callbackProvider;
            _cache = cache; // Can be null / not provided.

            _authenticationProviderFactory = new AuthenticationProviderFactory();
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // TODO: We need to be smarter here - if 'something' was 'provided', use that instead.
            if (_cache == null)
            {
                _cache = new SessionCache();
            }

            _cache.Initialize();
        }

        public string ReturnToUrlParameterKey
        {
            get { return (string.IsNullOrEmpty(_returnToUrlParameterKey) ? "returnUrl" : _returnToUrlParameterKey); }
            set { _returnToUrlParameterKey = value; }
        }

        public ITraceManager TraceManager { get { return _traceManager.Value; } }

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
            _cache[SessionKeyState] = redirectToAuthenticateSettings.State;
            _cache[SessionKeyReturnToUrl] = DetermineReturnUrl(inputModel.ReturnUrl);
            _cache[SessionKeyRedirectToProviderUrl] = redirectToAuthenticateSettings.RedirectUri.AbsoluteUri;

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

            var previousRedirectUrl = string.IsNullOrEmpty(_cache[SessionKeyRedirectToProviderUrl])
                                          ? "N.A."
                                          : _cache[SessionKeyRedirectToProviderUrl];
            TraceSource.TraceInformation("Previous Redirect Url: " + previousRedirectUrl);

            #region Deserialize Tokens, etc.

            // Retrieve any (previously) serialized access token stuff. (eg. public/private keys and state).
            // TODO: Check if this is an access token or an auth token thingy-thing.
            TraceSource.TraceVerbose("Retrieving (local serializaed) AccessToken, State and RedirectToUrl.");
            var state = _cache[SessionKeyState];
            var redirectToUrl = _cache[SessionKeyReturnToUrl];

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
                model.ReturnUrl = redirectToUrl;
            }

            // Finally! We can hand over the logic to the consumer to do whatever they want.
            TraceSource.TraceVerbose("About to execute your custom callback provider logic.");
            return _callbackProvider.Process(HttpContext, model);
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

        private string DetermineReturnUrl(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                // Maybe they have used another parameter key name, different to the input model?
                returnUrl = Request.Params[ReturnToUrlParameterKey];
            }

            return string.IsNullOrEmpty(returnUrl)
                       ? Request.UrlReferrer == null ? string.Empty : Request.UrlReferrer.AbsoluteUri
                       : returnUrl;
        }
    }
}