using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Tracing;

namespace Nancy.SimpleAuthentication
{
    public class SimpleAuthenticationModule : NancyModule
    {
        private const string SessionKeyState = "SimpleAuthentication-StateKey-cf92a651-d638-4ce4-a393-f612d3be4c3a";
        public const string DefaultRedirectRoute = "/authenticate/{providerkey}";
        public const string DefaultCallbackRoute = "/authenticate/callback";
        public const string DefaultUserInformationRoute = "/authenticate/{providerkey}/me/{accesstoken}";

        private readonly IAuthenticationProviderCallback _authenticationProviderCallback;

        private readonly Lazy<ITraceManager> _traceManager = new Lazy<ITraceManager>(() => new TraceManager());
        private readonly WebApplicationService _webApplicationService;
        private string _callbackRoute;
        private string _redirectRoute;
        private string _returnToUrlParameterKey;

        public SimpleAuthenticationModule(IAuthenticationProviderFactory authenticationProviderFactory,
            IAuthenticationProviderCallback authenticationProviderCallback) : this(authenticationProviderFactory,
            authenticationProviderCallback,
            DefaultRedirectRoute,
            DefaultCallbackRoute)
        {    
        }

        public SimpleAuthenticationModule(IAuthenticationProviderFactory authenticationProviderFactory,
            IAuthenticationProviderCallback authenticationProviderCallback,
            string redirectRoute,
            string callbackRoute)
        {
            if (authenticationProviderFactory == null)
            {
                throw new ArgumentNullException("authenticationProviderFactory");
            }

            if (authenticationProviderCallback == null)
            {
                throw new ArgumentNullException("authenticationProviderCallback");
            }

            _authenticationProviderCallback = authenticationProviderCallback;

            RedirectRoute = redirectRoute;
            CallbackRoute = callbackRoute;

            _webApplicationService = new WebApplicationService(authenticationProviderFactory,
                TraceSource,
                CallbackRoute);

            // Define the routes and how they are handled.
            Get[RedirectRoute] = parameters => RedirectToProvider(parameters);
            Get[CallbackRoute, true] = async (parameters, ct) => await AuthenticateCallbackAsync();
            Get[DefaultUserInformationRoute, true] = async (parameters, ct) => await AuthenticateMeAsync(parameters);
        }

        public string RedirectRoute
        {
            get
            {
                return (string.IsNullOrWhiteSpace(_redirectRoute))
                    ? DefaultRedirectRoute
                    : _redirectRoute;
            }
            private set
            {
                if (!string.IsNullOrWhiteSpace(value) &&
                    !value.Contains("{providerkey}"))
                {
                    var errorMessage = string.Format(
                        "The RedirectRoute requires a 'capture segment' with the Nancy route segment variable name '{{providerkey}}'. Eg. '{0}'",
                        DefaultRedirectRoute);
                    throw new ArgumentException(errorMessage, "value");
                }
                _redirectRoute = value;
            }
        }

