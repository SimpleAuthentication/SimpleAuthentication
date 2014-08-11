using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SimpleAuthentication.Core.Tracing;

namespace SimpleAuthentication.Core.Providers
{
    public abstract class BaseProvider : IAuthenticationProvider
    {
        private string _stateKey;

        protected BaseProvider(string name, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            Name = name;

            // Optional.
            Description = description;

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

        public string Description { get; private set; }

        public abstract Uri AuthenticateRedirectionUrl { get;}

        public AccessToken AccessToken { get; set; }

        public abstract Task<IAuthenticatedClient> AuthenticateClientAsync(IDictionary<string, string> queryString,
            string state,
            Uri callbackUrl);

        public ITraceManager TraceManager { set; protected get; }

        public abstract RedirectToAuthenticateSettings GetRedirectToAuthenticateSettings(Uri requestUrl);

        #endregion

        protected TraceSource TraceSource
        {
            get { return TraceManager["SimpleAuthentication.Providers." + Name]; }
        }

        protected KeyValuePair<string, string> GetQuerystringStateAsKeyValuePair(string state)
        {
            if (string.IsNullOrWhiteSpace(state))
            {
                throw new ArgumentNullException("state");
            }

            return new KeyValuePair<string, string>(StateKey, state);
        }
    }
}