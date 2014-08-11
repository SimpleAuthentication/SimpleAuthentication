using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleAuthentication.Core.Providers.Twitter;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers.Twitter;
using SimpleAuthentication.Core.Tracing;

namespace SimpleAuthentication.Core.Providers
{
    public class TwitterProvider : BaseOAuth10Provider<RequestTokenResult>
    {
        private const string BaseUrl = "https://api.twitter.com";

        public TwitterProvider(ProviderParams providerParams)
            : base("Twitter", providerParams, "OAuth 1.0")
        {
        }

        //#region BaseOAuth20Token<AccessTokenResult> Implementation

        public override Uri AuthenticateRedirectionUrl
        {
            get { return new Uri(BaseUrl); }
        }

        //public override IEnumerable<string> DefaultScopes
        //{
        //    get { return new[]{string.Empty}; }
        //}

        //protected override async Task<AccessTokenResult> GetAccessTokenFromProviderAsync(string authorizationCode,
        //    Uri redirectUrl)
        //{
        //    if (string.IsNullOrWhiteSpace(authorizationCode))
        //    {
        //        throw new ArgumentNullException("authorizationCode");
        //    }

        //    if (redirectUrl == null ||
        //        string.IsNullOrWhiteSpace(redirectUrl.AbsoluteUri))
        //    {
        //        throw new ArgumentNullException("redirectUrl");
        //    }

        //    HttpResponseMessage response;

        //    using (var client = HttpClientFactory.GetHttpClient())
        //    {
        //        var requestUri = new Uri(RequestTokenUrl);
        //        var oAuth10AuthenticationHeader = base.CreateOAuth10AuthorizationHeaderSignature(RequestTokenUrl);
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(oAuth10AuthenticationHeader);

        //        TraceSource.TraceVerbose("Retrieving Access Token endpoint: {0}",
        //            requestUri.AbsoluteUri);
        //        response = await client.PostAsync(requestUri, null);
        //    }

        //    var jsonContent = await response.Content.ReadAsStringAsync();

        //    //if (response.StatusCode != HttpStatusCode.OK)
        //    //{
        //    //    TraceSource.TraceWarning("No Access Token Result retrieved from Google. Error Status Code: {0}. Error Message: {1}",
        //    //        response.StatusCode,
        //    //        jsonContent);
        //    //    return null;
        //    //}

        //    //var result = JsonConvert.DeserializeObject<dynamic>(jsonContent);
        //    //if (result == null)
        //    //{
        //    //    TraceSource.TraceWarning("No Access Token Result retrieved from Google.");
        //    //}

        //    //return MapDynamicResultToAccessTokenResult(result);
        //    throw new NotFiniteNumberException();
        //}

        //protected override AccessToken MapAccessTokenResultToAccessToken(AccessTokenResult accessTokenResult)
        //{
        //    throw new NotImplementedException();
        //}

        //protected override Task<UserInformation> RetrieveUserInformationAsync(AccessToken accessToken)
        //{
        //    throw new NotImplementedException();
        //}

        //#endregion

        protected override async Task<RequestTokenResult> GetRequestTokenAsync(Uri callbackUri, string state)
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

            var requestUri = string.Format("{0}/oauth/request_token", BaseUrl);

            var response = GetResponseForPostAsync(requestUri, callbackUri.AbsoluteUri).Result;

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                TraceSource.TraceWarning("No Access Token Result retrieved from Google. Error Status Code: {0}. Error Message: {1}",
                    response.StatusCode,
                    content);
                return null;
            }

