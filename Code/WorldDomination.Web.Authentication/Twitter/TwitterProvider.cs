using System;
using System.Collections.Specialized;
using System.Net;
using CuttingEdge.Conditions;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Contrib;
using WorldDomination.Web.Authentication.Config;

namespace WorldDomination.Web.Authentication.Twitter
{
    public class TwitterProvider : IAuthenticationProvider
    {
        private const string DeniedKey = "denied";
        private const string OAuthTokenKey = "oauth_token";
        private const string OAuthTokenSecretKey = "oauth_token_secret";
        private const string OAuthVerifierKey = "oauth_verifier";

        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly IRestClient _restClient;

        public TwitterProvider(ProviderKey providerKey, IRestClient restClient = null)
            : this(providerKey.Key, providerKey.Secret, restClient)
        {
        }

        public TwitterProvider(string consumerKey, string consumerSecret, IRestClient restClient = null)
        {
            Condition.Requires(consumerKey).IsNotNullOrEmpty();
            Condition.Requires(consumerSecret).IsNotNullOrEmpty();

            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;

            // IRestClient can be optional.
            _restClient = restClient ?? new RestClient("https://api.twitter.com");
        }

        private RequestTokenResult RetrieveRequestToken()
        {
            IRestResponse response;

            try
            {
                _restClient.Authenticator = OAuth1Authenticator.ForRequestToken(_consumerKey, _consumerSecret,
                                                                                CallBackUri.AbsoluteUri);
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

            return new RequestTokenResult
            {
                OAuthToken = oAuthToken,
                OAuthTokenSecret = oAuthTokenSecret
            };
        }

        private static VerifierResult RetrieveOAuthVerifier(NameValueCollection parameters)
        {
            Condition.Requires(parameters).IsNotNull();

            var denied = parameters[DeniedKey];
            if (!string.IsNullOrEmpty(denied))
            {
                throw new AuthenticationException(
                    "Failed to accept the Twitter App Authorization. Therefore, authentication didn't proceed.");
            }

            var oAuthToken = parameters[OAuthTokenKey];
            var oAuthVerifier = parameters[OAuthVerifierKey];

            if (string.IsNullOrEmpty(oAuthToken) ||
                string.IsNullOrEmpty(oAuthVerifier))
            {
                throw new AuthenticationException(
                    "Failed to retrieve an oauth_token and an oauth_token_secret after the client has signed and approved via Twitter.");
            }

            return new VerifierResult
            {
                OAuthToken = oAuthToken,
                OAuthVerifier = oAuthVerifier
            };
        }

        private AccessTokenResult RetrieveAccessToken(VerifierResult verifierResult)
        {
            Condition.Requires(verifierResult).IsNotNull();
            Condition.Requires(verifierResult.OAuthToken).IsNotNullOrEmpty();
            Condition.Requires(verifierResult.OAuthVerifier).IsNotNullOrEmpty();

            IRestResponse response;
            try
            {
                var request = new RestRequest("oauth/access_token", Method.POST);
                _restClient.Authenticator = OAuth1Authenticator.ForAccessToken(_consumerKey, _consumerSecret,
                                                                               verifierResult.OAuthToken,
                                                                               null, verifierResult.OAuthVerifier);
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
            return new AccessTokenResult
            {
                AccessToken = querystringParameters[OAuthTokenKey],
                AccessTokenSecret = querystringParameters[OAuthTokenSecretKey]
            };
        }

        private VerifyCredentialsResult VerifyCredentials(AccessTokenResult accessTokenResult)
        {
            Condition.Requires(accessTokenResult).IsNotNull();
            Condition.Requires(accessTokenResult.AccessToken).IsNotNullOrEmpty();
            Condition.Requires(accessTokenResult.AccessTokenSecret).IsNotNullOrEmpty();

            IRestResponse<VerifyCredentialsResult> response;
            try
            {
                _restClient.Authenticator = OAuth1Authenticator.ForProtectedResource(_consumerKey, _consumerSecret,
                                                                                     accessTokenResult.AccessToken,
                                                                                     accessTokenResult.AccessTokenSecret);
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

        public string Name
        {
            get { return "Twitter"; }
        }

        public Uri CallBackUri { get; private set; }

        public Uri RedirectToAuthenticate(IAuthenticationServiceSettings authenticationServiceSettings)
        {
            Condition.Requires(authenticationServiceSettings).IsNotNull();

            CallBackUri = authenticationServiceSettings.CallBackUri;

            // First we need to grab a request token.
            var oAuthToken = RetrieveRequestToken();

            // Now we need the user to enter their name/password/accept this app @ Twitter.
            // This means we need to redirect them to the Twitter website.
            var request = new RestRequest("oauth/authenticate");
            request.AddParameter(OAuthTokenKey, oAuthToken.OAuthToken);
            return _restClient.BuildUri(request);
        }

        public IAuthenticatedClient AuthenticateClient(NameValueCollection parameters, string existingState)
        {
            // Retrieve the OAuth Verifier.
            var oAuthVerifier = RetrieveOAuthVerifier(parameters);

            // Convert the Request Token to an Access Token, now that we have a verifier.
            var oAuthAccessToken = RetrieveAccessToken(oAuthVerifier);

            // Grab the user information.
            var verifyCredentialsResult = VerifyCredentials(oAuthAccessToken);

            return new AuthenticatedClient(Name.ToLowerInvariant())
            {
                UserInformation = new UserInformation
                {
                    Name = verifyCredentialsResult.Name,
                    Id = verifyCredentialsResult.Id.ToString(),
                    Locale = verifyCredentialsResult.Lang,
                    UserName = verifyCredentialsResult.ScreenName,
                    Picture = verifyCredentialsResult.ProfileImageUrl
                },
                AccessToken = oAuthAccessToken.AccessToken
            };
        }

        public IAuthenticationServiceSettings DefaultAuthenticationServiceSettings
        {
            get { return new TwitterAuthenticationServiceSettings(); }
        }

        #endregion
    }
}