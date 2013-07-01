using System;
using System.Collections.Specialized;
using System.Diagnostics;
using WorldDomination.Web.Authentication.Tracing;

namespace WorldDomination.Web.Authentication.Providers
{
    public abstract class BaseProvider : IAuthenticationProvider
    {
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

        public string Name { get; private set; }

        public abstract IAuthenticationServiceSettings DefaultAuthenticationServiceSettings { get; }
        public abstract Uri RedirectToAuthenticate(IAuthenticationServiceSettings authenticationServiceSettings);

        public abstract IAuthenticatedClient AuthenticateClient(
            IAuthenticationServiceSettings authenticationServiceSettings,
            NameValueCollection queryStringParameters);

        public ITraceManager TraceManager { set; protected get; }

        #endregion

        protected TraceSource TraceSource { get { return TraceManager["WD.Web.Authentication.Providers." + Name]; } }
    }
}