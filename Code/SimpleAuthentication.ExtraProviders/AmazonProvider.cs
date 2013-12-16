using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers;
using SimpleAuthentication.Core.Tracing;
using SimpleAuthentication.ExtraProviders.Amazon;

namespace SimpleAuthentication.ExtraProviders
{
    //https://images-na.ssl-images-amazon.com/images/G/01/lwa/dev/docs/website-developer-guide._TTH_.pdf
    //http://login.amazon.com

    public class AmazonProvider : BaseOAuth20Provider<AccessTokenResult>
    {
        private const string AccessTokenKey = "access_token";

        public AmazonProvider(ProviderParams providerParams) : this("Amazon", providerParams)
        {
        }

        protected AmazonProvider(string name, ProviderParams providerParams) : base(name, providerParams)
        {
            AuthenticateRedirectionUrl = new Uri("https://www.amazon.com/ap/oa");
        }

        #region BaseOAuth20Token<AccessTokenResult> Implementation

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

            var restRequest = new RestRequest("/auth/o2/token", Method.POST);

            restRequest.AddParameter("client_id", PublicApiKey);
            restRequest.AddParameter("client_secret", SecretApiKey);
            restRequest.AddParameter("code", authorizationCode);
            restRequest.AddParameter("grant_type", "authorization_code");
            restRequest.AddParameter("redirect_uri", redirectUri);

            var restClient = RestClientFactory.CreateRestClient("https://api.amazon.com");
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

            if (string.IsNullOrEmpty(accessTokenResult.AccessToken))
            {
                var errorMessage =
                    string.Format(
                        "Retrieved an Amazon Access Token but it there's an error with either the access_token parameters. Access Token: {0}.",
                        string.IsNullOrEmpty(accessTokenResult.AccessToken)
                            ? "-no access token-"
                            : accessTokenResult.AccessToken);

                TraceSource.TraceError(errorMessage);
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
                var restRequest = new RestRequest("/ap/user/profile", Method.GET);
                restRequest.AddParameter(AccessTokenKey, accessToken);

                var restClient = RestClientFactory.CreateRestClient("https://www.amazon.com");
                TraceSource.TraceVerbose("Retrieving user information. Amazon Endpoint: {0}",
                                         restClient.BuildUri(restRequest).AbsoluteUri);

                response = restClient.Execute<UserInfoResult>(restRequest);
            }
            catch (Exception exception)
            {
                var errorMessage =
                    string.Format("Failed to retrieve any Me data from the Amazon Api. Error Messages: {0}",
                                  exception.RecursiveErrorMessages());
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage, exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                var errorMessage = string.Format(
                    "Failed to obtain some 'Me' data from the Amazon api OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}. Error Message: {2}.",
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
            if (string.IsNullOrEmpty(response.Data.Profile.CustomerId))
            {
                const string errorMessage =
                    "Retrieve some user info from the Amazon Api, but we're missing: CustomerId.";
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return new UserInformation
                   {
                       Id = response.Data.Profile.CustomerId,
                       Name = response.Data.Profile.Name,
                       Email = response.Data.Profile.PrimaryEmail
                   };
        }

        #endregion

        public override IEnumerable<string> DefaultScopes
        {
            get { return new[] {"profile"}; }
        }
    }
}