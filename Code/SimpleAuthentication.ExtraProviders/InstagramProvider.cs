using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers;
using SimpleAuthentication.ExtraProviders.Instagram;

namespace SimpleAuthentication.ExtraProviders
{
    public class InstagramProvider : BaseOAuth20Provider<AccessTokenResult>
    {
        private const string AccessTokenKey = "access_token";

        public InstagramProvider(ProviderParams providerParams) : this("Instagram", providerParams)
        {
        }

        protected InstagramProvider(string name, ProviderParams providerParams) : base(name, providerParams)
        {
            AuthenticateRedirectionUrl = new Uri("https://api.instagram.com/oauth/authorize/");
        }

        public override IEnumerable<string> DefaultScopes
        {
            get { return new[] { "basic" }; }
        }

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

            var restRequest = new RestRequest("/oauth/access_token", Method.POST);
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
                var restRequest = new RestRequest("/users/self", Method.GET);
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
}