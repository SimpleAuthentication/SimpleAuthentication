using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CuttingEdge.Conditions;
using WorldDomination.Web.Authentication.Facebook;
using WorldDomination.Web.Authentication.Google;
using WorldDomination.Web.Authentication.Twitter;

namespace WorldDomination.Web.Authentication
{
    public class AuthenticationService
    {
        private IDictionary<Type, IAuthenticationProvider> _authenticationProviders;
        
        public void AddProvider(IAuthenticationProvider authenticationProvider)
        {
            if (_authenticationProviders == null)
            {
                _authenticationProviders = new Dictionary<Type, IAuthenticationProvider>();
            }

            // Does this provider already exist?
            if (_authenticationProviders.ContainsKey(authenticationProvider.GetType()))
            {
                throw new InvalidOperationException("Trying to add a " + authenticationProvider.GetType() + " provider, but one already exists.");
            }

            _authenticationProviders.Add(authenticationProvider.GetType(), authenticationProvider);
        }

        public RedirectResult RedirectToFacebookAuthentication(string state)
        {
            Condition.Requires(state).IsNotNullOrEmpty();

            var facebookProvider = GetAuthenticationProvider<FacebookProvider>();
            return facebookProvider.RedirectToAuthenticate(state);
        }

        public RedirectResult RedirectToTwitterAuthentication(string state)
        {
            Condition.Requires(state).IsNotNullOrEmpty();

            var twitterProvider = GetAuthenticationProvider<TwitterProvider>();
            return twitterProvider.RedirectToAuthenticate(state);
        }

        public RedirectResult RedirectToGoogleAuthentication(string state)
        {
            Condition.Requires(state).IsNotNullOrEmpty();

            var googleProvider = GetAuthenticationProvider<GoogleProvider>();
            return googleProvider.RedirectToAuthenticate(state);
        }

        public IAuthenticatedClient CheckCallback(HttpRequestBase httpRequestBase, string state)
        {
            Condition.Requires(httpRequestBase).IsNotNull();
            Condition.Requires(state).IsNotNull();

            if (httpRequestBase.Params == null ||
                !httpRequestBase.Params.HasKeys())
            {
                return new AuthenticatedClient(ProviderType.Unknown)
                       {
                           ErrorInformation = new ErrorInformation("No request params found - unable to determine from where we authenticated with/against.")
                       };
            }


            IAuthenticatedClient authenticatedClient = null;
            foreach(var provider in _authenticationProviders.Values)
            {
                authenticatedClient = provider.AuthenticateClient(httpRequestBase.Params, state);
                if (authenticatedClient != null)
                {
                    break;
                }
            }

            return authenticatedClient;
        }

        private T GetAuthenticationProvider<T>() where T : class, IAuthenticationProvider
        {
            IAuthenticationProvider authenticationProvider = null;
            if (_authenticationProviders != null)
            {
                _authenticationProviders.TryGetValue(typeof(T), out authenticationProvider);
            }

            if (authenticationProvider == null)
            {
                throw new InvalidOperationException("No " + typeof(T) + " providers have been added.");
            }

            return authenticationProvider as T;
        }
    }
}