            return MapRequestTokenToRequestTokenResult(content);
        }

        protected override async Task<AccessToken> GetAccessTokenAsync(IDictionary<string, string> queryString, 
            string callbackUrl)
        {
            if (queryString == null)
            {
                throw new ArgumentNullException("queryString");
            }

            if (string.IsNullOrWhiteSpace(callbackUrl))
            {
                throw new ArgumentNullException("callbackUrl");
            }

            var callbackResult = GetCallbackResultFromQuerystring(queryString);

            return await ExecuteGetAccessTokenAsync(callbackResult,
                callbackUrl);
        }

        protected override async Task<UserInformation> RetrieveUserInformationAsync(AccessToken accessToken)
        {
            if (accessToken == null)
            {
                throw new ArgumentNullException("accessToken");
            }

            throw new NotImplementedException();
            //var requestUri = string.Format("{0}/account/verify_credentials", BaseUrl);

            //var response = GetResponseForPostAsync(requestUri, callbackUri.AbsoluteUri).Result;

            //var content = await response.Content.ReadAsStringAsync();

            //if (response.StatusCode != HttpStatusCode.OK)
            //{
            //    TraceSource.TraceWarning("No Access Token Result retrieved from Google. Error Status Code: {0}. Error Message: {1}",
            //        response.StatusCode,
            //        content);
            //    return null;
            //}

            //return MapRequestTokenToRequestTokenResult(content);
        }

        protected override Uri CreateRedirectUri(RequestTokenResult requestToken)
        {
            if (requestToken == null ||
                string.IsNullOrWhiteSpace(requestToken.OAuthToken))
            {
                throw new ArgumentNullException("requestToken");
            }

            var url = string.Format("{0}/oauth/authenticate?oauth_token={1}",
                BaseUrl,
                requestToken.OAuthToken);

            return new Uri(url);
        }

        private RequestTokenResult MapRequestTokenToRequestTokenResult(string content)
        {
            const string tokenKey = "oauth_token";
            const string tokenSecretKey = "oauth_token_secret";

            string oAuthToken = null;
            string oAuthTokenSecret = null;

            var parameters = SystemHelpers.ConvertKeyValueContentToDictionary(content);
            if (parameters.ContainsKey(tokenKey))
            {
                oAuthToken = parameters[tokenKey];
            }

            if (parameters.ContainsKey(tokenSecretKey))
            {
                oAuthTokenSecret = parameters[tokenSecretKey];
            }

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

        private static CallbackResult GetCallbackResultFromQuerystring(IDictionary<string, string> queryString)
        {
            if (queryString == null)
            {
                throw new ArgumentNullException("queryString");
            }

            const string tokenKey = "oauth_token";
            const string verifierKey = "oauth_verifier";

            var callbackResult = new CallbackResult
            {
                Token = queryString.ContainsKey(tokenKey)
                    ? queryString[tokenKey]
                    : null,
                Verifier = queryString.ContainsKey(verifierKey)
                    ? queryString[verifierKey]
                    : null
            };

            if (string.IsNullOrWhiteSpace(callbackResult.Token) ||
                string.IsNullOrWhiteSpace(callbackResult.Verifier))
            {
                var errorMessage =
                    string.Format("Failed to recieve an {0} and/or {1} values. Both are required. {0}: {2}. {1}: {3}",
                        tokenKey,
                        verifierKey,
                        string.IsNullOrWhiteSpace(callbackResult.Token)
                            ? "-no token-"
                            : callbackResult.Token,
                        string.IsNullOrWhiteSpace(callbackResult.Verifier)
                            ? "-no verifier-"
                            : callbackResult.Verifier);
                throw new Exception(errorMessage);
            }

            return callbackResult;
        }

        private async Task<AccessToken> ExecuteGetAccessTokenAsync(CallbackResult callbackResult, string callbackUrl)
        {
            if (callbackResult == null)
            {
                throw new ArgumentNullException();
            }

            if (string.IsNullOrWhiteSpace(callbackUrl))
            {
                throw new ArgumentNullException("callbackUrl");
            }

            var requestUri = string.Format("{0}/oauth/access_token?oauth_verifier={1}", 
                BaseUrl,
                callbackResult.Verifier);

            var response = await GetResponseForPostAsync(requestUri,
                callbackUrl,
                callbackResult.Token);

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                TraceSource.TraceWarning("No Access Token Result retrieved from Twitter. Error Status Code: {0}. Error Message: {1}",
                    response.StatusCode,
                    content);
                return null;
            }

            return MapAccessTokenContentToAccessToken(content);
        }

        private AccessToken MapAccessTokenContentToAccessToken(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException("content");
            }

            const string tokenKey = "oauth_token";
            const string tokenSecretKey = "oauth_token_secret";

            string oAuthToken = null;
            string oAuthTokenSecret = null;

            var parameters = SystemHelpers.ConvertKeyValueContentToDictionary(content);
            if (parameters.ContainsKey(tokenKey))
            {
                oAuthToken = parameters[tokenKey];
            }

            if (parameters.ContainsKey(tokenSecretKey))
            {
                oAuthTokenSecret = parameters[tokenSecretKey];
            }

            TraceSource.TraceInformation("Retrieved OAuth Access Token: {0}. OAuth Access Token Secret: {1}.",
                                         string.IsNullOrEmpty(oAuthToken) ? "--no token--" : oAuthToken,
                                         string.IsNullOrEmpty(oAuthTokenSecret) ? "--no secret--" : oAuthTokenSecret);

            if (string.IsNullOrEmpty(oAuthToken) ||
                string.IsNullOrEmpty(oAuthTokenSecret))
            {
                throw new AuthenticationException(
                    "Retrieved a Twitter Access Token but it doesn't contain both the oauth_token and oauth_token_secret parameters.");
            }

            TraceSource.TraceVerbose("OAuth Access Token retrieved.");

            return new AccessToken
            {
                Token = oAuthToken,
                //SecretToken = oAuthTokenSecret
            };
        }
    }
}

/*
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

        public override async Task<RedirectToAuthenticateSettings> RedirectToAuthenticateAsync(Uri callbackUrl)
        {
            if (callbackUrl == null)
            {
                throw new ArgumentNullException("callbackUrl");
            }

            var state = Guid.NewGuid().ToString();

            // First we need to grab a request token.
            var oAuthToken = RetrieveRequestToken(callbackUrl, state);

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
        */
