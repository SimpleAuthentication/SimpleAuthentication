using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Nancy.Responses.Negotiation;
using Nancy.SimpleAuthentication.Caching;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Config;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers;
using SimpleAuthentication.Core.Tracing;

namespace Nancy.SimpleAuthentication
{
    public class SimpleAuthenticationModule : NancyModule
    {
        private const string SessionKeyState = "SimpleAuthentication-StateKey-cf92a651-d638-4ce4-a393-f612d3be4c3a";
        public static string RedirectRoute = "/authenticate/{providerkey}";
        public static string CallbackRoute = "/authenticate/callback";
        
        private readonly Lazy<ITraceManager> _traceManager = new Lazy<ITraceManager>(() => new TraceManager());
        private readonly AuthenticationProviderFactory _authenticationProviderFactory;
        private readonly IAuthenticationCallbackProvider _callbackProvider;
        private string _returnToUrlParameterKey;

        public SimpleAuthenticationModule(IAuthenticationCallbackProvider callbackProvider,
            IConfigService configService,
            ICache cache = null)
        {
            if (callbackProvider == null)
            {
                throw new ArgumentNullException("callbackProvider");
            }

            if (configService == null)
            {
                throw new ArgumentNullException("configService");
            }

            _callbackProvider = callbackProvider;
            _authenticationProviderFactory = new AuthenticationProviderFactory(configService);

            // Optionals:
            Cache = cache;

            // Define the routes and how they are handled.
            Get[RedirectRoute, true] = async (parameters, ct) => await RedirectToProviderAsync(parameters);
            Get[CallbackRoute, true] = async (x, ct) => await AuthenticateCallbackAsync();

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

        private async Task<Response> RedirectToProviderAsync(dynamic parameters)
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
            var callbackUri = GenerateCallbackUri(provider.Name);

            // Determine where we need to redirect to.
            var redirectToAuthenticateSettings = await provider.GetRedirectToAuthenticateSettingsAsync(callbackUri);
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
            Cache[SessionKeyState] = cacheData;

            // Now redirect :)
            return Response.AsRedirect(redirectToAuthenticateSettings.RedirectUri.AbsoluteUri);
        }

        private async Task<dynamic> AuthenticateCallbackAsync()
        {
            TraceSource.TraceVerbose("Retrieving Cache values - State and RedirectToUrl.");
            var cacheData = Cache[SessionKeyState];
            TraceSource.TraceInformation("Previous CacheData: {0}",
                cacheData == null
                    ? "--no cache data"
                    : cacheData.ToString());

            // If we don't have some previous state value cached, then it's possible we're trying to just
            // hack into the callback directly. ie. CSRF.
            if (cacheData == null ||
                string.IsNullOrWhiteSpace(cacheData.State))
            {
                throw new AuthenticationException("No cache data or cached State value was found which generally means that a Cross Site Request Forgery attempt might be made. A 'State' value is generated by the server when a client prepares to rediect to an Authentication Provider and passes that generated state value to that Provider. The provider then passes that state value back, which proves that the client (ie. that's -you-) have actually authenticated against a provider. Otherwise, anyone can just hit the callback Url and impersonate another user, bypassing the authentication stage. So what's the solution: make sure you call the 'RedirectToProvider' endpoint *before* you hit the 'AuthenticateCallbackAsync' callback endpoint.");
            }

            // Lets now start to setup the view model.
            var model = new AuthenticateCallbackResult();

            #region Retrieve the User Information

            try
            {
                // Which provider did we just authenticate with?
                var provider = GetAuthenticationProvider(cacheData.ProviderKey);

                // Where do we return to, after we've authenticated?
                var callbackUri = GenerateCallbackUri(provider.Name);

                // Nancy.Request Nancy.DynamicDictionary
                var queryString = new Dictionary<string, string>();
                foreach (var key in Request.Query.Keys)
                {
                    queryString.Add(key, Request.Query[key]);
                }

                // Grab the user information.
                model.AuthenticatedClient = await provider.AuthenticateClientAsync(queryString, 
                    cacheData.State, 
                    callbackUri);
            }
            catch (Exception exception)
            {
                TraceSource.TraceError(exception.Message);
                return _callbackProvider.OnRedirectToAuthenticationProviderError(this, exception);
            }

            #endregion

            // Do we have an optional redirect resource? Usually a previous referer?
            if (!string.IsNullOrWhiteSpace(cacheData.ReturnUrl))
            {
                TraceSource.TraceVerbose("Found return url: " + cacheData.ReturnUrl);
                model.ReturnUrl = cacheData.ReturnUrl;
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
            return CreateCallBackUri(providerName, Request.Url, CallbackRoute);
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

        private static Uri CreateCallBackUri(string providerKey,
            Uri requestUrl,
            string path)
        {
            if (String.IsNullOrWhiteSpace(providerKey))
            {
                throw new ArgumentNullException("providerKey");
            }

            if (requestUrl == null)
            {
                throw new ArgumentNullException("requestUrl");
            }

            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            var builder = new UriBuilder(requestUrl)
            {
                Path = path,
                Query = "providerkey=" + providerKey.ToLowerInvariant()
            };

            // Don't include port 80/443 in the Uri.
            if (builder.Uri.IsDefaultPort)
            {
                builder.Port = -1;
            }

            return builder.Uri;
        }
    }
}