        public string CallbackRoute
        {
            get
            {
                return (string.IsNullOrWhiteSpace(_callbackRoute))
                    ? DefaultCallbackRoute
                    : _callbackRoute;
            }
            set { _callbackRoute = value; }
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

        private dynamic RedirectToProvider(dynamic parameters)
        {
            dynamic result;

            var providerKey = GetProviderKeyFromRoute(parameters);

            var returnUrl = Request.Query[ReturnToUrlParameterKey];
            var referer = !string.IsNullOrWhiteSpace(Request.Headers.Referrer)
                ? Request.Headers.Referrer
                : null;

            var redirectToProviderData = new RedirectToProviderData(providerKey,
                Request.Url,
                referer,
                returnUrl);

            try
            {
                var redirectionResult = _webApplicationService.RedirectToProvider(redirectToProviderData);

                // Remember any important information for later, after we've returned back here.
                Session[SessionKeyState] = redirectionResult.CacheData;

                // Now redirect :)
                result = Response.AsRedirect(redirectionResult.RedirectUrl.AbsoluteUri);

            }
            catch (AuthenticationException exception)
            {
                result = _authenticationProviderCallback.OnError(this, ErrorType.RedirectToProvider, exception);
            }
            catch (Exception exception)
            {
                var authenticationException = new AuthenticationException(exception.Message);
                result = _authenticationProviderCallback.OnError(this, ErrorType.RedirectToProvider,
                    authenticationException);
            }

            return result;
        }

        private string GetProviderKeyFromRoute(dynamic parameters)
        {
            var providerKey = (string)parameters.providerkey;

            if (!string.IsNullOrEmpty(providerKey))
            {
                return providerKey;
            }

            var sampleUrl = RedirectRoute.Replace("{providerkey}", "google");
            var errorMessage = string.Format(
                "ProviderKey value missing. You need to supply a valid provider key so we know where to redirect the user Eg. ....{0}",
                sampleUrl);
            throw new ArgumentException(errorMessage);
        }

        private static string GetAccessTokenFromRoute(dynamic parameters)
        {
            var accessToken = (string) parameters.accesstoken;
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                return accessToken;
            }

            var errorMessage =
                string.Format(
                    "AccessToken value missing. You need to supply a valud access token in the route so we can attempt to retrieve the details. ie. {0}",
                    DefaultUserInformationRoute);
            throw new ArgumentNullException(errorMessage);
        }

        private async Task<dynamic> AuthenticateCallbackAsync()
        {
            TraceSource.TraceVerbose("Retrieving Cache values - State and RedirectToUrl.");

            dynamic result;
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
                    "No cache data or cached State value was found which generally means that a Cross Site Request Forgery attempt might be made. A 'State' value is generated by the server when a client prepares to redirect to an Authentication Provider and passes that generated state value to that Provider. The provider then passes that state value back, which proves that the client (ie. that's -you-) have actually authenticated against a provider. Otherwise, anyone can just hit the callback Url and impersonate another user, bypassing the authentication stage. So what's the solution: make sure you call the 'RedirectToProvider' endpoint *before* you hit the 'AuthenticateCallbackAsync' callback endpoint.");
            }

            var queryString = new Dictionary<string, string>();
            foreach (var key in Request.Query.Keys)
            {
                queryString.Add(key, Request.Query[key]);
            }

            try
            {

                var authenticateCallbackAsyncData = new AuthenticateCallbackAsyncData(Request.Url,
                    cacheData,
                    queryString);

                result =
                    await
                        _webApplicationService
                            .AuthenticateCallbackAsync<IAuthenticationProviderCallback, INancyModule, dynamic>(
                                _authenticationProviderCallback,
                                this,
                                authenticateCallbackAsyncData);
            }
            catch (AuthenticationException exception)
            {
                result = _authenticationProviderCallback.OnError(this, ErrorType.Callback, exception);
            }
            catch (Exception exception)
            {
                var authenticationException = new AuthenticationException(exception.Message);
                result = _authenticationProviderCallback.OnError(this, ErrorType.Callback, authenticationException);
            }

            return result;
        }

        private async Task<dynamic> AuthenticateMeAsync(dynamic parameters)
        {
            dynamic result;

            var providerKey = GetProviderKeyFromRoute(parameters);

            var accessToken = GetAccessTokenFromRoute(parameters);

            var superAwesomeAccessToken = new AccessToken
            {
                Token = accessToken
            };

            try
            {
                IAuthenticatedClient authenticatedClient = 
                    await _webApplicationService.AuthenticateMeAsync(providerKey, superAwesomeAccessToken);

                result = Response.AsJson(authenticatedClient);
            }
            catch (AuthenticationException exception)
            {
                result = _authenticationProviderCallback.OnError(this, ErrorType.UserInformation, exception);
            }
            catch (Exception exception)
            {
                var authenticationException = new AuthenticationException(exception.Message, exception);
                result = _authenticationProviderCallback.OnError(this, ErrorType.UserInformation,
                    authenticationException);
            }

            return result;
        }
    }
}