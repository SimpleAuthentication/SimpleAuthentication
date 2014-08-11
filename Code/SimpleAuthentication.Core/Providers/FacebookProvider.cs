using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers.Facebook;
using SimpleAuthentication.Core.Providers.Google;
using SimpleAuthentication.Core.Tracing;
using AccessTokenResult = SimpleAuthentication.Core.Providers.Facebook.AccessTokenResult;

namespace SimpleAuthentication.Core.Providers
{
    // REFERENCE: https://developers.facebook.com/docs/facebook-login/login-flow-for-web-no-jssdk/

    public class FacebookProvider : OAuth20Provider
    {
        public FacebookProvider(ProviderParams providerParams) : base(providerParams)
        {
            DisplayType = DisplayType.Unknown;
            IsMobile = false;
        }

        public DisplayType DisplayType { get; set; }
        public bool IsMobile { get; set; }

        #region IAuthenticationProvider Implementation

        public override string Name
        {
            get { return "Facebook"; }
        }

        public override string Description
        {
            get { return "OAuth 2.0"; }
        }

        #endregion

        #region OAuth20Provider Implementation

        public override RedirectToAuthenticateSettings GetRedirectToAuthenticateSettings(Uri callbackUrl)
        {
            if (callbackUrl == null)
            {
                throw new ArgumentNullException("callbackUrl");
            }

            var url = IsMobile
                    ? "https://m.facebook.com/dialog/oauth"
                    : "https://www.facebook.com/dialog/oauth";

            var providerAuthenticationUrl = new Uri(url);
            return GetRedirectToAuthenticateSettings(callbackUrl, providerAuthenticationUrl);
        }

        protected override async Task<AccessToken> GetAccessTokenAsync(string authorizationCode, Uri redirectUrl)
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

                var encodedContent = new FormUrlEncodedContent(postData);

                var requestUri = new Uri("https://graph.facebook.com/oauth/access_token");

                //TraceSource.TraceVerbose("Retrieving Access Token endpoint: {0}",
                //    requestUri.AbsoluteUri);

                response = await client.PostAsync(requestUri, encodedContent);
            }

            // RANT: Facebook send back all their data as Json except this f'ing endpoint.
            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorMessage =
                    string.Format(
                        "Failed to retrieve an Access Token from Facebook. Status Code: {0}. Error Message: {1}",
                        response.StatusCode,
                        content);

                throw new AuthenticationException(errorMessage);
            }

            return MapDynamicResultToAccessToken(content);
        }

        protected override async Task<UserInformation> GetUserInformationAsync(AccessToken accessToken)
        {
            if (accessToken == null)
            {
                throw new ArgumentNullException("accessToken");
            }

            if (string.IsNullOrWhiteSpace(accessToken.Token))
            {
                throw new ArgumentException("accessToken.Token");
            }

            MeResult meResult;

            try
            {
                string jsonResponse;

                using (var client = HttpClientFactory.GetHttpClient())
                {
                    var requestUri = new Uri(string.Format("https://graph.facebook.com/v2.0/me?fields=id,name,gender,email,link,locale&access_token={0}", accessToken.Token));

                    //TraceSource.TraceVerbose("Retrieving user information. Google Endpoint: {0}",
                    //    requestUri.AbsoluteUri);

                    jsonResponse = await client.GetStringAsync(requestUri);
                }

                meResult = JsonConvert.DeserializeObject<MeResult>(jsonResponse);
            }
            catch (Exception exception)
            {
                var errorMessage =
                    string.Format("Failed to retrieve any UserInfo data from the Google Api. Error Messages: {0}",
                        exception.RecursiveErrorMessages());
                //TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage, exception);
            }

            return new UserInformation
            {
                Id = meResult.Id < 0
                    ? "0"
                    : meResult.Id.ToString(),
                Name = meResult.Name.Trim(),
                Email = meResult.Email,
                Locale = meResult.Locale,
                UserName = meResult.Username,
                Gender = string.IsNullOrWhiteSpace(meResult.Gender)
                    ? GenderType.Unknown
                    : GenderTypeHelpers.ToGenderType(meResult.Gender),
                Picture = string.Format("https://graph.facebook.com/{0}/picture", meResult.Id)
            };
        }

        #endregion

        private static AccessToken MapDynamicResultToAccessToken(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException("content");
            }

            // The data is in a key/value format. So lets split this data out.
            var keyValues = SystemHelpers.ConvertKeyValueContentToDictionary(content);
            if (keyValues == null ||
                !keyValues.Any())
            {
                var errorMessage =
                    string.Format("Failed to extract the access token from the body content. Content returned: {0}",
                        content);
                throw new AuthenticationException(errorMessage);
            }

            const string tokenKey = "access_token";
            const string expiresOnKey = "expires";

            if (!keyValues.ContainsKey(tokenKey) &&
                !keyValues.ContainsKey(expiresOnKey))
            {
                var errorMessage =
                    string.Format(
                        "Failed to extract the access token from the body content. Content needs both the '{0}' and '{1}' keys.",
                        tokenKey,
                        expiresOnKey);
                throw new AuthenticationException(errorMessage);
            }

            var expiresIn = Convert.ToDouble(keyValues[expiresOnKey], CultureInfo.InvariantCulture);
            return new AccessToken
            {
                Token = keyValues[tokenKey],
                ExpiresOn = expiresIn > 0
                    ? DateTime.UtcNow.AddSeconds(expiresIn)
                    : DateTime.MaxValue
            };
        }
    }
}