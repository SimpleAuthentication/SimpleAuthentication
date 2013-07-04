using System;
using System.Collections.Specialized;
using System.Diagnostics;
using SimpleAuthentication.Tracing;

namespace SimpleAuthentication.Providers
{
    public abstract class BaseProvider : IAuthenticationProvider
    {
        private string _stateKey;

        protected BaseProvider(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            Name = name;

            TraceManager = new Lazy<ITraceManager>(() => new TraceManager()).Value;
        }

        #region IAuthentionProvider Implementation

        public string StateKey
        {
            get { return (string.IsNullOrEmpty(_stateKey) ? "state" : _stateKey); }
            set { _stateKey = value; }
        }

        public string Name { get; private set; }

        public Uri AuthenticateRedirectionUrl { get; set; }

        public AccessToken AccessToken { get; set; }

        public abstract RedirectToAuthenticateSettings RedirectToAuthenticate(Uri requestUri);

        public abstract IAuthenticatedClient AuthenticateClient(NameValueCollection queryStringParameters,
                                                                string state,
                                                                Uri callbackUri);

        public ITraceManager TraceManager { set; protected get; }

        #endregion

        protected TraceSource TraceSource
        {
            get { return TraceManager["SimpleAuthentication.Providers." + Name]; }
        }

        protected string GetQuerystringState(string state)
        {
            if (string.IsNullOrWhiteSpace(state))
            {
                throw new ArgumentNullException("state");
            }

            return string.Format("&{0}={1}", StateKey, state);
        }
    }
}