using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers;
using SimpleAuthentication.Core.Tracing;
using SimpleAuthentication.ExtraProviders.LinkedIn;

namespace SimpleAuthentication.ExtraProviders
{
    // REFERENCE: https://developers.LinkedIn.com/accounts/docs/OAuth2Login

    public class LinkedInProvider : BaseOAuth20Provider<AccessTokenResult>
    {
        private const string AccessTokenKey = "oauth2_access_token";
        private const string ExpiresInKey = "expires_in";

        public LinkedInProvider(ProviderParams providerParams) : this("LinkedIn", providerParams)
        {
        }

        protected LinkedInProvider(string name, ProviderParams providerParams) : base(name, providerParams)
        {
            AuthenticateRedirectionUrl = new Uri("https://www.linkedin.com/uas/oauth2/authorization");
        }

        #region BaseOAuth20Token<AccessTokenResult> Implementation

        public override IEnumerable<string> DefaultScopes
        {
            get { return new[] {"r_basicprofile", "r_emailaddress"}; }
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

            var restRequest = new RestRequest("/uas/oauth2/accessToken", Method.POST);
            restRequest.AddParameter("client_id", PublicApiKey);
            restRequest.AddParameter("client_secret", SecretApiKey);
            restRequest.AddParameter("redirect_uri", redirectUri.AbsoluteUri);
            restRequest.AddParameter("code", authorizationCode);
            restRequest.AddParameter("grant_type", "authorization_code");

            var restClient = RestClientFactory.CreateRestClient("https://www.linkedin.com");

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
                accessTokenResult.ExpiresIn <= 0)
            {
                var errorMessage =
                    string.Format(
                        "Retrieved a LinkedIn Access Token but it doesn't contain one or more of either: {0} or {1}.",
                        AccessTokenKey, ExpiresInKey);
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return new AccessToken
                   {
                       PublicToken = accessTokenResult.AccessToken,
                       ExpiresOn = DateTime.UtcNow.AddSeconds(accessTokenResult.ExpiresIn)
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
                var restRequest = new RestRequest("/v1/people/~:(id,first-name,last-name,email-address)", Method.GET);
                restRequest.AddParameter(AccessTokenKey, accessToken.PublicToken);
                var restClient = RestClientFactory.CreateRestClient("https://api.linkedin.com/");

                TraceSource.TraceVerbose("Retrieving user information. LinkedIn Endpoint: {0}",
                                         restClient.BuildUri(restRequest).AbsoluteUri);

                response = restClient.Execute<UserInfoResult>(restRequest);
            }
            catch (Exception exception)
            {
                var errorMessage =
                    string.Format("Failed to retrieve any UserInfo data from the LinkedIn Api. Error Messages: {0}",
                                  exception.RecursiveErrorMessages());
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage, exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                var errorMessage = string.Format(
                    "Failed to obtain some UserInfo data from the LinkedIn Api OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}. Error Message: {2}.",
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

            // Lets check to make sure we have some bare minimum data.
            if (string.IsNullOrEmpty(response.Data.Id))
            {
                const string errorMessage =
                    "We were unable to retrieve the User Id from LinkedIn API, the user may have denied the authorization.";
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return new UserInformation
                   {
                       Id = response.Data.Id,
                       Name = string.Format("{0}{1}",
                                            string.IsNullOrEmpty(response.Data.FirstName)
                                                ? string.Empty
                                                : response.Data.FirstName + " ",
                                            string.IsNullOrEmpty(response.Data.LastName)
                                                ? string.Empty
                                                : response.Data.LastName).Trim(),
                       Email = response.Data.EmailAddress
                   };
        }

        #endregion
    }
}