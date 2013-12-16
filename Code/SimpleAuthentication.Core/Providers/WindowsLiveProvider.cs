using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers.WindowsLive;
using SimpleAuthentication.Core.Tracing;

namespace SimpleAuthentication.Core.Providers
{
    public class WindowsLiveProvider : BaseOAuth20Provider<AccessTokenResult>
    {
        // *********************************************************************
        // REFERENCE: http://msdn.microsoft.com/en-us/library/live/hh243647.aspx
        // *********************************************************************

        private const string AccessTokenKey = "access_token";

        public WindowsLiveProvider(ProviderParams providerParams) : this("WindowsLive", providerParams)
        {
        }

        protected WindowsLiveProvider(string name, ProviderParams providerParams) : base(name, providerParams)
        {
            AuthenticateRedirectionUrl = new Uri("https://login.live.com/oauth20_authorize.srf");
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

            var restRequest = new RestRequest("/oauth20_token.srf");
            restRequest.AddParameter("client_id", PublicApiKey);
            restRequest.AddParameter("redirect_uri", redirectUri);
            restRequest.AddParameter("client_secret", SecretApiKey);
            restRequest.AddParameter("code", authorizationCode);
            restRequest.AddParameter("grant_type", "authorization_code");

            var restClient = RestClientFactory.CreateRestClient("https://login.live.com/oauth20_token.srf");
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
                        "Retrieved a Windows Live Access Token but it there's an error with either the access_token parameters. Access Token: {0}.",
                        string.IsNullOrEmpty(accessTokenResult.AccessToken)
                            ? "-no access token-"
                            : accessTokenResult.AccessToken);

                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return new AccessToken
                   {
                       PublicToken = accessTokenResult.AccessToken,
                       // TODO: Wire up the ExpiresIn .. but right now it's a string.. what should it -really- be?
                       //ExpiresOn = DateTime.UtcNow.AddSeconds(response.Data.ExpiresIn)
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
                var restRequest = new RestRequest("/v5.0/me");
                restRequest.AddParameter(AccessTokenKey, accessToken.PublicToken);

                var restClient = RestClientFactory.CreateRestClient("https://apis.live.net");
                TraceSource.TraceVerbose("Retrieving user information. Microsoft Live Endpoint: {0}",
                                         restClient.BuildUri(restRequest).AbsoluteUri);

                response = restClient.Execute<UserInfoResult>(restRequest);
            }
            catch (Exception exception)
            {
                var errorMessage =
                    string.Format("Failed to retrieve any Me data from the Microsoft Live api. Error Messages: {0}",
                                  exception.RecursiveErrorMessages());
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage, exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                var errorMessage = string.Format(
                    "Failed to obtain some 'Me' data from the Microsoft Live api OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}. Error Message: {2}.",
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
            if (string.IsNullOrEmpty(response.Data.id))
            {
                const string errorMessage =
                    "We were unable to retrieve the User Id from Windows Live Api, the user may have denied the authorization.";
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return new UserInformation
                   {
                       Name = string.Format("{0} {1}",
                                            string.IsNullOrEmpty(response.Data.first_name)
                                                ? string.Empty
                                                : response.Data.first_name,
                                            string.IsNullOrEmpty(response.Data.last_name)
                                                ? string.Empty
                                                : response.Data.last_name).Trim(),
                       Locale = response.Data.locale,
                       UserName = response.Data.name,
                       Id = response.Data.id,
                       Email = response.Data.emails.Preferred,
                       Gender = (GenderType) Enum.Parse(typeof (GenderType), response.Data.gender ?? "Unknown", true)
                   };
        }

        #endregion

        public override IEnumerable<string> DefaultScopes
        {
            get { return new[] {"wl.signin", "wl.basic", "wl.emails"}; }
        }
    }
}