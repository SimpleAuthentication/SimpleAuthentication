using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Config;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers;
using SimpleAuthentication.Core.Tracing;

namespace Nancy.SimpleAuthentication
{
    [Obsolete]
    public class SimpleAuthenticationModuleOriginal : NancyModule
    {
        private const string SessionKeyState = "SimpleAuthentication-StateKey-cf92a651-d638-4ce4-a393-f612d3be4c3a";
        public static string RedirectRoute = "/authenticatedd/{providerkey}";
        public static string CallbackRoute = "/authenticatedd/callback";

        private readonly AuthenticationProviderFactory _authenticationProviderFactory;
        private readonly IAuthenticationProviderCallback _providerCallback;
        private readonly Lazy<ITraceManager> _traceManager = new Lazy<ITraceManager>(() => new TraceManager());
        private string _returnToUrlParameterKey;

        public SimpleAuthenticationModuleOriginal(IAuthenticationProviderCallback providerCallback,
            IConfigService configService)
        {
            if (providerCallback == null)
            {
                throw new ArgumentNullException("providerCallback");
            }

            if (configService == null)
            {
                throw new ArgumentNullException("configService");
            }

            _providerCallback = providerCallback;
            _authenticationProviderFactory = new AuthenticationProviderFactory(configService);

            // Define the routes and how they are handled.
            Get[RedirectRoute] = parameters => RedirectToProvider(parameters);
            Get[CallbackRoute, true] = async (x, ct) => await AuthenticateCallbackAsync();
        }

        public string ReturnToUrlParameterKey
        {
            get { return (string.IsNullOrEmpty(_returnToUrlParameterKey) ? "returnUrl" : _returnToUrlParameterKey); }
            set { _returnToUrlParameterKey = value; }
        }

        public ITraceManager TraceManager
        {
            get { return _traceManager.Value; }
        }

        private TraceSource TraceSource
        {
            get { return TraceManager["Nancy.SimpleAuthentication.SimpleAuthenticationModule"]; }
        }

        private Response RedirectToProvider(dynamic parameters)
        {
            var providerKey = (string) parameters.providerkey;

            if (string.IsNullOrEmpty(providerKey))
            {
                throw new ArgumentException(
                    "ProviderKey value missing. You need to supply a valid provider key so we know where to redirect the user Eg. google.");
            }

            // Grab the Provider.
            var provider = GetAuthenticationProvider(providerKey);

            // Where do we return to, after we've authenticated?
            var callbackUri = GenerateCallbackUri();

            // Determine where we need to redirect to.
            var redirectToAuthenticateSettings = provider.GetRedirectToAuthenticateSettings(callbackUri);
            if (redirectToAuthenticateSettings == null)
            {
                // We failed to determine where to go. A classic example of this is with OpenId and a bad OpenId endpoint.
                const string errorMessage =
                    "No redirect to authencate settings retrieved. This means we don't know where to go. A classic example of this is with OpenId and a bad OpenId endpoint. Please check the data you are providing to the Controller. Otherwise, you will need to debug the individual provider class you are trying use to connect with.";
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            // Remember any important information for later, after we've returned back here.
            var cacheData = new CacheData(providerKey,
                redirectToAuthenticateSettings.State,
                DetermineReturnUrl());
            Session[SessionKeyState] = cacheData;

            // Now redirect :)
            return Response.AsRedirect(redirectToAuthenticateSettings.RedirectUri.AbsoluteUri);
        }

        private async Task<dynamic> AuthenticateCallbackAsync()
        {
            TraceSource.TraceVerbose("Retrieving Cache values - State and RedirectToUrl.");
            
            var cacheData = Session[SessionKeyState] as CacheData;
            Session[SessionKeyState] = null;
            TraceSource.TraceInformation("Previous CacheData: {0}",
                cacheData == null
                    ? "--no cache data"
                    : cacheData.ToString());

            // If we don't have some previous state value cached, then it's possible we're trying to just
            // hack into the callback directly. ie. CSRF.
            if (cacheData == null ||
                string.IsNullOrWhiteSpace(cacheData.State))
            {
                throw new AuthenticationException(
                    "No cache data or cached State value was found which generally means that a Cross Site Request Forgery attempt might be made. A 'State' value is generated by the server when a client prepares to rediect to an Authentication Provider and passes that generated state value to that Provider. The provider then passes that state value back, which proves that the client (ie. that's -you-) have actually authenticated against a provider. Otherwise, anyone can just hit the callback Url and impersonate another user, bypassing the authentication stage. So what's the solution: make sure you call the 'RedirectToProvider' endpoint *before* you hit the 'AuthenticateCallbackAsync' callback endpoint.");
            }

            dynamic result;

            try
            {
                var model = await RetrieveUserInformation(cacheData.ProviderKey,
                    cacheData.State);

                // Do we have an optional redirect resource? Usually a previous referer?
                if (!string.IsNullOrWhiteSpace(cacheData.ReturnUrl))
                {
                    TraceSource.TraceVerbose("Found return url: " + cacheData.ReturnUrl);
                    model.ReturnUrl = cacheData.ReturnUrl;
                }

                // Finally! We can hand over the logic to the consumer to do whatever they want.
                TraceSource.TraceVerbose("About to execute your custom callback provider logic.");
                result = _providerCallback.Process(model);
            }
            catch (Exception exception)
            {
                TraceSource.TraceError(exception.Message);
                result = _providerCallback.OnRedirectToAuthenticationProviderError(exception);
            }

            return result;
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

        private Uri GenerateCallbackUri(string query = null)
        {
            // Optional, but UriBuilder doesn't like a null query value.
            if (query == null)
            {
                query = string.Empty;
            }

            var builder = new UriBuilder((System.Uri)Request.Url)
            {
                Path = CallbackRoute,
                Query = query
            };

            // Don't include port 80/443 in the Uri.
            if (builder.Uri.IsDefaultPort)
            {
                builder.Port = -1;
            }

            return builder.Uri;
        }

        private string DetermineReturnUrl()
        {
            var returnUrl = Request.Query[ReturnToUrlParameterKey];

            return !string.IsNullOrEmpty(returnUrl)
                ? returnUrl
                : !string.IsNullOrWhiteSpace(Request.Headers.Referrer)
                    ? Request.Headers.Referrer
                    : null;
        }

        private async Task<AuthenticateCallbackResult> RetrieveUserInformation(string providerKey,
            string state)
        {
            if (string.IsNullOrWhiteSpace(providerKey))
            {
                throw new ArgumentNullException("providerKey");
            }

            if (string.IsNullOrWhiteSpace(state))
            {
                throw new ArgumentNullException("state");
            }

            // Which provider did we just authenticate with?
            var provider = GetAuthenticationProvider(providerKey);

            // CRAZY?? Yeah, we still need the callback uri (even though we're -IN- the callback
            // because some providers use that as some security check or something. Urgh...
            var callbackUri = GenerateCallbackUri();

            // Nancy.Request Nancy.DynamicDictionary
            var queryString = new Dictionary<string, string>();
            foreach (var key in Request.Query.Keys)
            {
                queryString.Add(key, Request.Query[key]);
            }

            // Grab the user information.
            var model = new AuthenticateCallbackResult
            {
                AuthenticatedClient = await provider.AuthenticateClientAsync(queryString,
                    state,
                    callbackUri)
            };

            return model;
        }
    }
}