using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers.Google;
using SimpleAuthentication.Core.Providers.OAuth.V20;
using SimpleAuthentication.Core.Tracing;

namespace SimpleAuthentication.Core.Providers
{
    public class GoogleProvider : OAuth20Provider
    {
        public GoogleProvider(ProviderParams providerParams) : base(providerParams)
        {
            if (providerParams.Scopes == null ||
                !providerParams.Scopes.Any())
            {
                // Default our scopes if none have been provided.
                Scopes = new[]
                       {
                           "https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email"
                       };
            }
        }

        #region IAuthenticationProvider Implementation

        public override string Name
        {
            get { return "Google"; }
        }

        public override string Description
        {
            get { return "OAuth 2.0"; }
        }

        public override async Task<RedirectToAuthenticateSettings> GetRedirectToAuthenticateSettingsAsync(Uri callbackUrl)
        {
            if (callbackUrl == null)
            {
                throw new ArgumentNullException("callbackUrl");
            }

            var providerAuthenticationUrl = new Uri("https://accounts.google.com/o/oauth2/auth");
            return GetRedirectToAuthenticateSettings(callbackUrl, providerAuthenticationUrl);
        }

        #endregion

        #region OAuth20Provider Implementation

        protected override Uri AccessTokenUri
        {
            get { return new Uri("https://accounts.google.com/o/oauth2/token"); }
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

            UserInfoResult userInfoResult;

            try
            {
                string jsonResponse;

                using (var client = HttpClientFactory.GetHttpClient())
                {
                    var requestUri = new Uri(string.Format("https://www.googleapis.com/oauth2/v2/userinfo?access_token={0}", accessToken.Token));

                    //TraceSource.TraceVerbose("Retrieving user information. Google Endpoint: {0}",
                    //    requestUri.AbsoluteUri);

                    jsonResponse = await client.GetStringAsync(requestUri);
                }

                userInfoResult = JsonConvert.DeserializeObject<UserInfoResult>(jsonResponse);
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

        protected override AccessToken MapAccessTokenContentToAccessToken(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException("content");
            }

            var result = JsonConvert.DeserializeObject<dynamic>(content);
            var expiresIn = Convert.ToDouble(result.expires_in, CultureInfo.InvariantCulture);
            return new AccessToken
            {
                Token = result.access_token,
                ExpiresOn = expiresIn > 0
                    ? DateTime.UtcNow.AddSeconds(expiresIn)
                    : DateTime.MaxValue
            };
        }

        #endregion
    }
}