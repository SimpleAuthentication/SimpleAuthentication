using System;
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
        private readonly FacebookProvider _facebookProvider;
        private readonly TwitterProvider _twitterProvider;
        public AuthenticationService(FacebookProvider facebookProvider,
            TwitterProvider twitterProvider)
        {
            // Any one of these can be optional.
            _facebookProvider = facebookProvider;
            _twitterProvider = twitterProvider;
        }

        public RedirectResult RedirectToFacebookAuthentication(string state)
        {
            Condition.Requires(state).IsNotNullOrEmpty();
            Condition.Requires(_facebookProvider).IsNotNull();
            return _facebookProvider.RedirectToAuthenticate(state);
        }

        public RedirectResult RedirectToTwitterAuthentication(string callbackUrl)
        {
            Condition.Requires(callbackUrl).IsNotNullOrEmpty();

            return _twitterProvider.RedirectToAuthenticate(callbackUrl);
        }

        public IAuthenticatedClient CheckCallback(HttpRequestBase httpRequestBase, string state)
        {
            Condition.Requires(httpRequestBase).IsNotNull();
            Condition.Requires(state).IsNotNull();

            if (httpRequestBase.Params == null ||
                !httpRequestBase.Params.HasKeys())
            {
                throw new InvalidOperationException(
                    "No request params found - unable to determine from where we authenticated with/against.");
            }

            // Tried to authenticate against Facebook?
            if (_facebookProvider != null)
            {
                var client = TryGetFacebookClient(httpRequestBase.Params, state);
                if (client != null)
                {
                    return client;
                }
            }

            if (_twitterProvider != null)
            {
                var client = TryGetTwitterClient(httpRequestBase.Params);
                if (client != null)
                {
                    return client;
                }
            }

            // Nothing found :(
            return null;
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

                _facebookProvider.RetrieveAccessToken(facebookClient);
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
            _twitterProvider.RetrieveUserInformation(twitterClient, parameters);

            return twitterClient;
        }
    }
}