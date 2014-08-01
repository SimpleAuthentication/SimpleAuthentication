using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers.Facebook;
using SimpleAuthentication.Core.Tracing;

namespace SimpleAuthentication.Core.Providers
{
    // REFERENCE: https://developers.facebook.com/docs/facebook-login/login-flow-for-web-no-jssdk/

    public class FacebookProvider : BaseOAuth20Provider<AccessTokenResult>
    {
        public FacebookProvider(ProviderParams providerParams) : this("Facebook", providerParams)
        {
        }

        protected FacebookProvider(string name, ProviderParams providerParams) : base(name, providerParams)
        {
            DisplayType = DisplayType.Unknown;
            IsMobile = false;
        }

        #region BaseOAuth20Token<AccessTokenResult> Implementation

        public override Uri AuthenticateRedirectionUrl
        {
            get
            {
                return IsMobile
                    ? new Uri("https://m.facebook.com/dialog/oauth")
                    : new Uri("https://www.facebook.com/dialog/oauth");
            }
        }

        protected override async Task<AccessTokenResult> GetAccessTokenFromProviderAsync(string authorizationCode,
            Uri redirectUrl)
        {
            if (string.IsNullOrWhiteSpace(authorizationCode))
            {
                throw new ArgumentNullException("authorizationCode");
            }

            if (redirectUrl == null ||
                string.IsNullOrWhiteSpace(redirectUrl.AbsoluteUri))
            {
                throw new ArgumentNullException("redirectUrl");
            }

            HttpResponseMessage response;

            using (var client = HttpClientFactory.GetHttpClient())
            {
                var requestUri = GetAccessTokenUri(redirectUrl.AbsoluteUri, authorizationCode);
                TraceSource.TraceVerbose("Retrieving Access Token endpoint: {0}",
                    requestUri.AbsoluteUri);

                response = await client.GetAsync(requestUri);
            }

            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                TraceSource.TraceWarning(
                    "No Access Token Result retrieved from Google. Error Status Code: {0}. Error Message: {1}",
                    response.StatusCode,
                    content);
                return null;
            }

            return ExtractAccessTokenFromContent(content);

            //return MapAccessTokenResultToAccessToken(accessTokenResult);
        }

        //protected override string CreateRedirectionQuerystringParameters(Uri callbackUri, string state)
        //{
        //    if (callbackUri == null)
        //    {
        //        throw new ArgumentNullException("callbackUri");
        //    }

        //    if (string.IsNullOrEmpty(state))
        //    {
        //        throw new ArgumentNullException("state");
        //    }

        //    var display = DisplayType == DisplayType.Unknown
        //                      ? string.Empty
        //                      : "&display=" + DisplayType.ToString().ToLowerInvariant();

        //    // REFERENCE: https://developers.facebook.com/docs/reference/dialogs/oauth/
        //    // NOTE: Facebook is case-sensitive anal retentive with regards to their uri + querystring params.
        //    //       So ... we'll lowercase the entire biatch. Thanks, Facebook :(
        //    return string.Format("client_id={0}&redirect_uri={1}{2}{3}{4}",
        //                         PublicApiKey, callbackUri.AbsoluteUri, GetScope(), GetQuerystringState(state), display)
        //                 .ToLowerInvariant();
        //}

