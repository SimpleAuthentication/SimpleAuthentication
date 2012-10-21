using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Web.Mvc;
using CuttingEdge.Conditions;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Contrib;

namespace WorldDomination.Web.Authentication.Twitter
{
    public class TwitterProvider : IAuthenticationProvider
    {
        private const string OAuthTokenKey = "oauth_token";
        private const string OAuthTokenSecretKey = "oauth_token_secret";
        private const string OAuthVerifierKey = "oauth_verifier";

        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly Uri _redirectUri;
        private readonly IRestClient _restClient;

        public TwitterProvider(string consumerKey, string consumerSecret, Uri redirectUri) : this(consumerKey, consumerSecret, redirectUri, null)
        {
        }

        public TwitterProvider(string consumerKey, string consumerSecret, Uri redirectUri, IRestClient restClient)
        {
            Condition.Requires(consumerKey).IsNotNullOrEmpty();
            Condition.Requires(consumerSecret).IsNotNullOrEmpty();
            Condition.Requires(redirectUri).IsNotNull();

            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _redirectUri = redirectUri;

            // IRestClient can be optional.
            _restClient = restClient ?? new RestClient("https://api.twitter.com");
        }

        private IDictionary<string, string> RetrieveRequestToken()
        {
            IRestResponse response;

            try
            {
                _restClient.Authenticator = OAuth1Authenticator.ForRequestToken(_consumerKey, _consumerSecret,
                                                                                _redirectUri.AbsoluteUri);
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

        private static IDictionary<string, string> RetrieveOAuthVerifier(NameValueCollection parameters)
        {
            Condition.Requires(parameters).IsNotNull();

            var oAuthToken = parameters[OAuthTokenKey];
            var oAuthVerifier = parameters[OAuthVerifierKey];

            if (string.IsNullOrEmpty(oAuthToken) ||
                string.IsNullOrEmpty(oAuthVerifier))
            {
                throw new AuthenticationException(
                    "Failed to retrieve an oauth_token and an oauth_token_secret after the client has signed and approved via Twitter.");
            }

            return new Dictionary<string, string>
                   {
                       {OAuthTokenKey, oAuthToken},
                       {OAuthVerifierKey, oAuthVerifier}
                   };
        }

        private IDictionary<string, string> RetrieveAccessToken(string oAuthToken, string oAuthTokenSecret,
                                                                string oAuthVerifier)
        {
            Condition.Requires(oAuthToken).IsNotNullOrEmpty();
            //Condition.Requires(oAuthTokenSecret).IsNotNullOrEmpty();
            Condition.Requires(oAuthVerifier).IsNotNullOrEmpty();

            IRestResponse response;
            try
            {
                var request = new RestRequest("oauth/access_token", Method.POST);
                _restClient.Authenticator = OAuth1Authenticator.ForAccessToken(_consumerKey, _consumerSecret, oAuthToken,
                                                                               oAuthTokenSecret, oAuthVerifier);
                response = _restClient.Execute(request);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Failed to convert Request Token to an Access Token, from Twitter.",
                                                  exception);
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
            return new Dictionary<string, string>
                   {
                       {OAuthTokenKey, querystringParameters[OAuthTokenKey]},
                       {OAuthTokenSecretKey, querystringParameters[OAuthTokenSecretKey]}
                   };
        }


        private VerifyCredentialsResult VerifyCredentials(string oAuthToken, string oAuthTokenSecret)
        {
            Condition.Requires(oAuthToken).IsNotNullOrEmpty();
            Condition.Requires(oAuthTokenSecret).IsNotNullOrEmpty();

            IRestResponse<VerifyCredentialsResult> response;
            try
            {
                _restClient.Authenticator = OAuth1Authenticator.ForProtectedResource(_consumerKey, _consumerSecret,
                                                                                     oAuthToken, oAuthTokenSecret);
                var request = new RestRequest("1.1/account/verify_credentials.json");
                response = _restClient.Execute<VerifyCredentialsResult>(request);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException(
                    "Failed to retrieve VerifyCredentials json data from the Twitter Api.", exception);
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

        #region Implementation of IAuthenticationProvider

        public RedirectResult RedirectToAuthenticate(string state)
        {
            Condition.Requires(state).IsNotNullOrEmpty();

            // First we need to grab a request token.
            var oAuthToken = RetrieveRequestToken();

            // Now we need the user to enter their name/password/accept this app @ Twitter.
            // This means we need to redirect them to the Twitter website.
            var request = new RestRequest("oauth/authorize");
            request.AddParameter(OAuthTokenKey, oAuthToken[OAuthTokenKey]);
            var url = _restClient.BuildUri(request).ToString();
            return new RedirectResult(url);
        }

        public IAuthenticatedClient AuthenticateClient(NameValueCollection parameters, string existingState)
        {
            try
            {
                // Retrieve the OAuth Verifier.
                var oAuthVerifier = RetrieveOAuthVerifier(parameters);

                // Convert the Request Token to an Access Token, now that we have a verifier.
                var oAuthAccessToken = RetrieveAccessToken(oAuthVerifier[OAuthTokenKey], null,
                                                           oAuthVerifier[OAuthVerifierKey]);

                // Grab the user information.
                var verifyCredentialsResult = VerifyCredentials(oAuthAccessToken[OAuthTokenKey],
                                                                oAuthAccessToken[OAuthTokenSecretKey]);

                return new AuthenticatedClient(ProviderType.Twitter)
                       {
                           UserInformation = new UserInformation
                                             {
                                                 Name = verifyCredentialsResult.Name,
                                                 Id = verifyCredentialsResult.Id,
                                                 Locale = verifyCredentialsResult.Lang,
                                                 UserName = verifyCredentialsResult.ScreenName
                                             },
                           AccessToken = oAuthAccessToken[OAuthTokenKey]
                       };
            }
            catch (Exception exception)
            {
                return new AuthenticatedClient(ProviderType.Twitter)
                       {
                           ErrorInformation = new ErrorInformation
                                              {
                                                  Message = exception.Message,
                                                  Exception = exception
                                              }
                       };
            }
        }

        #endregion
    }
}