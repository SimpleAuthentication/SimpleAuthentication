//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Http;
//using System.Threading.Tasks;
//using RestSharp;
//using SimpleAuthentication.Core.Exceptions;
//using SimpleAuthentication.Core.Providers.WindowsLive;
//using SimpleAuthentication.Core.Tracing;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleAuthentication.Core.Providers.OAuth.V20;
using SimpleAuthentication.Core.Providers.WindowsLive;

namespace SimpleAuthentication.Core.Providers
{
    public class WindowsLiveProvider : OAuth20Provider
    {
        public WindowsLiveProvider(ProviderParams providerParams) : base(providerParams)
        {
        }

        #region IAuthenticationProvider Implementation

        public override string Name
        {
            get { return "WindowsLive"; }
        }

        public override async Task<RedirectToAuthenticateSettings> GetRedirectToAuthenticateSettingsAsync(
            Uri callbackUrl)
        {
            if (callbackUrl == null)
            {
                throw new ArgumentNullException("callbackUrl");
            }

            var providerAuthenticationUrl = new Uri("https://login.live.com/oauth20_authorize.srf");
            return GetRedirectToAuthenticateSettings(callbackUrl, providerAuthenticationUrl);
        }

        #endregion

        #region OAuth20Provider Implementation

        protected override IEnumerable<string> DefaultScopes
        {
            get { return new[] { "wl.signin", "wl.basic", "wl.emails" }; }
        }

        protected override Uri AccessTokenUri
        {
            get { return new Uri("https://login.live.com/oauth20_token.srf"); }
        }

        protected override Uri UserInformationUri(AccessToken accessToken)
        {
            var requestUrl = string.Format("https://apis.live.net/v5.0/me?access_token={0}",
                accessToken.Token);

            return new Uri(requestUrl);
        }

        protected override UserInformation GetUserInformationFromContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException("content");
            }

            var result = JsonConvert.DeserializeObject<UserInfoResult>(content);

            var name = string.Format("{0} {1}",
                string.IsNullOrWhiteSpace(result.FirstName)
                    ? string.Empty
                    : result.FirstName,
                string.IsNullOrWhiteSpace(result.LastName)
                    ? string.Empty
                    : result.LastName);
            var picture = string.Format("https://apis.live.net/v5.0/{0}/picture", result.Id);
            var gender = (GenderType) Enum.Parse(typeof (GenderType), result.Gender ?? "Unknown", true);

            // NOTE: No username field provided.
            return new UserInformation
            {
                Name = result.Name,
                Locale = result.Locale,
                Id = result.Id,
                Email = result.Emails == null
                    ? null
                    : result.Emails.Preferred,
                Picture = picture,
                Gender = gender
            };
        }

        #endregion
    }

//    public class WindowsLiveProvider : BaseOAuthProvider<AccessTokenResult>
//    {
//        // *********************************************************************
//        // REFERENCE: http://msdn.microsoft.com/en-us/library/live/hh243647.aspx
//        // *********************************************************************

//        private const string AccessTokenKey = "access_token";

//        public WindowsLiveProvider(ProviderParams providerParams) : base("WindowsLive", providerParams, "OAuth 2.0")
//        {
//        }

//        #region BaseOAuth20Token<AccessTokenResult> Implementation

//        public override Uri AuthenticateRedirectionUrl
//        {
//            get { return new Uri("https://accounts.google.com/o/oauth2/auth"); }
//        }

//        public override IEnumerable<string> DefaultScopes
//        {
//            get { return new[] { "wl.signin", "wl.basic", "wl.emails" }; }
//        }

//        protected override async Task<AccessTokenResult> GetAccessTokenFromProviderAsync(string authorizationCode,
//            Uri redirectUrl)
//        {
//            if (string.IsNullOrWhiteSpace(authorizationCode))
//            {
//                throw new ArgumentNullException("authorizationCode");
//            }

//            if (redirectUrl == null ||
//                string.IsNullOrWhiteSpace(redirectUrl.AbsoluteUri))
//            {
//                throw new ArgumentNullException("redirectUrl");
//            }

//            HttpResponseMessage response;

//            using (var client = HttpClientFactory.GetHttpClient())
//            {
//                var postData = new List<KeyValuePair<string, string>>
//                {
//                    new KeyValuePair<string, string>("client_id", PublicApiKey),
//                    new KeyValuePair<string, string>("client_secret", SecretApiKey),
//                    new KeyValuePair<string, string>("redirect_uri", redirectUrl.AbsoluteUri),
//                    new KeyValuePair<string, string>("code", authorizationCode),
//                    new KeyValuePair<string, string>("grant_type", "authorization_code")
//                };

