using System;
using System.Collections.Specialized;
using System.Net;
using System.Runtime.Caching;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Contrib;

namespace WorldDomination.Web.Authentication.ExtraProviders.BitBucket
{
    // REFERENCE: https://confluence.atlassian.com/display/BITBUCKET/oauth+Endpoint

    public class BitBucketProvider : IAuthenticationProvider
    {
        private const string BaseUrl = " https://bitbucket.org";
        private const string OAuthTokenKey = "oauth_token";
        private const string OAuthTokenSecretKey = "oauth_token_secret";
        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly IRestClientFactory _restClientFactory;

        private System.Runtime.Caching.ObjectCache _memoryCache = MemoryCache.Default;
        private const int CacheExpiryInMinutes = 10;


        public BitBucketProvider(CustomProviderParams providerParams)
        {
            _consumerKey = providerParams.Key;
            _consumerSecret = providerParams.Secret;
            _restClientFactory = providerParams.RestClientFactory ?? new RestClientFactory();
        }

        public BitBucketProvider(string consumerKey, string consumerSecret, IRestClientFactory restClientFactory = null)
        {
            if (string.IsNullOrEmpty(consumerKey))
            {
                throw new ArgumentNullException("consumerKey");
            }

            if (string.IsNullOrEmpty(consumerSecret))
            {
                throw new ArgumentNullException("consumerSecret");
            }

            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;

            // IRestClientFactory can be optional.
            _restClientFactory = restClientFactory ?? new RestClientFactory();
        }

        private RequestTokenResult RetrieveRequestToken(Uri callBackUri)
        {
            if (callBackUri == null ||
                string.IsNullOrEmpty(callBackUri.AbsoluteUri))
            {
                throw new ArgumentNullException("callBackUri");
            }

            IRestResponse response;

            try
            {
                var restClient = _restClientFactory.CreateRestClient(BaseUrl);
                restClient.Authenticator = OAuth1Authenticator.ForRequestToken(_consumerKey, _consumerSecret,
                                                                               callBackUri.AbsoluteUri);
                var request = new RestRequest("!api/1.0/oauth/request_token", Method.POST);
                response = restClient.Execute(request);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Failed to obtain a Request Token from BitBucket.", exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                throw new AuthenticationException(
                    string.Format(
                        "Failed to obtain a Request Token from BitBucket OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}",
                        response == null ? "-- null response --" : response.StatusCode.ToString(),
                        response == null ? string.Empty : response.StatusDescription));
            }

            // Grab the params which should have the request token info.
            var querystringParameters = HttpUtility.ParseQueryString(response.Content);
            var oAuthToken = querystringParameters[OAuthTokenKey];
            var oAuthTokenSecret = querystringParameters[OAuthTokenSecretKey];


            // Cache the TokenSecret, for later.
            _memoryCache.Add(oAuthToken, oAuthTokenSecret, DateTime.Now.AddMinutes(CacheExpiryInMinutes));

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

        private static VerifierResult RetrieveOAuthVerifier(NameValueCollection queryStringParameters)
        {
            if (queryStringParameters == null)
            {
                throw new ArgumentNullException("queryStringParameters");
            }

            if (queryStringParameters.Count <= 0)
            {
                throw new ArgumentOutOfRangeException("queryStringParameters");
            }

            var denied = queryStringParameters["denied"];
            if (!string.IsNullOrEmpty(denied))
            {
                throw new AuthenticationException(
                    "Failed to accept the BitBucket App Authorization. Therefore, authentication didn't proceed.");
            }

            var oAuthToken = queryStringParameters[OAuthTokenKey];
            var oAuthVerifier = queryStringParameters["oauth_verifier"];

            if (string.IsNullOrEmpty(oAuthToken) ||
                string.IsNullOrEmpty(oAuthVerifier))
            {
                throw new AuthenticationException(
                    "Failed to retrieve an oauth_token and an oauth_token_secret after the client has signed and approved via BitBucket.");
            }

            return new VerifierResult
                   {
                       OAuthToken = oAuthToken,
                       OAuthVerifier = oAuthVerifier
                   };
        }

