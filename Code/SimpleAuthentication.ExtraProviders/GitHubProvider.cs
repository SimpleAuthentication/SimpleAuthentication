using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;
using SimpleAuthentication.ExtraProviders.GitHub;
using SimpleAuthentication.Providers;
using SimpleAuthentication.Tracing;

namespace SimpleAuthentication.ExtraProviders
{
    public class GitHubProvider : BaseOAuth20Provider<AccessTokenResult>
    {
        private const string AccessTokenKey = "access_token";
        private const string TokenTypeKey = "token_type";

        public GitHubProvider(ProviderParams providerParams) : base("GitHub", providerParams)
        {
            AuthenticateRedirectionUrl = new Uri("https://github.com/login/oauth/authorize");
        }

        #region BaseOAuth20Token<AccessTokenResult> Implementation

        public override IEnumerable<string> DefaultScopes
        {
            get { return new[] {"user:email"}; }
        }

        public override string ScopeKey
        {
            get { return ","; }
        }

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

            var restRequest = new RestRequest("/login/oauth/access_token", Method.POST);
            restRequest.AddParameter("client_id", PublicApiKey);
            restRequest.AddParameter("client_secret", SecretApiKey);
            restRequest.AddParameter("redirect_uri", redirectUri.AbsoluteUri);
            restRequest.AddParameter("code", authorizationCode);
            restRequest.AddParameter("grant_type", "authorization_code");

            var restClient = RestClientFactory.CreateRestClient("https://github.com");
            TraceSource.TraceVerbose("Retrieving Access Token endpoint: {0}",
                                     restClient.BuildUri(restRequest).AbsoluteUri);

            return restClient.Execute<AccessTokenResult>(restRequest);
        }

        protected override AccessToken MapAccessTokenResultToAccessToken(AccessTokenResult accessTokenResult)
        {
            if (accessTokenResult == null)
            {
                throw new ArgumentNullException("accessTokenResult");
            }

            if (string.IsNullOrEmpty(accessTokenResult.AccessToken) ||
                string.IsNullOrEmpty(accessTokenResult.TokenType))
            {
                var errorMessage =
                    string.Format(
                        "Retrieved a GitHub Access Token but it doesn't contain one or more of either: {0} or {1}.",
                        AccessTokenKey, TokenTypeKey);
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return new AccessToken
            {
                PublicToken = accessTokenResult.AccessToken
                //ExpiresOn = DateTime.UtcNow.AddSeconds(accessTokenResult.ExpiresIn)
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
                var request = new RestRequest("/user", Method.GET);
                request.AddParameter(AccessTokenKey, accessToken.PublicToken);

                var restClient = RestClientFactory.CreateRestClient("https://api.github.com");
                response = restClient.Execute<UserInfoResult>(request);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Failed to obtain User Info from GitHub.", exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                throw new AuthenticationException(
                    string.Format(
                        "Failed to obtain User Info from GitHub OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}",
                        response == null ? "-- null response --" : response.StatusCode.ToString(),
                        response == null ? string.Empty : response.StatusDescription));
            }

            // Lets check to make sure we have some bare minimum data.
            if (string.IsNullOrEmpty(response.Data.Id.ToString()) ||
                string.IsNullOrEmpty(response.Data.Login) ||
                string.IsNullOrEmpty(response.Data.Name))
            {
                throw new AuthenticationException(
                    "Retrieve some user info from the GitHub Api, but we're missing one or more of either: Id, Login, and Name.");
            }

            return new UserInformation
            {
                Id = response.Data.Id.ToString(),
                Name = response.Data.Name,
                Email = response.Data.Email ?? "",
                Picture = response.Data.AvatarUrl,
                UserName = response.Data.Login
            };
        }

        #endregion
    }
}