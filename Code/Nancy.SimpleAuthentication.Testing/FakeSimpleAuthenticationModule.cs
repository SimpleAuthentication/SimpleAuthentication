using System;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;

namespace Nancy.SimpleAuthentication.Testing
{
    public class FakeSimpleAuthenticationModule : NancyModule
    {
        private string _redirectRoute;
        private string _callbackRoute;
        private readonly AuthenticateCallbackResult _authenticateCallbackResult;
        private readonly AuthenticationException _authenticationException;
        private readonly IAuthenticationProviderCallback _authenticationProviderCallback;
        private string _redirectRouteResultLocation;

        public FakeSimpleAuthenticationModule(IAuthenticationProviderCallback authenticationProviderCallback,
            AuthenticateCallbackResult authenticateCallbackResult)
        {
            if (authenticationProviderCallback == null)
            {
                throw new ArgumentNullException("authenticationProviderCallback");
            }

            if (authenticateCallbackResult == null)
            {
                throw new ArgumentNullException("authenticateCallbackResult");
            }

            _authenticationProviderCallback = authenticationProviderCallback;
            _authenticateCallbackResult = authenticateCallbackResult;

            InitializeModule();
        }

        public FakeSimpleAuthenticationModule(IAuthenticationProviderCallback authenticationProviderCallback,
            AuthenticationException authenticationException)
        {
            if (authenticationProviderCallback == null)
            {
                throw new ArgumentNullException("authenticationProviderCallback");
            }

            if (authenticationException == null)
            {
                throw new ArgumentNullException("authenticationException");
            }

            _authenticationProviderCallback = authenticationProviderCallback;
            _authenticationException = authenticationException;

            InitializeModule();
        }

        public string RedirectRoute
        {
            get { return _redirectRoute; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException("value");
                }
                _redirectRoute = value;
            }
        }

        public string CallbackRoute
        {
            get { return _callbackRoute; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException("value");
                }
                _callbackRoute = value;
            }
        }

        public string RedirectRouteResultLocation
        {
            get { return _redirectRouteResultLocation; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException("value");
                }
                _redirectRouteResultLocation = value;
            }
        }

        private void InitializeModule()
        {
            RedirectRoute = SimpleAuthenticationModule.DefaultRedirectRoute;
            CallbackRoute = SimpleAuthenticationModule.DefaultCallbackRoute;
            RedirectRouteResultLocation = "http://www.someProvider.com/oauth/authenticate";

            Get[RedirectRoute] = _ => RedirectToProvider();
            Get[CallbackRoute] = _ => AuthenticateCallback();
        }

        private dynamic RedirectToProvider()
        {
            return Response.AsRedirect(_redirectRouteResultLocation);
        }

        private dynamic AuthenticateCallback()
        {
            return _authenticationException != null
                ? _authenticationProviderCallback.OnError(this, ErrorType.RedirectToProvider, _authenticationException)
                : _authenticationProviderCallback.Process(this, _authenticateCallbackResult);
        }
    }
}