using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using SimpleAuthentication.Core.Tracing;

namespace SimpleAuthentication.Core.Providers
{
    public abstract class BaseProvider : IAuthenticationProvider
    {
        private string _stateKey;

        protected BaseProvider(string name, string authenticationType)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrWhiteSpace(authenticationType))
            {
                throw new ArgumentNullException("authenticationType");
            }

            Name = name;
            AuthenticationType = authenticationType;

            TraceManager = new Lazy<ITraceManager>(() => new TraceManager()).Value;
        }

        #region IAuthentionProvider Implementation

        public string StateKey
        {
            get
            {
                return (string.IsNullOrWhiteSpace(_stateKey)
                    ? "state"
                    : _stateKey);
            }
            set { _stateKey = value; }
        }

        public string Name { get; private set; }

        public string AuthenticationType { get; private set; }

        public abstract Uri AuthenticateRedirectionUrl { get;}

        public AccessToken AccessToken { get; set; }

        public abstract Task<IAuthenticatedClient> AuthenticateClientAsync(NameValueCollection queryStringParameters,
            string state,
            Uri callbackUrl);

        public ITraceManager TraceManager { set; protected get; }

        public abstract RedirectToAuthenticateSettings GetRedirectToAuthenticateSettings(Uri requestUrl);

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