//                var content = new FormUrlEncodedContent(postData);

//                var requestUri = new Uri("https://login.live.com/oauth20_token.srf");

//                TraceSource.TraceVerbose("Retrieving Access Token endpoint: {0}",
//                    requestUri.AbsoluteUri);

//                response = await client.PostAsync(requestUri, content);
//            }

//            var jsonContent = await response.Content.ReadAsStringAsync();

//            if (response.StatusCode != HttpStatusCode.OK)
//            {
//                TraceSource.TraceWarning("No Access Token Result retrieved from Google. Error Status Code: {0}. Error Message: {1}",
//                    response.StatusCode,
//                    jsonContent);
//                return null;
//            }

//            var result = JsonConvert.DeserializeObject<dynamic>(jsonContent);
//            if (result == null)
//            {
//                TraceSource.TraceWarning("No Access Token Result retrieved from Google.");
//            }

//            return MapDynamicResultToAccessTokenResult(result);

//        }

//        protected override AccessToken MapAccessTokenResultToAccessToken(AccessTokenResult accessTokenResult)
//        {
//            if (accessTokenResult == null)
//            {
//                throw new ArgumentNullException("accessTokenResult");
//            }

//            if (string.IsNullOrWhiteSpace(accessTokenResult.AccessToken) ||
//                accessTokenResult.ExpiresIn <= 0 ||
//                string.IsNullOrWhiteSpace(accessTokenResult.TokenType))
//            {
//                var errorMessage =
//                    string.Format(
//                        "Retrieved a Google Access Token but it doesn't contain one or more of either: {0}, {1} or {2}.",
//                        AccessTokenKey, ExpiresInKey, TokenTypeKey);
//                TraceSource.TraceError(errorMessage);
//                throw new AuthenticationException(errorMessage);
//            }

//            return new AccessToken
//            {
//                PublicToken = accessTokenResult.AccessToken,
//                ExpiresOn = DateTime.UtcNow.AddSeconds(accessTokenResult.ExpiresIn)
//            };
//        }

//        protected override async Task<UserInformation> RetrieveUserInformationAsync(AccessToken accessToken)
//        {
//            if (accessToken == null)
//            {
//                throw new ArgumentNullException("accessToken");
//            }

//            if (string.IsNullOrWhiteSpace(accessToken.PublicToken))
//            {
//                throw new ArgumentException("accessToken.PublicToken");
//            }

//            UserInfoResult userInfoResult;

//            try
//            {
//                string jsonResponse;

//                using (var client = HttpClientFactory.GetHttpClient())
//                {
//                    var requestUri = new Uri(string.Format("https://www.googleapis.com/oauth2/v2/userinfo?{0}={1}",
//                        AccessTokenKey, accessToken.PublicToken));

//                    TraceSource.TraceVerbose("Retrieving user information. Google Endpoint: {0}",
//                        requestUri.AbsoluteUri);

//                    jsonResponse = await client.GetStringAsync(requestUri);
//                }

//                userInfoResult = JsonConvert.DeserializeObject<UserInfoResult>(jsonResponse);
//            }
//            catch (Exception exception)
//            {
//                var errorMessage =
//                    string.Format("Failed to retrieve any UserInfo data from the Google Api. Error Messages: {0}",
//                        exception.RecursiveErrorMessages());
//                TraceSource.TraceError(errorMessage);
//                throw new AuthenticationException(errorMessage, exception);
//            }

//            return new UserInformation
//            {
//                Id = userInfoResult.Id,
//                Gender = string.IsNullOrWhiteSpace(userInfoResult.Gender)
//                    ? GenderType.Unknown
//                    : GenderTypeHelpers.ToGenderType(userInfoResult.Gender),
//                Name = userInfoResult.Name,
//                Email = userInfoResult.Email,
//                Locale = userInfoResult.Locale,
//                Picture = userInfoResult.Picture,
//                UserName = userInfoResult.GivenName
//            };
//        }

//        #endregion


//        #region BaseOAuth20Token<AccessTokenResult> Implementation

//        protected override IRestResponse<AccessTokenResult> ExecuteRetrieveAccessToken(string authorizationCode,
//                                                                                       Uri redirectUri)
//        {
//            if (string.IsNullOrEmpty(authorizationCode))
//            {
//                throw new ArgumentNullException("authorizationCode");
//            }

//            if (redirectUri == null ||
//                string.IsNullOrEmpty(redirectUri.AbsoluteUri))
//            {
//                throw new ArgumentNullException("redirectUri");
//            }

