using System;
using System.Collections.Specialized;
using System.Diagnostics;
using Nancy.Responses.Negotiation;
using Nancy.SimpleAuthentication.Caching;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers;
using SimpleAuthentication.Core.Tracing;

namespace Nancy.SimpleAuthentication
{
    public class SimpleAuthenticationModule : NancyModule
    {
        private const string SessionKeyState = "SimpleAuthentication-StateKey-cf92a651-d638-4ce4-a393-f612d3be4c3a";
        private const string SessionKeyRedirectToUrl = "SimpleAuthentication-RedirectUrlKey-cf92a651-d638-4ce4-a393-f612d3be4c3a";
        private const string SessionKeyRedirectToProviderUrl = "SimpleAuthentication-RedirectToProviderUrlKey-cf92a651-d638-4ce4-a393-f612d3be4c3a";
        public static string RedirectRoute = "/authentication/redirect/{providerkey}";
        public static string CallbackRoute = "/authentication/authenticatecallback";
        
        private readonly Lazy<ITraceManager> _traceManager = new Lazy<ITraceManager>(() => new TraceManager());
        private readonly AuthenticationProviderFactory _authenticationProviderFactory;
        private readonly IAuthenticationCallbackProvider _callbackProvider;
        private string _returnToUrlParameterKey;

        public SimpleAuthenticationModule(IAuthenticationCallbackProvider callbackProvider)
        {
            _callbackProvider = callbackProvider;
            _authenticationProviderFactory = new AuthenticationProviderFactory();

            // Define the routes and how they are handled.
            Get[RedirectRoute] = parameters => RedirectToProvider(parameters);
            Post[RedirectRoute] = parameters => RedirectToProvider(parameters);
            Get[CallbackRoute] = parameters => AuthenticateCallback();

            // If no Cache type is provided, we'll use a Session as the default.
            Before += context =>
            {
                if (Cache == null)
                {
                    Cache = new SessionCache(context.Request.Session);
                }

                return null;
            };
        }

        public string ReturnToUrlParameterKey
        {
            get { return (string.IsNullOrEmpty(_returnToUrlParameterKey) ? "returnUrl" : _returnToUrlParameterKey); }
            set { _returnToUrlParameterKey = value; }
        }

        public ICache Cache { get; set; }

        public ITraceManager TraceManager { get { return _traceManager.Value; } }

        private TraceSource TraceSource
        {
            get { return TraceManager["Nancy.SimpleAuthentication.SimpleAuthenticationModule"]; }
        }

        private Response RedirectToProvider(dynamic parameters)
        {
            #region Input Model Validation

            var providerKey = (string) parameters.providerkey;

            if (string.IsNullOrEmpty(providerKey))
            {
                throw new ArgumentException(
                    "ProviderKey value missing. You need to supply a valid provider key so we know where to redirect the user Eg. google.");
            }

            var identityValue = (string) parameters.identifier;
            Uri identifier = null;
            if (!string.IsNullOrEmpty(identityValue) &&
                !Uri.TryCreate(identityValue, UriKind.Absolute, out identifier))
            {
                throw new ArgumentException("The Identifier value [" + identityValue +
                                            "] is not a valid Uri. Please fix it up. eg. http://goto.some.website/authenticate/");
            }

            #endregion

            // Grab the Provider.
            var provider = GetAuthenticationProvider(providerKey);

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
            Cache[SessionKeyState] = redirectToAuthenticateSettings.State;
            Cache[SessionKeyRedirectToUrl] = DetermineReturnUrl();
            Cache[SessionKeyRedirectToProviderUrl] = redirectToAuthenticateSettings.RedirectUri.AbsoluteUri;

            // Now redirect :)
            return Response.AsRedirect(redirectToAuthenticateSettings.RedirectUri.AbsoluteUri);
        }

        private dynamic AuthenticateCallback()
        {
            var providerKey = (string) Request.Query.providerKey;
            if (string.IsNullOrEmpty(providerKey))
            {
                throw new ArgumentException(
                    "ProviderKey value missing. You need to supply a valid provider key so we know where to redirect the user Eg. providerkey=google.");
            }

            var previousRedirectUrl = string.IsNullOrEmpty((string) Cache[SessionKeyRedirectToProviderUrl])
                                          ? "N.A."
                                          : (string) Cache[SessionKeyRedirectToProviderUrl];
            TraceSource.TraceInformation("Previous Redirect Url: " + previousRedirectUrl);

            #region Deserialize Tokens, etc.

            // Retrieve any (previously) serialized access token stuff. (eg. public/private keys and state).
            // TODO: Check if this is an access token or an auth token thingy-thing.
            TraceSource.TraceVerbose("Retrieving (local serializaed) AccessToken, State and RedirectToUrl.");
            var state = Cache[SessionKeyState] as string;
            var redirectToUrl = Cache[SessionKeyRedirectToUrl] as string;

            #endregion

            // Lets now start to setup the view model.
            var model = new AuthenticateCallbackData();

            #region Retrieve the User Information

            try
            {
                // Which provider did we just authenticate with?
                var provider = GetAuthenticationProvider(providerKey);

                // Where do we return to, after we've authenticated?
                var callbackUri = GenerateCallbackUri(provider.Name);

                var queryString = new NameValueCollection();
                foreach (var key in Request.Query.Keys)
                {
                    queryString.Add(key, Request.Query[key]);
                }

                // Grab the user information.
                model.AuthenticatedClient = provider.AuthenticateClient(queryString, state, callbackUri);
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
            return _callbackProvider.Process(this, model);
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
            return SystemHelpers.CreateCallBackUri(providerName, Request.Url, CallbackRoute);
        }

        private string DetermineReturnUrl()
        {
            var returnUrl = Request.Query[ReturnToUrlParameterKey];

            return string.IsNullOrEmpty(returnUrl)
                       ? Request.Headers.Referrer
                       : returnUrl;
        }
    }
}