        protected override string GetAuthorizationCodeFromQueryString(NameValueCollection queryStringParameters)
        {
            if (queryStringParameters == null)
            {
                throw new ArgumentNullException("queryStringParameters");
            }

            if (queryStringParameters.Count <= 0)
            {
                throw new ArgumentOutOfRangeException("queryStringParameters");
            }

            // Is this a facebook callback?
            var code = queryStringParameters["code"];

            // Maybe we have an error?
            var errorReason = queryStringParameters["error_reason"];
            var error = queryStringParameters["error"];
            var errorDescription = queryStringParameters["error_description"];
            if (!string.IsNullOrEmpty(errorReason) &&
                !string.IsNullOrEmpty(error) &&
                !string.IsNullOrEmpty(errorDescription))
            {
                var errorMessage = string.Format("Reason: {0}. Error: {1}. Description: {2}.",
                    string.IsNullOrEmpty(errorReason) ? "-no error reason-" : errorReason,
                    string.IsNullOrEmpty(error) ? "-no error-" : error,
                    string.IsNullOrEmpty(errorDescription)
                        ? "-no error description-"
                        : errorDescription);
                TraceSource.TraceVerbose(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            if (string.IsNullOrEmpty(code))
            {
                const string errorMessage = "No code parameter provided in the response query string from Facebook.";
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return code;
        }

        /*
        protected override IRestResponse<AccessTokenResult> ExecuteRetrieveAccessToken(string authorizationCode,
                                                                                       Uri redirectUri)
        {
            if (string.IsNullOrEmpty(authorizationCode))
            {
                throw new ArgumentNullException("authorizationCode");
            }

            if (redirectUri == null ||
                string.IsNullOrEmpty(redirectUri.AbsoluteUri))
            {
                throw new ArgumentNullException("redirectUri");
            }

            var restRequest = new RestRequest();
            restRequest.AddParameter("client_id", PublicApiKey);
            restRequest.AddParameter("client_secret", SecretApiKey);
            restRequest.AddParameter("code", authorizationCode);
            restRequest.AddParameter("redirect_uri", redirectUri.AbsoluteUri.ToLowerInvariant());
            restRequest.AddHeader("Content-Type", "application/json");
            restRequest.AddParameter("format", "json");

            var restClient = RestClientFactory.CreateRestClient(BaseUrl);
            TraceSource.TraceVerbose("Retrieving Access Token endpoint: {0}",
                                     restClient.BuildUri(restRequest).AbsoluteUri);

            // Really really sad hack. Facebook send back all their data as Json except
            // this f'ing endpoint. As such, we'll fuck with things here.
            // We'll manually create the data - if possible.
            // How - we will try and recreate the content result.
            restRequest.OnBeforeDeserialization = response =>
            {
                // Grab the content and convert it into json.
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    // Something is wrong - so just leave. This is handled elsewhere.
                    return;
                }

                // Lets do this!
                var querystringData = HttpUtility.ParseQueryString(response.Content);
                var json = new StringBuilder("{"); // Start.
                
                foreach (var key in querystringData.AllKeys)
                {
                    json.AppendFormat("\"{0}\":\"{1}\"", key, querystringData[key]);
                }

                json.Append("}"); // End.

                response.Content = json.ToString();
                response.ContentType = "text/json";
            };

            return restClient.Execute<AccessTokenResult>(restRequest);
        }
        */

        protected override AccessToken MapAccessTokenResultToAccessToken(AccessTokenResult accessTokenResult)
        {
            if (accessTokenResult == null)
            {
                throw new ArgumentNullException("accessTokenResult");
            }

            if (string.IsNullOrEmpty(accessTokenResult.AccessToken) ||
                accessTokenResult.Expires <= 0)
            {
                var errorMessage =
                    string.Format(
                        "Retrieved a Facebook Access Token but there's an error with either the access_token and/or expires_on parameters. Access Token: {0}. Expires In: {1}.",
                        string.IsNullOrEmpty(accessTokenResult.AccessToken)
                            ? "-no access token-"
                            : accessTokenResult.AccessToken,
                        accessTokenResult.Expires);

                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return new AccessToken
            {
                PublicToken = accessTokenResult.AccessToken,
                ExpiresOn = DateTime.UtcNow.AddSeconds(accessTokenResult.Expires)
            };
        }

        protected override async Task<UserInformation> RetrieveUserInformationAsync(AccessToken accessToken)
        {
            if (accessToken == null)
            {
                throw new ArgumentNullException("accessToken");
            }

            if (string.IsNullOrEmpty(accessToken.PublicToken))
            {
                throw new ArgumentException("accessToken.PublicToken");
            }

            HttpResponseMessage response;

            try
            {
                var requestUrl = new Uri(string.Format("https://graph.facebook.com/me?access_token={0}",
                    accessToken.PublicToken));
                TraceSource.TraceVerbose("Retrieving user information. Facebook Endpoint: {0}",
                    requestUrl.AbsoluteUri);

                using (var client = HttpClientFactory.GetHttpClient())
                {
                    response = await client.GetAsync(requestUrl);
                }
            }
            catch (Exception exception)
            {
                var authenticationException =
                    new AuthenticationException("Failed to retrieve any Me data from the Facebook Api.", exception);
                var errorMessage = authenticationException.RecursiveErrorMessages();
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage, exception);
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = string.Format(
                    "Failed to obtain some 'Me' data from the Facebook api OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}.",
                    response.StatusCode,
                    string.IsNullOrWhiteSpace(response.ReasonPhrase)
                        ? "-no description-"
                        : response.ReasonPhrase);

                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            var meResultJson = await response.Content.ReadAsStringAsync();
            var meResult = JsonConvert.DeserializeObject<MeResult>(meResultJson);

            var id = meResult.Id < 0 ? 0 : meResult.Id;
            var name = string.Format("{0} {1}",
                string.IsNullOrEmpty(meResult.FirstName)
                    ? string.Empty
                    : meResult.FirstName,
                string.IsNullOrEmpty(meResult.LastName)
                    ? string.Empty
                    : meResult.LastName)
                .Trim();

            return new UserInformation
            {
                Id = id.ToString(),
                Name = name,
                Email = meResult.Email,
                Locale = meResult.Locale,
                UserName = meResult.Username,
                Picture = string.Format("https://graph.facebook.com/{0}/picture", id)
            };
        }

        #endregion

        /// <summary>
        /// Are we on a mobile device?
        /// </summary>
        /// <remarks>This will have the side-effect of auto setting the AuthenticateRedirectionUrl property, based on this set value.</remarks>
        public bool IsMobile { get; set; }

        public DisplayType DisplayType { get; set; }

        public override IEnumerable<string> DefaultScopes
        {
            get { return new[] {"email"}; }
        }

        public override string ScopeSeparator
        {
            get { return ","; }
        }

        private Uri GetAccessTokenUri(string redirectUrl,
            string authorizationCode)
        {
            // NOTE: StackOverflow has a waay better solution (http://stackoverflow.com/questions/17096201/build-query-string-for-system-net-httpclient-get)
            //       but it has a dependency on System.Web (urgh!) so I'm not wanting to do that.. so .. I have to have my own.
            //       Urgh^Urgh.

            var queryString = new StringBuilder();
            queryString.AppendFormat("https://graph.facebook.com/oauth/access_token?client_id={0}&client_secret={1}&redirect_uri={2}&code={3}&format=json",
                Uri.EscapeDataString(PublicApiKey),
                Uri.EscapeDataString(SecretApiKey),
                Uri.EscapeDataString(redirectUrl.ToLowerInvariant()),
                Uri.EscapeDataString(authorizationCode));

            if (DisplayType != DisplayType.Unknown)
            {
                 queryString.AppendFormat("&display={0}", Uri.EscapeDataString(DisplayType.ToString().ToLowerInvariant()));
            }

            // Note: Do we need to do anything with scopes?

            return new Uri(queryString.ToString());
        }

        private static AccessTokenResult ExtractAccessTokenFromContent(string content)
        {
            // <RANT>
            // Most of the facebook stuff returns results as json. Nice.
            //  -- E X C E P T --
            //      -- T H I S   A P I   C A L L --
            // FML.
            // </RANT>

            var accessTokenResult = new AccessTokenResult();

            var results = content.Split(new[] {'&'});
            foreach (var result in results)
            {
                var keyValue = result.Split(new[] {'='});
                if (keyValue.First().Equals("access_token"))
                {
                    accessTokenResult.AccessToken = keyValue[1];
                }
                else if (keyValue.First().Equals("expires"))
                {
                    int value = 0;
                    if (int.TryParse(keyValue[1], out value))
                    {
                        accessTokenResult.Expires = value;
                    }
                }
            }

            return accessTokenResult;
        }
    }
}