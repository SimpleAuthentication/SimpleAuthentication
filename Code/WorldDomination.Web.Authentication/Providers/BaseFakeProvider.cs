using System;
using System.Collections.Specialized;

namespace WorldDomination.Web.Authentication.Providers
{
    public abstract class BaseFakeProvider : BaseProvider, IFakeAuthenticationProvider
    {
        public abstract string Name { get; }

        public string Scope
        {
            get { return "--not used--"; }
            set { throw new NotImplementedException(); }
        }

        public string PublicKey
        {
            get { return "--not used--"; }
        }

        public string PrivateKey
        {
            get { return "--not used--"; }
        }

        public abstract IAuthenticationServiceSettings DefaultAuthenticationServiceSettings { get; }
        public abstract Uri RedirectToAuthenticate(IAuthenticationServiceSettings authenticationServiceSettings);

        public abstract IAuthenticatedClient AuthenticateClient(
            IAuthenticationServiceSettings authenticationServiceSettings,
            NameValueCollection queryStringParameters);

        public abstract string RedirectToAuthenticateExceptionMessage { get; set; }
        public abstract UserInformation UserInformation { get; set; }
        public abstract string AuthenticateClientExceptionMessage { get; set; }
    }
}