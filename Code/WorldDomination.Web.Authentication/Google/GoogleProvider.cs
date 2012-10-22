using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using CuttingEdge.Conditions;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Contrib;

namespace WorldDomination.Web.Authentication.Google
{
    // REFERENCE: https://developers.google.com/accounts/docs/OAuth2Login

    public class GoogleProvider : IAuthenticationProvider
    {
        private const string ScopeKey = "&scope={0}";
        private const string OAuthTokenKey = "oauth_token";
        private const string OAuthTokenSecretKey = "oauth_token_secret";
        private const string OAuthVerifierKey = "oauth_verifier";

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly Uri _redirectUri;
        private readonly IRestClient _restClient;
        private readonly IList<string> _scope;

        public GoogleProvider(string clientId, string clientSecret, Uri redirectUri)
            : this(clientId, clientSecret, redirectUri, null, null)
        {
        }

        public GoogleProvider(string clientId, string clientSecret, Uri redirectUri, IList<string> scope,
                              IRestClient restClient)
        {
            Condition.Requires(clientId).IsNotNullOrEmpty();
            Condition.Requires(clientSecret).IsNotNullOrEmpty();
            Condition.Requires(redirectUri).IsNotNull();

            _clientId = clientId;
            _clientSecret = clientSecret;
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
            _restClient = restClient ?? new RestClient("https://accounts.google.com");
        }

        private IDictionary<string, string> RetrieveRequestToken(string state)
        {
            Condition.Requires(state).IsNotNullOrEmpty();

            IRestResponse response;

            try
            {
                // NOTE: Can this be replaced with a RestSharp Authenticator?
                //_restClient.Authenticator = OAuth1Authenticator.ForRequestToken(_clientId, _clientSecret,
                //                                                                _redirectUri.AbsoluteUri);
                var request = new RestRequest("o/oauth2/auth", Method.GET);
                request.AddParameter("client_id", _clientId);
                request.AddParameter("redirect_uri", _redirectUri);
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

        public string Name
        {
            get { return "Google"; }
        }

        public Uri RedirectToAuthenticate(string state)
        {
            Condition.Requires(state).IsNotNullOrEmpty();

            var oauthDialogUri = string.Format("https://accounts.google.com/o/oauth2/auth?client_id={0}&redirect_uri={1}&response_type=code&state={2}",
                                               _clientId, _redirectUri.AbsoluteUri, state);

            // Do we have any scope options?
            if (_scope != null && _scope.Count > 0)
            {
                // Google uses a space-delimeted string for their scope key.
                oauthDialogUri += string.Format(ScopeKey, string.Join(" ", _scope));
            }

            return new Uri(oauthDialogUri);
        }

        public IAuthenticatedClient AuthenticateClient(NameValueCollection parameters, string existingState)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}