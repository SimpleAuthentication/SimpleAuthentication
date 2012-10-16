using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using CuttingEdge.Conditions;
using RestSharp;
using RestSharp.Contrib;

namespace WorldDomination.Web.Authentication.Facebook
{
    // REFERENCE: http://developers.facebook.com/docs/authentication/server-side/

    public class FacebookProvider
    {
        private const string ScopeKey = "&scope={0}";
        private const string StateKey = "&state={0}";
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly Uri _redirectUri;
        private readonly IRestClient _restClient;
        private readonly IList<string> _scope;

        public FacebookProvider(string clientId, string clientSecret, Uri redirectUri)
            : this(clientId, clientSecret, redirectUri, null, null)
        {
        }

        public FacebookProvider(string clientId, string clientSecret, Uri redirectUri, IRestClient restClient)
            : this(clientId, clientSecret, redirectUri, null, restClient)
        {
        }

        public FacebookProvider(string clientId, string clientSecret, Uri redirectUri, IList<string> scope)
            : this(clientId, clientSecret, redirectUri, scope, null)
        {
        }

        public FacebookProvider(string clientId, string clientSecret, Uri redirectUri, IList<string> scope,
                                IRestClient restClient)
        {
            Condition.Requires(clientId).IsNotNullOrEmpty();
            Condition.Requires(clientSecret).IsNotNullOrEmpty();
            Condition.Requires(redirectUri).IsNotNull();

            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUri = redirectUri;

            // Optionals.
            _scope = scope;
            _restClient = restClient ?? new RestClient("http://graph.facebook.com");
        }

        public RedirectResult RedirectToAuthenticate(string state)
        {
            Condition.Requires(state).IsNotNullOrEmpty();

            var oauthDialogUri = string.Format("https://www.facebook.com/dialog/oauth?client_id={0}&redirect_uri={1}",
                                               _clientId, _redirectUri.AbsoluteUri);

            // Do we have any scope options?
            if (_scope != null && _scope.Count > 0)
            {
                oauthDialogUri += string.Format(ScopeKey, string.Join(",", _scope));
            }

            oauthDialogUri += string.Format(StateKey, state);

            return new RedirectResult(oauthDialogUri);
        }

        public void RetrieveUserInformation(FacebookClient facebookClient)
        {
            Condition.Requires(facebookClient).IsNotNull();
            Condition.Requires(facebookClient.Code).IsNotNullOrEmpty();

            IRestResponse response;
            try
            {
                //    "/oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}";
                var restRequest = new RestRequest("oauth/access_token");
                restRequest.AddParameter("client_id", _clientId);
                restRequest.AddParameter("redirect_uri", _redirectUri);
                restRequest.AddParameter("client_secret", _clientSecret);
                restRequest.AddParameter("code", facebookClient.Code);

                response = _restClient.Execute(restRequest);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Failed to retrieve an oauth access token from Facebook.", exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                throw new AuthenticationException(
                    string.Format(
                        "Failed to obtain an Access Token from Facebook OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}",
                        response == null ? "-- null response --" : response.StatusCode.ToString(),
                        response == null ? string.Empty : response.StatusDescription));
            }

            // Extract the relevant data.
            ExtractContent(response.Content, facebookClient);

            // Grab the user's information.
            facebookClient.UserInformation = RetrieveMe(facebookClient.AccessToken);
        }

        private static void ExtractContent(string responseBody, FacebookClient facebookClient)
        {
            Condition.Requires(responseBody).IsNotNullOrEmpty();
            Condition.Requires(facebookClient).IsNotNull();

            var querystringParameters = HttpUtility.ParseQueryString(responseBody);
            facebookClient.AccessToken = querystringParameters["access_token"];
            int expiresOn;
            if (int.TryParse(querystringParameters["expires_on"], out expiresOn))
            {
                facebookClient.ExpiresOn = DateTime.UtcNow.AddSeconds(expiresOn);
            }
            else
            {
                facebookClient.ExpiresOn = DateTime.MinValue;
            }

            if (string.IsNullOrEmpty(facebookClient.AccessToken) ||
                facebookClient.ExpiresOn <= DateTime.UtcNow)
            {
                throw new AuthenticationException(
                    "Retrieved a Facebook Access Token but it doesn't contain both the access_token and expires_on parameters.");
            }
        }

        private UserInformation RetrieveMe(string accessToken)
        {
            Condition.Requires(accessToken).IsNotNull();

            IRestResponse<MeResult> response;

            try
            {
                var restRequest = new RestRequest("me");
                restRequest.AddParameter("access_token", accessToken);

                response = _restClient.Execute<MeResult>(restRequest);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Failed to retrieve any Me data from the Facebook Api.", exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                throw new AuthenticationException(
                    string.Format(
                        "Failed to obtain some Me data from the Facebook api OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}",
                        response == null ? "-- null response --" : response.StatusCode.ToString(),
                        response == null ? string.Empty : response.StatusDescription));
            }

            return new UserInformation
                   {
                       Id = response.Data.Id,
                       Name = (response.Data.FirstName + " " + response.Data.LastName).Trim(),
                       Locale = response.Data.Locale,
                       UserName = response.Data.Username
                   };
        }
    }
}