//            var restRequest = new RestRequest("/oauth20_token.srf");
//            restRequest.AddParameter("client_id", PublicApiKey);
//            restRequest.AddParameter("redirect_uri", redirectUri);
//            restRequest.AddParameter("client_secret", SecretApiKey);
//            restRequest.AddParameter("code", authorizationCode);
//            restRequest.AddParameter("grant_type", "authorization_code");

//            var restClient = RestClientFactory.CreateRestClient("https://login.live.com/oauth20_token.srf");
//            TraceSource.TraceVerbose("Retrieving Access Token endpoint: {0}",
//                                     restClient.BuildUri(restRequest).AbsoluteUri);

//            return restClient.Execute<AccessTokenResult>(restRequest);
//        }

//        protected override AccessToken MapAccessTokenResultToAccessToken(AccessTokenResult accessTokenResult)
//        {
//            if (accessTokenResult == null)
//            {
//                throw new ArgumentNullException("accessTokenResult");
//            }

//            if (string.IsNullOrEmpty(accessTokenResult.AccessToken))
//            {
//                var errorMessage =
//                    string.Format(
//                        "Retrieved a Windows Live Access Token but it there's an error with either the access_token parameters. Access Token: {0}.",
//                        string.IsNullOrEmpty(accessTokenResult.AccessToken)
//                            ? "-no access token-"
//                            : accessTokenResult.AccessToken);

//                TraceSource.TraceError(errorMessage);
//                throw new AuthenticationException(errorMessage);
//            }

//            return new AccessToken
//                   {
//                       PublicToken = accessTokenResult.AccessToken,
//                       // TODO: Wire up the ExpiresIn .. but right now it's a string.. what should it -really- be?
//                       //ExpiresOn = DateTime.UtcNow.AddSeconds(response.Data.ExpiresIn)
//                   };
//        }

//        protected override UserInformation RetrieveUserInformation(AccessToken accessToken)
//        {
//            if (accessToken == null)
//            {
//                throw new ArgumentNullException("accessToken");
//            }

//            if (string.IsNullOrEmpty(accessToken.PublicToken))
//            {
//                throw new ArgumentException("accessToken.PublicToken");
//            }

//            IRestResponse<UserInfoResult> response;

//            try
//            {
//                var restRequest = new RestRequest("/v5.0/me");
//                restRequest.AddParameter(AccessTokenKey, accessToken.PublicToken);

//                var restClient = RestClientFactory.CreateRestClient("https://apis.live.net");
//                TraceSource.TraceVerbose("Retrieving user information. Microsoft Live Endpoint: {0}",
//                                         restClient.BuildUri(restRequest).AbsoluteUri);

//                response = restClient.Execute<UserInfoResult>(restRequest);
//            }
//            catch (Exception exception)
//            {
//                var errorMessage =
//                    string.Format("Failed to retrieve any Me data from the Microsoft Live api. Error Messages: {0}",
//                                  exception.RecursiveErrorMessages());
//                TraceSource.TraceError(errorMessage);
//                throw new AuthenticationException(errorMessage, exception);
//            }

//            if (response == null ||
//                response.StatusCode != HttpStatusCode.OK)
//            {
//                var errorMessage = string.Format(
//                    "Failed to obtain some 'Me' data from the Microsoft Live api OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}. Error Message: {2}.",
//                    response == null ? "-- null response --" : response.StatusCode.ToString(),
//                    response == null ? string.Empty : response.StatusDescription,
//                    response == null
//                        ? string.Empty
//                        : response.ErrorException == null
//                              ? "--no error exception--"
//                              : response.ErrorException.RecursiveErrorMessages());

//                TraceSource.TraceError(errorMessage);
//                throw new AuthenticationException(errorMessage);
//            }

//            // Lets check to make sure we have some bare minimum data.
//            if (string.IsNullOrEmpty(response.Data.id))
//            {
//                const string errorMessage =
//                    "We were unable to retrieve the User Id from Windows Live Api, the user may have denied the authorization.";
//                TraceSource.TraceError(errorMessage);
//                throw new AuthenticationException(errorMessage);
//            }

//            return new UserInformation
//                   {
//                       Name = string.Format("{0} {1}",
//                                            string.IsNullOrEmpty(response.Data.first_name)
//                                                ? string.Empty
//                                                : response.Data.first_name,
//                                            string.IsNullOrEmpty(response.Data.last_name)
//                                                ? string.Empty
//                                                : response.Data.last_name).Trim(),
//                       Locale = response.Data.locale,
//                       UserName = response.Data.name,
//                       Id = response.Data.id,
//                       Email = response.Data.emails.Preferred,
//                       Gender = (GenderType) Enum.Parse(typeof (GenderType), response.Data.gender ?? "Unknown", true)
//                   };
//        }

//        #endregion
//    }
}