using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers.Google;
using SimpleAuthentication.Core.Tracing;

namespace SimpleAuthentication.Core.Providers
{
    // REFERENCE: https://developers.google.com/accounts/docs/OAuth2Login

    public class GoogleProvider : BaseOAuthProvider<AccessTokenResult>
    {
        private const string AccessTokenKey = "access_token";
        private const string ExpiresInKey = "expires_in";
        private const string TokenTypeKey = "token_type";

        public GoogleProvider(ProviderParams providerParams) : base("Google", providerParams, "OAuth 2.0")
        {
        }

        #region BaseOAuth20Token<AccessTokenResult> Implementation

        public override Uri AuthenticateRedirectionUrl
        {
            get { return new Uri("https://accounts.google.com/o/oauth2/auth"); }
        }

        public override IEnumerable<string> DefaultScopes
        {
            get
            {
                return new[]
                {
                    "https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email"
                };
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
                var postData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", PublicApiKey),
                    new KeyValuePair<string, string>("client_secret", SecretApiKey),
                    new KeyValuePair<string, string>("redirect_uri", redirectUrl.AbsoluteUri),
                    new KeyValuePair<string, string>("code", authorizationCode),
                    new KeyValuePair<string, string>("grant_type", "authorization_code")
                };

                var content = new FormUrlEncodedContent(postData);

                var requestUri = new Uri("https://accounts.google.com/o/oauth2/token");

                TraceSource.TraceVerbose("Retrieving Access Token endpoint: {0}",
                    requestUri.AbsoluteUri);

                response = await client.PostAsync(requestUri, content);
            }

            var jsonContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                TraceSource.TraceWarning("No Access Token Result retrieved from Google. Error Status Code: {0}. Error Message: {1}",
                    response.StatusCode,
                    jsonContent);
                return null;
            }

            var result = JsonConvert.DeserializeObject<dynamic>(jsonContent);
            if (result == null)
            {
                TraceSource.TraceWarning("No Access Token Result retrieved from Google.");
            }

            return MapDynamicResultToAccessTokenResult(result);

        }

        protected override AccessToken MapAccessTokenResultToAccessToken(AccessTokenResult accessTokenResult)
        {
            if (accessTokenResult == null)
            {
                throw new ArgumentNullException("accessTokenResult");
            }

            if (string.IsNullOrWhiteSpace(accessTokenResult.AccessToken) ||
                accessTokenResult.ExpiresIn <= 0 ||
                string.IsNullOrWhiteSpace(accessTokenResult.TokenType))
            {
                var errorMessage =
                    string.Format(
                        "Retrieved a Google Access Token but it doesn't contain one or more of either: {0}, {1} or {2}.",
                        AccessTokenKey, ExpiresInKey, TokenTypeKey);
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return new AccessToken
            {
                PublicToken = accessTokenResult.AccessToken,
                ExpiresOn = DateTime.UtcNow.AddSeconds(accessTokenResult.ExpiresIn)
            };
        }

        protected override async Task<UserInformation> RetrieveUserInformationAsync(AccessToken accessToken)
        {
            if (accessToken == null)
            {
                throw new ArgumentNullException("accessToken");
            }

            if (string.IsNullOrWhiteSpace(accessToken.PublicToken))
            {
                throw new ArgumentException("accessToken.PublicToken");
            }

            UserInfoResult userInfoResult;

            try
            {
                string jsonResponse;

                using (var client = HttpClientFactory.GetHttpClient())
                {
                    var requestUri = new Uri(string.Format("https://www.googleapis.com/oauth2/v2/userinfo?{0}={1}",
                        AccessTokenKey, accessToken.PublicToken));

                    TraceSource.TraceVerbose("Retrieving user information. Google Endpoint: {0}",
                        requestUri.AbsoluteUri);

                    jsonResponse = await client.GetStringAsync(requestUri);
                }

                userInfoResult = JsonConvert.DeserializeObject<UserInfoResult>(jsonResponse);
            }
            catch (Exception exception)
            {
                var errorMessage =
                    string.Format("Failed to retrieve any UserInfo data from the Google Api. Error Messages: {0}",
                        exception.RecursiveErrorMessages());
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage, exception);
            }

            return new UserInformation
            {
                Id = userInfoResult.Id,
                Gender = string.IsNullOrWhiteSpace(userInfoResult.Gender)
                    ? GenderType.Unknown
                    : GenderTypeHelpers.ToGenderType(userInfoResult.Gender),
                Name = userInfoResult.Name,
                Email = userInfoResult.Email,
                Locale = userInfoResult.Locale,
                Picture = userInfoResult.Picture,
                UserName = userInfoResult.GivenName
            };
        }

        #endregion

        private static AccessTokenResult MapDynamicResultToAccessTokenResult(dynamic result)
        {
            if (result == null)
            {
                
                return null;
            }

            var accessTokenResult = new AccessTokenResult
            {
                AccessToken = result.access_token,
                TokenType = result.token_type,
                ExpiresIn = result.expires_in,
                IdToken = result.id_token
            };

            return accessTokenResult;
        }
    }
}