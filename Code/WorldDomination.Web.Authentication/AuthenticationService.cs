using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CuttingEdge.Conditions;
using WorldDomination.Web.Authentication.Facebook;
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

        public RedirectResult RedirectToTwitterAuthentication(string callbackUrl)
        {
            Condition.Requires(callbackUrl).IsNotNullOrEmpty();

            var twitterProvider = GetAuthenticationProvider<TwitterProvider>();
            return twitterProvider.RedirectToAuthenticate(callbackUrl);
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

            //// Tried to authenticate against Facebook?
            //var facebookProvider = GetAuthenticationProvider<FacebookProvider>();
            //if (facebookProvider != null)
            //{
            //    var client = TryGetFacebookClient(httpRequestBase.Params, state);
            //    if (client != null)
            //    {
            //        return client;
            //    }
            //}

            //var twitterProvider = GetAuthenticationProvider<TwitterProvider>();
            //if (twitterProvider != null)
            //{
            //    var client = TryGetTwitterClient(httpRequestBase.Params);
            //    if (client != null)
            //    {
            //        return client;
            //    }
            //}

            //// Nothing found :(
            //return null;
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

        private FacebookClient TryGetFacebookClient(NameValueCollection parameters, string existingState)
        {
            Condition.Requires(parameters).IsNotNull().IsLongerThan(0);
            Condition.Requires(existingState).IsNotNull();

            // Is this a facebook callback?
            var code = parameters["code"];
            var state = parameters["state"];

            if (!string.IsNullOrEmpty(code) &&
                !string.IsNullOrEmpty(state))
            {
                // CSRF (state) check.
                if (state != existingState)
                {
                    throw new InvalidOperationException("The states do not match. It's possible that you may be a victim of a CSRF.");
                }

                // Now ask Facebook for a Token.
                var facebookClient = new FacebookClient
                           {
                               Code = code,
                               State = state
                           };

                var facebookProvider = GetAuthenticationProvider<FacebookProvider>();
                facebookProvider.RetrieveUserInformation(facebookClient);
                return facebookClient;
            }

            // Maybe we have an error?
            var errorReason = parameters["error_reason"];
            var error = parameters["error"];
            var errorDescription = parameters["error_description"];
            if (!string.IsNullOrEmpty(errorReason) &&
                !string.IsNullOrEmpty(error) &&
                !string.IsNullOrEmpty(errorDescription))
            {
                return new FacebookClient
                           {
                               ErrorReason = errorReason,
                               Error = error,
                               ErrorDescription = errorDescription
                           };
            }

            // Nope. Didn't auth with Facebook.
            return null;
        }

        private TwitterClient TryGetTwitterClient(NameValueCollection parameters)
        {
            Condition.Requires(parameters).IsNotNull();

            // Client didn't want to accept the Twitter app policy.
            var denied = parameters["denied"];
            if (!string.IsNullOrEmpty(denied))
            {
                return new TwitterClient {DeniedToken = denied};
            }
            
            // Now ask Twitter for some user information.
            var twitterClient = new TwitterClient();
            var twitterProvider = GetAuthenticationProvider<TwitterProvider>();
            twitterProvider.RetrieveUserInformation(twitterClient, parameters);

            return twitterClient;
        }
    }
}