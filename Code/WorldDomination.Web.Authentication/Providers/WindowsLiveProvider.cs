using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using RestSharp;
using WorldDomination.Web.Authentication.Providers.Google;
using WorldDomination.Web.Authentication.Providers.WindowsLive;
using WorldDomination.Web.Authentication.Tracing;
using AccessTokenResult = WorldDomination.Web.Authentication.Providers.WindowsLive.AccessTokenResult;
using UserInfoResult = WorldDomination.Web.Authentication.Providers.WindowsLive.UserInfoResult;

namespace WorldDomination.Web.Authentication.Providers
{
    public class WindowsLiveProvider : BaseOAuth20Provider<AccessTokenResult>
    {
        // *********************************************************************
        // REFERENCE: http://msdn.microsoft.com/en-us/library/live/hh243647.aspx
        // *********************************************************************

        private const string RedirectUrl =
            "https://login.live.com/oauth20_authorize.srf?client_id={0}{2}&response_type=code&redirect_uri={1}";

        private const string AccessTokenKey = "access_token";
        
        protected override string ScopeKey { get { return "&scope="; }}
        protected override string DefaultScope { get { return "wl.signin wl.basic wl.emails"; } }
        protected override string ScopeSeparator { get { return " "; }}

        public WindowsLiveProvider(ProviderParams providerParams) : base(providerParams)
        {
        }

        #region Implementation of IAuthenticationProvider

        public override  string Name
        {
            get { return "WindowsLive"; }
        }

        public override IAuthenticationServiceSettings DefaultAuthenticationServiceSettings
        {
            get { return new WindowsLiveAuthenticationServiceSettings(); }
        }

        protected override TraceSource TraceSource
        {
            get { return TraceManager["WD.Web.Authentication.Providers." + Name]; }
        }

        public override Uri RedirectToAuthenticate(IAuthenticationServiceSettings authenticationServiceSettings)
        {
            var oauthDialogUri = string.Format(RedirectUrl, ClientKey,
                                               authenticationServiceSettings.CallBackUri.AbsoluteUri, GetScope());

            oauthDialogUri += string.IsNullOrEmpty(authenticationServiceSettings.State)
                                  ? string.Empty
                                  : "&state=" + authenticationServiceSettings.State;

            return new Uri(oauthDialogUri);
        }

        #endregion

        #region BaseOAuth20Provider Members

        protected override string RetrieveAuthorizationCode(NameValueCollection queryStringParameters,
                                                            string existingState = null)
        {
            if (queryStringParameters == null)
            {
                throw new ArgumentNullException("queryStringParameters");
            }

            if (queryStringParameters.Count <= 0)
            {
                throw new ArgumentOutOfRangeException("queryStringParameters");
            }

            var code = queryStringParameters["code"];
            var error = queryStringParameters["error"];

            // First check for any errors.
            if (!string.IsNullOrEmpty(error))
            {
                var errorMessage = "Failed to retrieve an authorization code from Microsoft Live. The error provided is: " + error;
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            // Otherwise, we need a code.
            if (string.IsNullOrEmpty(code))
            {
                const string errorMessage = "No code parameter provided in the response query string from Microsoft Live.";
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return code;
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

            var restRequest = new RestRequest("/oauth20_token.srf");
            restRequest.AddParameter("client_id", ClientKey);
            restRequest.AddParameter("redirect_uri", redirectUri);
            restRequest.AddParameter("client_secret", ClientSecret);
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
                const string errorMessage = "We were unable to retrieve the User Id from Windows Live Api, the user may have denied the authorization.";
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
    }
}