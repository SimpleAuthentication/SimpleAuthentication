using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers.OAuth.V20;
using SimpleAuthentication.ExtraProviders.Instagram;

namespace SimpleAuthentication.ExtraProviders
{
    public class InstagramProvider : OAuth20Provider
    {
        public InstagramProvider(ProviderParams providerParams)
            : base(providerParams)
        {
        }

        #region OAuth20Provider Implementation

        public override string Name
        {
            get { return "Instagram"; }
        }

        protected override IEnumerable<string> DefaultScopes
        {
            get { return new[] {"basic"}; }
        }

        protected override Uri AccessTokenUri
        {
            get { return new Uri("https://api.instagram.com/oauth/access_token"); }
        }

        protected override AccessToken MapAccessTokenContentToAccessToken(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException("content");
            }

            var accessToken = JsonConvert.DeserializeObject<dynamic>(content);

            string token = accessToken.access_token;
            if (string.IsNullOrWhiteSpace(token))
            {
                var errorMessage =
                    string.Format(
                        "No 'access_token' key/value found in the {0} access token response. Content retrieved: {1}.",
                        Name,
                        content);
                throw new AuthenticationException(errorMessage);
            }

            // NOTE: Instagram doesn't include an EXPIRES ON value.
            return new AccessToken
            {
                Token = accessToken.access_token,
                ExpiresOn = DateTime.MaxValue
            };
        }

        protected override Uri UserInformationUri(AccessToken accessToken)
        {
            var requestUri = string.Format("https://api.instagram.com/v1/users/self?access_token={0}",
                accessToken.Token);
            return new Uri(requestUri);
        }

        protected override UserInformation GetUserInformationFromContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException("content");
            }

            var userInfo = JsonConvert.DeserializeObject<UserInfo>(content);
            
            return new UserInformation
            {
                Id = userInfo.Data.Id,
                Name = userInfo.Data.FullName,
                UserName = userInfo.Data.UserName,
                Picture = userInfo.Data.ProfilePicture
            };
        }

        public override async Task<RedirectToAuthenticateSettings> GetRedirectToAuthenticateSettingsAsync(Uri callbackUrl)
        {
            if (callbackUrl == null)
            {
                throw new ArgumentNullException("callbackUrl");
            }

            var providerAuthenticationUrl = new Uri("https://api.instagram.com/oauth/authorize/");
            return GetRedirectToAuthenticateSettings(callbackUrl, providerAuthenticationUrl);
        }

        #endregion

        /*

        public override string ScopeSeparator
        {
            get { return " "; }
        }

        protected override IRestResponse<AccessTokenResult> ExecuteRetrieveAccessToken(string authorizationCode, Uri redirectUri)
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

            var restRequest = new RestRequest("https://api.instagram.com/oauth/access_token", Method.POST);
            restRequest.AddParameter("client_id", PublicApiKey);
            restRequest.AddParameter("client_secret", SecretApiKey);
            restRequest.AddParameter("redirect_uri", redirectUri.AbsoluteUri);
            restRequest.AddParameter("code", authorizationCode);
            restRequest.AddParameter("grant_type", "authorization_code");

            var restClient = RestClientFactory.CreateRestClient("https://api.instagram.com");
            return restClient.Execute<AccessTokenResult>(restRequest);
        }

        protected override AccessToken MapAccessTokenResultToAccessToken(AccessTokenResult accessTokenResult)
        {
            if (accessTokenResult == null)
            {
                throw new ArgumentNullException("accessTokenResult");
            }

            if (string.IsNullOrEmpty(accessTokenResult.AccessToken))
            {
                var errorMessage =
                    string.Format(
                        "Retrieved an Instagram Access Token but it doesn't contain {0}.",
                        AccessTokenKey);
                throw new AuthenticationException(errorMessage);
            }

            return new AccessToken
            {
                PublicToken = accessTokenResult.AccessToken
            };
        }

        protected override UserInformation RetrieveUserInformation(AccessToken accessToken)
        {
            if (accessToken == null)
            {
                throw new ArgumentNullException("accessToken");
            }

            if (string.IsNullOrEmpty(accessToken.PublicToken))
            {
                throw new ArgumentException("accessToken.PublicToken");
            }

            IRestResponse<UserInfoResult> response;

            try
            {
                var restRequest = new RestRequest("https://api.instagram.com/v1//users/self", Method.GET);
                restRequest.AddParameter(AccessTokenKey, accessToken.PublicToken);
                var restClient = RestClientFactory.CreateRestClient("https://api.instagram.com/v1/");
                restClient.UserAgent = PublicApiKey;
                response = restClient.Execute<UserInfoResult>(restRequest);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Failed to obtain User Info from Instagram.", exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                throw new AuthenticationException(
                    string.Format(
                        "Failed to obtain User Info from Instagram OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}",
                        response == null ? "-- null response --" : response.StatusCode.ToString(),
                        response == null ? string.Empty : response.StatusDescription));
            }

            // Lets check to make sure we have some bare minimum data.
            if (string.IsNullOrEmpty(response.Data.Data.Id) ||
                string.IsNullOrEmpty(response.Data.Data.Username))
            {
                throw new AuthenticationException(
                    string.Format(
                        "Retrieve some user info from the Instagram API, but we're missing one or both: Id: '{0}' and Username: '{1}'.",
                        string.IsNullOrEmpty(response.Data.Data.Id) ? "--missing--" : response.Data.Data.Id,
                        string.IsNullOrEmpty(response.Data.Data.Username) ? "--missing--" : response.Data.Data.Username));
            }

            return new UserInformation
            {
                Id = response.Data.Data.Id,
                Name = response.Data.Data.FullName,
                Picture = response.Data.Data.ProfilePicture,
                UserName = response.Data.Data.Username
            };
        }
    }
         * 
         **/
    }
}