using System;
using System.Collections.Specialized;
using System.Net;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Contrib;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers.Twitter;
using SimpleAuthentication.Core.Tracing;

namespace SimpleAuthentication.Core.Providers
{
    public class TwitterProvider : BaseProvider, IPublicPrivateKeyProvider
    {
        private const string BaseUrl = "https://api.twitter.com";
        private const string DeniedKey = "denied";
        private const string OAuthTokenKey = "oauth_token";
        private const string OAuthTokenSecretKey = "oauth_token_secret";
        private const string OAuthVerifierKey = "oauth_verifier";

        public TwitterProvider(ProviderParams providerParams) : base("Twitter", "OAuth 1.0a")
        {
            providerParams.Validate();

            PublicApiKey = providerParams.PublicApiKey;
            SecretApiKey = providerParams.SecretApiKey;

            RestClientFactory = new RestClientFactory();
        }

        #region IPublicPrivateKeyProvider Implementation

        public string PublicApiKey { get; protected set; }
        public string SecretApiKey { get; protected set; }

        #endregion

        public IRestClientFactory RestClientFactory { get; set; }

        private RequestTokenResult RetrieveRequestToken(Uri callbackUri, string state)
        {
            TraceSource.TraceVerbose("Retrieving the Request Token.");

            if (callbackUri == null)
            {
                throw new ArgumentNullException("callbackUri");
            }

            if (string.IsNullOrEmpty(state))
            {
                throw new ArgumentNullException("state");
            }

            IRestResponse response;
            var uri = string.Format("{0}{1}", callbackUri, GetQuerystringState(state));

            try
            {
                var restClient = RestClientFactory.CreateRestClient(BaseUrl);
                restClient.Authenticator = OAuth1Authenticator.ForRequestToken(PublicApiKey, SecretApiKey,
                                                                               uri);
                var restRequest = new RestRequest("oauth/request_token", Method.POST);

                TraceSource.TraceVerbose("Retrieving user information. Twitter Endpoint: {0}",
                                         restClient.BuildUri(restRequest).AbsoluteUri);

                response = restClient.Execute(restRequest);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Failed to obtain a Request Token from Twitter.", exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                var errorMessage = string.Format(
                    "Failed to obtain a request token from the Twitter api OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}. Error Message: {2}.",
                    response == null ? "-- null response --" : response.StatusCode.ToString(),
                    response == null ? string.Empty : response.StatusDescription,
                    response == null
                        ? string.Empty
                        : response.ErrorException == null
                              ? "--no error exception--"
                              : response.ErrorException.RecursiveErrorMessages());

                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            // Grab the params which should have the request token info.
            var querystringParameters = HttpUtility.ParseQueryString(response.Content);
            var oAuthToken = querystringParameters[OAuthTokenKey];
            var oAuthTokenSecret = querystringParameters[OAuthTokenSecretKey];

            TraceSource.TraceInformation("Retrieved OAuth Token: {0}. OAuth Verifier: {1}.",
                                         string.IsNullOrEmpty(oAuthToken) ? "--no token--" : oAuthToken,
                                         string.IsNullOrEmpty(oAuthTokenSecret) ? "--no secret--" : oAuthTokenSecret);

            if (string.IsNullOrEmpty(oAuthToken) ||
                string.IsNullOrEmpty(oAuthTokenSecret))
            {
                throw new AuthenticationException(
                    "Retrieved a Twitter Request Token but it doesn't contain both the oauth_token and oauth_token_secret parameters.");
            }

            TraceSource.TraceVerbose("OAuth Token retrieved.");

            return new RequestTokenResult
            {
                OAuthToken = oAuthToken,
                OAuthTokenSecret = oAuthTokenSecret
            };
        }