        private AccessTokenResult RetrieveAccessToken(VerifierResult verifierResult)
        {
            if (verifierResult == null)
            {
                throw new ArgumentNullException("verifierResult");
            }

            if (string.IsNullOrEmpty(verifierResult.OAuthToken))
            {
                throw new ArgumentException("verifierResult.OAuthToken");
            }

            if (string.IsNullOrEmpty(verifierResult.OAuthToken))
            {
                throw new ArgumentException("verifierResult.OAuthVerifier");
            }

            IRestResponse response;
            try
            {
                var request = new RestRequest("!api/1.0/oauth/access_token", Method.POST);
                var restClient = _restClientFactory.CreateRestClient(BaseUrl);

                // Retrieve the token secret from the cache :)
                string tokenSecret = null;
                if (_memoryCache != null && _memoryCache.Contains(verifierResult.OAuthToken))
                {
                    tokenSecret = (string)_memoryCache.Get(verifierResult.OAuthToken);
                    _memoryCache.Remove(verifierResult.OAuthToken);
                }

                restClient.Authenticator = OAuth1Authenticator.ForAccessToken(_consumerKey, _consumerSecret,
                                                                              verifierResult.OAuthToken,
                                                                              tokenSecret, verifierResult.OAuthVerifier);
                response = restClient.Execute(request);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Failed to convert Request Token to an Access Token, from BitBucket.",
                                                  exception);
            }

            if (response == null || response.StatusCode != HttpStatusCode.OK)
            {
                throw new AuthenticationException(
                    string.Format(
                        "Failed to obtain an Access Token from " + Name + " OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}",
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
            if (accessTokenResult == null)
            {
                throw new ArgumentNullException("accessTokenResult");
            }

            if (string.IsNullOrEmpty(accessTokenResult.AccessToken))
            {
                throw new ArgumentException("accessTokenResult.AccessToken");
            }

            if (string.IsNullOrEmpty(accessTokenResult.AccessTokenSecret))
            {
                throw new ArgumentException("accessTokenResult.AccessTokenSecret");
            }

            IRestResponse<VerifyCredentialsResult> response;
            try
            {
                var restClient = _restClientFactory.CreateRestClient(BaseUrl);
                restClient.Authenticator = OAuth1Authenticator.ForProtectedResource(_consumerKey, _consumerSecret,
                                                                                     accessTokenResult.AccessToken,
                                                                                     accessTokenResult.AccessTokenSecret);
                var request = new RestRequest("1.0/user");
                var responsex = restClient.Execute(request);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException(
                    "Failed to retrieve VerifyCredentials json data from the Twitter Api.", exception);
            }

            //if (response == null ||
            //    response.StatusCode != HttpStatusCode.OK ||
            //    response.Data == null)
            //{
            //    throw new AuthenticationException(
            //        string.Format(
            //            "Failed to retrieve VerifyCredentials json data OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}",
            //            response == null ? "-- null response --" : response.StatusCode.ToString(),
            //            response == null ? string.Empty : response.StatusDescription));
            //}

            //return response.Data;
            return null;
        }

        #region Implementation of IAuthenticationProvider

        public string Name
        {
            get { return "bitbucket"; }
        }

        public IAuthenticationServiceSettings DefaultAuthenticationServiceSettings
        {
            get { return new BitBucketAuthenticationServiceSettings(); }
        }

        public Uri RedirectToAuthenticate(IAuthenticationServiceSettings authenticationServiceSettings)
        {
            if (authenticationServiceSettings == null)
            {
                throw new ArgumentNullException("authenticationServiceSettings");
            }

            if (authenticationServiceSettings.CallBackUri == null)
            {
                throw new ArgumentException("authenticationServiceSettings.CallBackUri");
            }

            // First we need to grab a request token.
            var requestToken = RetrieveRequestToken(authenticationServiceSettings.CallBackUri);

            // Now redirect them to the BitBucket website to authenticate.
            var request = new RestRequest("!api/1.0/oauth/authenticate");
            request.AddParameter(OAuthTokenKey, requestToken.OAuthToken);
            var restClient = _restClientFactory.CreateRestClient(BaseUrl);
            return restClient.BuildUri(request);
        }

        public IAuthenticatedClient AuthenticateClient(IAuthenticationServiceSettings authenticationServiceSettings,
                                                       NameValueCollection queryStringParameters)
        {
            if (authenticationServiceSettings == null)
            {
                throw new ArgumentNullException("authenticationServiceSettings");
            }

            // Retrieve the OAuth Verifier.
            var oAuthVerifier = RetrieveOAuthVerifier(queryStringParameters);
            
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

            return null;
        }

        #endregion
    }
}