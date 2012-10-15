using System;
using System.Collections.Specialized;
using System.Net;
using System.Web.Mvc;
using CuttingEdge.Conditions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Contrib;

namespace WorldDomination.Web.Authentication.Twitter
{
    public class TwitterProvider
    {
        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly IRestClient _restClient;

        public TwitterProvider(string consumerKey, string consumerSecret) : this(consumerKey, consumerSecret, null)
        {
        }

        public TwitterProvider(string consumerKey, string consumerSecret, IRestClient restClient)
        {
            Condition.Requires(consumerKey).IsNotNullOrEmpty();
            Condition.Requires(consumerSecret).IsNotNullOrEmpty();

            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;

            // IRestClient can be optional.
            _restClient = restClient ?? new RestClient("http://api.twitter.com");
        }

        public RedirectResult RedirectToAuthenticate(string callbackUrl)
        {
            Condition.Requires(callbackUrl).IsNotNullOrEmpty();

            // First we need to grab a request token.
            var twitterClient = RetrieveRequestToken(callbackUrl);

            // Now we need the user to enter their name/password/accept this app @ Twitter.
            // This means we need to redirect them to the Twitter website.
            var request = new RestRequest("oauth/authorize");
            request.AddParameter("oauth_token", twitterClient.OAuthToken);
            var url = _restClient.BuildUri(request).ToString();
            return new RedirectResult(url);
        }

        public void RetrieveUserInformation(TwitterClient twitterClient, NameValueCollection parameters)
        {
            // Retrieve the OAuth Verifier.
            RetrieveOAuthVerifier(twitterClient, parameters);

            // Convert the Request Token to an Access Token, now that we have a verifier.
            RetrieveAccessToken(twitterClient);

            // Grab the user information.
            var verifyCredentialsResult = VerifyCredentials(twitterClient);

            twitterClient.UserInformation = new UserInformation
                                            {
                                                Name = verifyCredentialsResult.Name,
                                                Id = verifyCredentialsResult.Id,
                                                Locale = verifyCredentialsResult.Lang,
                                                UserName = verifyCredentialsResult.ScreenName
                                            };
        }

        private static void RetrieveOAuthVerifier(TwitterClient twitterClient, NameValueCollection parameters)
        {
            twitterClient.OAuthToken = parameters["oauth_token"];
            twitterClient.OAuthVerifier = parameters["oauth_verifier"];

            if (string.IsNullOrEmpty(twitterClient.OAuthToken) ||
                string.IsNullOrEmpty(twitterClient.OAuthVerifier))
            {
                throw new AuthenticationException("Failed to retrieve an oauth_token and an oauth_token_secret after the client has signed and approved via Twitter.");
            }
        }

        private TwitterClient RetrieveRequestToken(string callbackUrl)
        {
            Condition.Requires(callbackUrl).IsNotNullOrEmpty();

            IRestResponse response;

            try
            {
                _restClient.Authenticator = OAuth1Authenticator.ForRequestToken(_consumerKey, _consumerSecret, callbackUrl);
                var request = new RestRequest("oauth/request_token", Method.POST);
                response = _restClient.Execute(request);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Failed to obtain a Request Token from Twitter.", exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                throw new AuthenticationException(
                    string.Format(
                        "Failed to obtain a Request Token from Twitter OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}",
                        response == null ? "-- null response --" : response.StatusCode.ToString(),
                        response == null ? string.Empty : response.StatusDescription));
            }

            // Grab the params which should have the request token info.
            var querystringParameters = HttpUtility.ParseQueryString(response.Content);
            var twitterClient = new TwitterClient
            {
                OAuthToken = querystringParameters["oauth_token"],
                OAuthTokenSecret = querystringParameters["oauth_token_secret"],
            };

            if (string.IsNullOrEmpty(twitterClient.OAuthToken) ||
                string.IsNullOrEmpty(twitterClient.OAuthTokenSecret))
            {
                throw new AuthenticationException("Retrieved a Twitter Request Token but it doesn't contain both the oauth_token and oauth_token_secret parameters.");
            }

            return twitterClient;
        }

        private void RetrieveAccessToken(TwitterClient twitterClient)
        {
            IRestResponse response;
            try
            {
                var request = new RestRequest("oauth/access_token", Method.POST);
                _restClient.Authenticator = OAuth1Authenticator.ForAccessToken(_consumerKey, _consumerSecret, twitterClient.OAuthToken, twitterClient.OAuthTokenSecret, twitterClient.OAuthVerifier);
                response = _restClient.Execute(request);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Failed to convert Request Token to an Access Token, from Twitter.", exception);
            }

            if (response == null || response.StatusCode != HttpStatusCode.OK)
            {
                throw new AuthenticationException(
                    string.Format(
                        "Failed to obtain an Access Token from Twitter OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}",
                        response == null ? "-- null response --" : response.StatusCode.ToString(),
                        response == null ? string.Empty : response.StatusDescription));
            }

            var querystringParameters = HttpUtility.ParseQueryString(response.Content);
            twitterClient.OAuthToken = querystringParameters["oauth_token"];
            twitterClient.OAuthTokenSecret = querystringParameters["oauth_token_secret"];
        }

        private VerifyCredentialsResult VerifyCredentials(TwitterClient twitterClient)
        {
            Condition.Requires(twitterClient).IsNotNull();
            Condition.Requires(twitterClient.OAuthToken).IsNotNullOrEmpty();
            Condition.Requires(twitterClient.OAuthVerifier).IsNotNullOrEmpty();

            IRestResponse<VerifyCredentialsResult> response;
            try
            {
                _restClient.Authenticator = OAuth1Authenticator.ForProtectedResource(_consumerKey, _consumerSecret, twitterClient.OAuthToken, twitterClient.OAuthTokenSecret);
                var request = new RestRequest("1.1/account/verify_credentials.json");
                response = _restClient.Execute<VerifyCredentialsResult>(request);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Failed to retrieve VerifyCredentials json data from the Twitter Api.", exception);
            }

            if (response == null || 
                response.StatusCode != HttpStatusCode.OK ||
                response.Data == null)
            {
                throw new AuthenticationException(
                    string.Format(
                        "Failed to retrieve VerifyCredentials json data OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}",
                        response == null ? "-- null response --" : response.StatusCode.ToString(),
                        response == null ? string.Empty : response.StatusDescription));
            }

            return response.Data;
        }
    }
}