using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using CuttingEdge.Conditions;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Contrib;

namespace WorldDomination.Web.Authentication.Google
{
    // REFERENCE: https://developers.google.com/accounts/docs/OAuth2Login

    public class GoogleProvider : IAuthenticationProvider
    {
                private const string OAuthTokenKey = "oauth_token";
        private const string OAuthTokenSecretKey = "oauth_token_secret";
        private const string OAuthVerifierKey = "oauth_verifier";

        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly Uri _redirectUri;
        private readonly IList<string> _scope;
        private readonly IRestClient _restClient;

        public GoogleProvider(string consumerKey, string consumerSecret, Uri redirectUri) : this(consumerKey, consumerSecret, redirectUri, null, null)
        {
        }

        public GoogleProvider(string consumerKey, string consumerSecret, Uri redirectUri, IList<string> scope, IRestClient restClient)
        {
            Condition.Requires(consumerKey).IsNotNullOrEmpty();
            Condition.Requires(consumerSecret).IsNotNullOrEmpty();
            Condition.Requires(redirectUri).IsNotNull();

            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _redirectUri = redirectUri;

            // Optionals.
            _scope = scope == null ||
                     scope.Count <= 0
                         ? new List<string>
                           {
                               "https://www.googleapis.com/auth/userinfo.profile",
                               "https://www.googleapis.com/auth/userinfo.email"
                           }
                         : scope;
            _restClient = restClient ?? new RestClient("https://accounts.google.com/o/oauth2/auth");
        }

        private IDictionary<string, string> RetrieveRequestToken(string state)
        {
            Condition.Requires(state).IsNotNullOrEmpty();

            IRestResponse response;

            try
            {
                _restClient.Authenticator = OAuth1Authenticator.ForRequestToken(_consumerKey, _consumerSecret,
                                                                                _redirectUri.AbsoluteUri);
                var request = new RestRequest("oauth/request_token", Method.POST);
                request.AddParameter("response_type", "code");
                request.AddParameter("state", state);
                request.AddParameter("scope", string.Join(" ", _scope));
                response = _restClient.Execute(request);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Failed to obtain a Request Token from Google.", exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                throw new AuthenticationException(
                    string.Format(
                        "Failed to obtain a Request Token from Google OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}",
                        response == null ? "-- null response --" : response.StatusCode.ToString(),
                        response == null ? string.Empty : response.StatusDescription));
            }

            // Grab the params which should have the request token info.
            var querystringParameters = HttpUtility.ParseQueryString(response.Content);
            var oAuthToken = querystringParameters[OAuthTokenKey];
            var oAuthTokenSecret = querystringParameters[OAuthTokenSecretKey];

            if (string.IsNullOrEmpty(oAuthToken) ||
                string.IsNullOrEmpty(oAuthTokenSecret))
            {
                throw new AuthenticationException(
                    "Retrieved a Twitter Request Token but it doesn't contain both the oauth_token and oauth_token_secret parameters.");
            }

            return new Dictionary<string, string>
                   {
                       {OAuthTokenKey, oAuthToken},
                       {OAuthTokenSecretKey, oAuthTokenSecret}
                   };
        }

        #region Implementation of IAuthenticationProvider

        public RedirectResult RedirectToAuthenticate(string state)
        {
            Condition.Requires(state).IsNotNullOrEmpty();

            // First we need to grab a request token.
            var oAuthToken = RetrieveRequestToken(state);

            return null;
        }

        public IAuthenticatedClient AuthenticateClient(NameValueCollection parameters, string existingState)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
