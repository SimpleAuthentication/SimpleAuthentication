using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using RestSharp;
using WorldDomination.Web.Authentication.Providers.Google;
using WorldDomination.Web.Authentication.Providers.WindowsLive;
using UserInfoResult = WorldDomination.Web.Authentication.Providers.WindowsLive.UserInfoResult;

namespace WorldDomination.Web.Authentication.Providers
{
    public class WindowsLiveProvider : BaseOAuth20Provider
    {
        // *********************************************************************
        // REFERENCE: http://msdn.microsoft.com/en-us/library/live/hh243647.aspx
        // *********************************************************************

        private const string RedirectUrl =
            "https://login.live.com/oauth20_authorize.srf?client_id={0}&scope={2}&response_type=code&redirect_uri={1}";

        private const string AccessTokenKey = "access_token";

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _scope = string.Join(" ", new[] {"wl.signin", "wl.basic", "wl.emails"});

        public WindowsLiveProvider(ProviderParams providerParams)
        {
            providerParams.Validate();

            _clientId = providerParams.Key;
            _clientSecret = providerParams.Secret;
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
            var oauthDialogUri = string.Format(RedirectUrl, _clientId,
                                               authenticationServiceSettings.CallBackUri.AbsoluteUri, _scope);

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

            /* Documentation:
               Google returns an authorization code to your application if the user grants your application the permissions it requested. 
               The authorization code is returned to your application in the query string parameter code. If the state parameter was included in the request,
               then it is also included in the response. */
            var code = queryStringParameters["code"];
            var error = queryStringParameters["error"];

            // First check for any errors.
            if (!string.IsNullOrEmpty(error))
            {
                throw new AuthenticationException(
                    "Failed to retrieve an authorization code from Google. The error provided is: " + error);
            }

            // Otherwise, we need a code.
            if (string.IsNullOrEmpty(code))
            {
                throw new AuthenticationException("No code parameter provided in the response query string from Google.");
            }

            return code;
        }

        protected override AccessToken RetrieveAccessToken(string authorizationCode, Uri redirectUri)
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

            IRestResponse<AuthenticatedTokenResult> response;

            try
            {
                var request = new RestRequest("/oauth20_token.srf");
                request.AddParameter("client_id", _clientId);
                request.AddParameter("redirect_uri", redirectUri);
                request.AddParameter("client_secret", _clientSecret);
                request.AddParameter("code", authorizationCode);
                request.AddParameter("grant_type", "authorization_code");
                var restClient = RestClientFactory.CreateRestClient("https://login.live.com/oauth20_token.srf");
                response = restClient.Execute<AuthenticatedTokenResult>(request);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Failed to obtain an Access Token from Windows Live.", exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                throw new AuthenticationException(
                    string.Format(
                        "Failed to obtain an Access Token from Google OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}",
                        response == null ? "-- null response --" : response.StatusCode.ToString(),
                        response == null ? string.Empty : response.StatusDescription));
            }

            // Grab the params which should have the request token info.
            //if (string.IsNullOrEmpty(response.Data.AccessToken) ||
            //    response.Data.ExpiresIn <= 0 ||
            //    string.IsNullOrEmpty(response.Data.TokenType))
            //{
            //    throw new AuthenticationException(
            //        "Retrieved a Google Access Token but it doesn't contain one or more of either: " + AccessTokenKey +
            //        ", " + ExpiresInKey + " or " + TokenTypeKey);
            //}

            return new AccessToken
                   {
                       PublicToken = response.Data.AccessToken,
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
                var request = new RestRequest("/v5.0/me");
                request.AddParameter(AccessTokenKey, accessToken.PublicToken);

                var restClient = RestClientFactory.CreateRestClient("https://apis.live.net");
                response = restClient.Execute<UserInfoResult>(request);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Failed to obtain User Info from Google.", exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                throw new AuthenticationException(
                    string.Format(
                        "Failed to obtain User Info from Windows Live OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}",
                        response == null ? "-- null response --" : response.StatusCode.ToString(),
                        response == null ? string.Empty : response.StatusDescription));
            }

            // Lets check to make sure we have some bare minimum data.
            if (string.IsNullOrEmpty(response.Data.id))
            {
                throw new AuthenticationException(
                    "We were unable to retrieve the User Id from Windows Live Api, the user may have denied the authorization.");
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