        private VerifierResult RetrieveOAuthVerifier(NameValueCollection queryStringParameters)
        {
            TraceSource.TraceVerbose("Retrieving the OAuth Verifier.");

            if (queryStringParameters == null)
            {
                throw new ArgumentNullException("queryStringParameters");
            }

            if (queryStringParameters.Count <= 0)
            {
                throw new ArgumentOutOfRangeException("queryStringParameters");
            }

            var denied = queryStringParameters[DeniedKey];
            if (!string.IsNullOrEmpty(denied))
            {
                throw new AuthenticationException(
                    "Failed to accept the Twitter App Authorization. Therefore, authentication didn't proceed.");
            }

            var oAuthToken = queryStringParameters[OAuthTokenKey];
            var oAuthVerifier = queryStringParameters[OAuthVerifierKey];

            TraceSource.TraceInformation("Retrieved OAuth Token: {0}. OAuth Verifier: {1}.",
                                         string.IsNullOrEmpty(oAuthToken) ? "--no token--" : oAuthToken,
                                         string.IsNullOrEmpty(oAuthVerifier) ? "--no verifier--" : oAuthVerifier);

            if (string.IsNullOrEmpty(oAuthToken) ||
                string.IsNullOrEmpty(oAuthVerifier))
            {
                throw new AuthenticationException(
                    "Failed to retrieve an oauth_token and an oauth_token_secret after the client has signed and approved via Twitter.");
            }

            TraceSource.TraceVerbose("OAuth Verifier retrieved.");

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
                var restRequest = new RestRequest("oauth/access_token", Method.POST);
                var restClient = RestClientFactory.CreateRestClient(BaseUrl);
                TraceSource.TraceVerbose("Retrieving Access Token endpoint: {0}",
                                         restClient.BuildUri(restRequest).AbsoluteUri);

                restClient.Authenticator = OAuth1Authenticator.ForAccessToken(PublicApiKey, SecretApiKey,
                                                                              verifierResult.OAuthToken,
                                                                              null, verifierResult.OAuthVerifier);
                response = restClient.Execute(restRequest);
            }
            catch (Exception exception)
            {
                var errorMessage =
                    string.Format("Failed to retrieve an oauth access token from Twitter. Error Messages: {0}",
                                  exception.RecursiveErrorMessages());
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage, exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                var errorMessage = string.Format(
                    "Failed to obtain an Access Token from Twitter OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}. Error Content: {2}. Error Message: {3}.",
                    response == null ? "-- null response --" : response.StatusCode.ToString(),
                    response == null ? string.Empty : response.StatusDescription,
                    response == null ? string.Empty : response.Content,
                    response == null
                        ? string.Empty
                        : response.ErrorException == null
                              ? "--no error exception--"
                              : response.ErrorException.RecursiveErrorMessages());

                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            var querystringParameters = HttpUtility.ParseQueryString(response.Content);

            TraceSource.TraceVerbose("Retrieved OAuth Token - Public Key: {0}. Secret Key: {1} ",
                                     string.IsNullOrEmpty(querystringParameters[OAuthTokenKey])
                                         ? "no public key retrieved from the querystring. What Ze Fook?"
                                         : querystringParameters[OAuthTokenKey],
                                     string.IsNullOrEmpty(querystringParameters[OAuthTokenSecretKey])
                                         ? "no secret key retrieved from the querystring. What Ze Fook?"
                                         : querystringParameters[OAuthTokenSecretKey]);

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
                var restClient = RestClientFactory.CreateRestClient(BaseUrl);
                restClient.Authenticator = OAuth1Authenticator.ForProtectedResource(PublicApiKey, SecretApiKey,
                                                                                    accessTokenResult.AccessToken,
                                                                                    accessTokenResult.AccessTokenSecret);
                var restRequest = new RestRequest("1.1/account/verify_credentials.json");

                TraceSource.TraceVerbose("Retrieving user information. Twitter Endpoint: {0}",
                                         restClient.BuildUri(restRequest).AbsoluteUri);

                response = restClient.Execute<VerifyCredentialsResult>(restRequest);
            }
            catch (Exception exception)
            {
                var errorMessage = "Failed to retrieve VerifyCredentials json data from the Twitter Api. Error Messages: "
                                   + exception.RecursiveErrorMessages();
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage, exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK ||
                response.Data == null)
            {
                var errorMessage = string.Format(
                    "Failed to obtain some VerifyCredentials json data from the Facebook api OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}. Error Message: {2}.",
                    response == null ? "-- null response --" : response.StatusCode.ToString(),
                    response == null ? string.Empty : response.StatusDescription,
                    response == null
                        ? string.Empty
                        : response.ErrorException == null
                              ? "--no error exception--"
                              : response.ErrorException.RecursiveErrorMessages());

                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return response.Data;
        }

        #region IAuthenticationProvider Implementation

        public override RedirectToAuthenticateSettings RedirectToAuthenticate(Uri callbackUri)
        {
            if (callbackUri == null)
            {
                throw new ArgumentNullException("callbackUri");
            }

            var state = Guid.NewGuid().ToString();

            // First we need to grab a request token.
            var oAuthToken = RetrieveRequestToken(callbackUri, state);

            // Now we need the user to enter their name/password/accept this app @ Twitter.
            // This means we need to redirect them to the Twitter website.
            var request = new RestRequest("oauth/authenticate");
            request.AddParameter(OAuthTokenKey, oAuthToken.OAuthToken);
            var restClient = RestClientFactory.CreateRestClient(BaseUrl);

            return new RedirectToAuthenticateSettings
                   {
                       RedirectUri = restClient.BuildUri(request),
                       State = state
                   };
        }

        public override IAuthenticatedClient AuthenticateClient(NameValueCollection queryStringParameters,
                                                                string state,
                                                                Uri callbackUri)
        {
            #region Parameter checks

            if (queryStringParameters == null ||
                queryStringParameters.Count <= 0)
            {
                throw new ArgumentNullException("queryStringParameters");
            }

            if (string.IsNullOrEmpty(state))
            {
                throw new ArgumentNullException("state");
            }

            if (callbackUri == null)
            {
                throw new ArgumentNullException("callbackUri");
            }

            #endregion

            TraceSource.TraceVerbose(
                "Trying to get the authenticated client details. NOTE: This is using OAuth 1.0a. ~~Le sigh~~.");

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
                AccessToken = new AccessToken
                {
                    PublicToken = oAuthAccessToken.AccessToken
                }
            };
        }

        #endregion
    }
}