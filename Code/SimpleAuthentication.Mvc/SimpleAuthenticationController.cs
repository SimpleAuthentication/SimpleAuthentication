using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Mvc;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Tracing;

namespace SimpleAuthentication.Mvc
{
    public class SimpleAuthenticationController : AsyncController
    {
        private const string SessionKeyState = "SimpleAuthentication-StateKey-427B6ED7-A803-4F18-A396-0084417B548D";
        public const string DefaultRedirectRoute = "authenticate/{providerkey}";
        public const string DefaultCallbackRoute = "authenticate/callback";
        public const string DefaultUserInformationRoute = "authenticate/{providerkey}/me/{accesstoken}";

        private readonly IAuthenticationProviderCallback _authenticationProviderCallback;

        private readonly Lazy<ITraceManager> _traceManager = new Lazy<ITraceManager>(() => new TraceManager());
        private readonly WebApplicationService _webApplicationService;
        private string _callbackRoute;
        private string _redirectRoute;
        private string _userInformationRoute;
        private string _returnToUrlParameterKey;

        public SimpleAuthenticationController(IAuthenticationProviderFactory authenticationProviderFactory,
            IAuthenticationProviderCallback authenticationProviderCallback) : this(authenticationProviderFactory,
                authenticationProviderCallback,
                DefaultRedirectRoute,
                DefaultCallbackRoute,
                DefaultUserInformationRoute)
        {
        }

        public SimpleAuthenticationController(IAuthenticationProviderFactory authenticationProviderFactory,
            IAuthenticationProviderCallback authenticationProviderCallback,
            string redirectRoute,
            string callbackRoute,
            string userInformationRoute)
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
            UserInformationRoute = userInformationRoute;

            _webApplicationService = new WebApplicationService(authenticationProviderFactory,
                TraceSource,
                CallbackRoute);
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

        public string UserInformationRoute
        {
            get
            {
                return (string.IsNullOrWhiteSpace(_userInformationRoute))
                    ? DefaultUserInformationRoute
                    : _userInformationRoute;
            }
            set { _userInformationRoute = value; }
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

        [HttpGet]
        public ActionResult RedirectToProvider(string providerKey)
        {
            RedirectToProviderResult redirectToProviderResult;

            if (string.IsNullOrEmpty(providerKey))
            {
                var sampleUrl = RedirectRoute.Replace("{providerkey}", "google");
                var errorMessage = string.Format(
                    "ProviderKey value missing. You need to supply a valid provider name so we know where to redirect the user Eg. ....{0}",
                    sampleUrl);
                throw new AuthenticationException(errorMessage);
            }

            var returnUrl = Request.QueryString[ReturnToUrlParameterKey];
            var referer = Request.UrlReferrer != null
                ? Request.UrlReferrer.AbsoluteUri
                : null;

            var redirectToProviderData = new RedirectToProviderData(providerKey,
                Request.Url,
                referer,
                returnUrl);

            try
            {
                redirectToProviderResult = _webApplicationService.RedirectToProvider(redirectToProviderData);
            }
            catch (AuthenticationException exception)
            {
                return _authenticationProviderCallback.OnError(this, ErrorType.RedirectToProvider, exception);
            }
            catch (Exception exception)
            {
                var authenticationException = new AuthenticationException(exception.Message);
                return _authenticationProviderCallback.OnError(this, ErrorType.RedirectToProvider,
                    authenticationException);
            }

            // Remember any important information for later, after we've returned back here.
            Session[SessionKeyState] = redirectToProviderResult.CacheData;

            return new RedirectResult(redirectToProviderResult.RedirectUrl.AbsoluteUri);
        }

        [HttpGet]
        public async Task<ActionResult> AuthenticateCallbackAsync()
        {
            TraceSource.TraceVerbose("Retrieving Cache values - State and RedirectToUrl.");

            // NOTE: Does using Session in this asyn action result ... destroy the async perf?
            var cacheData = Session[SessionKeyState] as CacheData;
            Session[SessionKeyState] = null;
            TraceSource.TraceInformation("Previous CacheData: {0}",
                cacheData == null
                    ? "--no cache data--"
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
            foreach (string key in Request.QueryString.Keys)
            {
                queryString.Add(key, Request.QueryString[key]);
            }

            var authenticateCallbackAsyncData = new AuthenticateCallbackAsyncData(Request.Url,
                cacheData,
                queryString);

            return
                await
                    _webApplicationService
                        .AuthenticateCallbackAsync<IAuthenticationProviderCallback, AsyncController, dynamic>(
                            _authenticationProviderCallback,
                            this,
                            authenticateCallbackAsyncData);
        }

        [HttpGet]
        public async Task<ActionResult> AuthenticateMeAsync(string providerKey, string accessToken)
        {
            if (string.IsNullOrEmpty(providerKey))
            {
                var sampleUrl = RedirectRoute.Replace("{providerkey}", "google");
                var errorMessage = string.Format(
                    "ProviderName value missing. You need to supply a valid provider name so we know where to redirect the user Eg. ....{0}",
                    sampleUrl);
                throw new AuthenticationException(errorMessage);
            }

            if (string.IsNullOrEmpty(accessToken))
            {
                var sampleUrl = UserInformationRoute.Replace("{providerkey}", "google").Replace("{accesstoken}", "ABCDE-1234");
                var errorMessage = string.Format(
                    "AccessToken value missing. You need to supply a valid access token so we attempt to retrieve the user information. Eg. ....{0}",
                    sampleUrl);
                throw new AuthenticationException(errorMessage);
            }

            var superAwesomeAccessToken = new AccessToken
            {
                Token = accessToken
            };

            IAuthenticatedClient authenticatedClient;

            try
            {
                authenticatedClient =
                 await _webApplicationService.AuthenticateMeAsync(providerKey, superAwesomeAccessToken);
            }
            catch (AuthenticationException exception)
            {
                return _authenticationProviderCallback.OnError(this, ErrorType.UserInformation, exception);
            }
            catch (Exception exception)
            {
                var authenticationException = new AuthenticationException(exception.Message, exception);
                return _authenticationProviderCallback.OnError(this, ErrorType.UserInformation,
                    authenticationException);
            }

            return Json(authenticatedClient);
        }
    }
}