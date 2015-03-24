using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers;
using SimpleAuthentication.Core.Tracing;
using SimpleAuthentication.ExtraProviders.ArcGISOnline;
using RestSharp.Deserializers;

namespace SimpleAuthentication.ExtraProviders
{
    public class ArcGISOnlineProvider : BaseOAuth20Provider<AccessTokenResult>
    {
        private const string AccessTokenKey = "token";

        public ArcGISOnlineProvider(ProviderParams providerParams)
            : this("ArcGISOnline", providerParams)
        {
        }

        protected ArcGISOnlineProvider(string name, ProviderParams providerParams)
            : base(name, providerParams)
        {

            AuthenticateRedirectionUrl = new Uri("https://www.arcgis.com/sharing/rest/oauth2/authorize");
        }

        #region BaseOAuth20Token<AccessTokenResult> Implementation

        public override IEnumerable<string> DefaultScopes
        {
            get { return new[] { "" }; }
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


            var restRequest = new RestRequest("/sharing/rest/oauth2/token", Method.POST);
            restRequest.AddParameter("client_id", PublicApiKey);
            restRequest.AddParameter("code", authorizationCode);
            restRequest.AddParameter("grant_type", "authorization_code");
            restRequest.AddParameter("redirect_uri", redirectUri);
            restRequest.AddHeader("Accept", "application/json; charset=utf-8");

            // Due to ArcGISOnline always responding back with Content-Type 'text/plain' we cannot use RestClientFactory 
            // because its interface does not expose AddHandler method. https://github.com/restsharp/RestSharp/issues/258
            var restClient = new RestClient("https://www.arcgis.com");
            TraceSource.TraceVerbose("Retrieving Access Token endpoint: {0}", restClient.BuildUri(restRequest).AbsoluteUri);

            restClient.AddHandler("text/plain", new JsonDeserializer());
            return restClient.Execute<AccessTokenResult>(restRequest);
        }


        protected override AccessToken MapAccessTokenResultToAccessToken(AccessTokenResult accessTokenResult)
        {
            if (accessTokenResult == null)
            {
                throw new ArgumentNullException("accessTokenResult");
            }

            if (string.IsNullOrEmpty(accessTokenResult.AccessToken) ||
                accessTokenResult.ExpiresIn <= 0 ||
                string.IsNullOrEmpty(accessTokenResult.RefreshToken))
            {
                var errorMessage =
                    string.Format(
                        "Retrieved a ArcGIS Online Access Token but it doesn't contain: {0}.",
                        AccessTokenKey);
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
                var restRequest = new RestRequest("/sharing/rest/community/self", Method.GET);
                restRequest.AddParameter("token", accessToken.PublicToken);
                restRequest.AddParameter("f", "json");

                // Due to ArcGISOnline always responding back with Content-Type 'text/plain' we cannot use RestClientFactory 
                // because its interface does not expose AddHandler method. https://github.com/restsharp/RestSharp/issues/258
                var restClient = new RestClient("https://www.arcgis.com");
                restClient.AddHandler("text/plain", new JsonDeserializer());

                TraceSource.TraceVerbose("Retrieving user information. ArcGIS Online Endpoint: {0}",
                                         restClient.BuildUri(restRequest).AbsoluteUri);

                response = restClient.Execute<UserInfoResult>(restRequest);
            }
            catch (Exception exception)
            {
                var errorMessage =
                    string.Format("Failed to retrieve any UserInfo data from the ArcGIS Online Api. Error Messages: {0}",
                                  exception.RecursiveErrorMessages());
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage, exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                var errorMessage = string.Format(
                    "Failed to obtain some UserInfo data from the ArcGIS Online Api OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}. Error Message: {2}.",
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
            if (string.IsNullOrEmpty(response.Data.OrgId))
            {
                const string errorMessage =
                    "We were unable to retrieve the OrgId from ArcGIS Online, the user may have denied the authorization.";
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            // ArcGISOnline doesnt have id (TTBOMK) so we're using user's username and orgId to create one

            return new UserInformation
            {
                Id = response.Data.Username + "-" + response.Data.OrgId,
                Gender = GenderType.Unknown,
                Name = response.Data.FullName,
                Email = response.Data.Email,
                Locale = "",
                Picture = "",
                UserName = response.Data.Username
            };
        }

        #endregion
    }
}
