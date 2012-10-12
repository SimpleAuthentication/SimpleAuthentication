using System;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CuttingEdge.Conditions;
using WorldDomination.Web.Authentication.Facebook;

namespace WorldDomination.Web.Authentication
{
    public class AuthenticationService
    {
        private FacebookProvider FacebookProvider { get; set; }

        public AuthenticationService(FacebookProvider facebookProvider)
        {
            // Any one of these can be optional.
            FacebookProvider = facebookProvider;
        }

        public RedirectResult RedirectToFacebookAuthentication(string state)
        {
            Condition.Requires(state).IsNotNullOrEmpty();
            Condition.Requires(FacebookProvider).IsNotNull();
            return FacebookProvider.RedirectToAuthenticate(state);
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
            if (FacebookProvider != null)
            {
                var client = TryGetFacebook(httpRequestBase.Params, state);
                if (client != null)
                {
                    return client;
                }
            }

            // Nothing found :(
            return null;
        }

        private FacebookClient TryGetFacebook(NameValueCollection parameters, string existingState)
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

                FacebookProvider.RetrieveAccessToken(facebookClient);
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
    }
}