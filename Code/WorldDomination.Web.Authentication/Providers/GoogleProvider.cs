﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using RestSharp;
using WorldDomination.Web.Authentication.Providers.Google;

namespace WorldDomination.Web.Authentication.Providers
{
    // REFERENCE: https://developers.google.com/accounts/docs/OAuth2Login

    public class GoogleProvider : BaseProvider, IAuthenticationProvider
    {
        private const string ScopeKey = "&scope={0}";
        private const string AccessTokenKey = "access_token";
        private const string ExpiresInKey = "expires_in";
        private const string TokenTypeKey = "token_type";

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly IList<string> _scope;

        public GoogleProvider(ProviderParams providerParams)
        {
            providerParams.Validate();

            _clientId = providerParams.Key;
            _clientSecret = providerParams.Secret;

            // Optionals.
            _scope = new List<string>
            {
                "https://www.googleapis.com/auth/userinfo.profile",
                "https://www.googleapis.com/auth/userinfo.email"
            };
        }

        private static string RetrieveAuthorizationCode(NameValueCollection queryStringParameters, string existingState = null)
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

        private AccessTokenResult RetrieveAccessToken(string authorizationCode, Uri redirectUri)
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

            IRestResponse<AccessTokenResult> response;

            try
            {
                var request = new RestRequest("/o/oauth2/token", Method.POST);
                request.AddParameter("client_id", _clientId);
                request.AddParameter("client_secret", _clientSecret);
                request.AddParameter("redirect_uri", redirectUri.AbsoluteUri);
                request.AddParameter("code", authorizationCode);
                request.AddParameter("grant_type", "authorization_code");
                var restClient = RestClientFactory.CreateRestClient("https://accounts.google.com");
                response = restClient.Execute<AccessTokenResult>(request);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Failed to obtain an Access Token from Google.", exception);
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
            if (string.IsNullOrEmpty(response.Data.AccessToken) ||
                response.Data.ExpiresIn <= 0 ||
                string.IsNullOrEmpty(response.Data.TokenType))
            {
                throw new AuthenticationException(
                    "Retrieved a Google Access Token but it doesn't contain one or more of either: " + AccessTokenKey +
                    ", " + ExpiresInKey + " or " + TokenTypeKey);
            }

            return response.Data;
        }

        private UserInfoResult RetrieveUserInfo(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentNullException("accessToken");
            }

            IRestResponse<UserInfoResult> response;

            try
            {
                var request = new RestRequest("/oauth2/v2/userinfo", Method.GET);
                request.AddParameter(AccessTokenKey, accessToken);

                var restClient = RestClientFactory.CreateRestClient("https://www.googleapis.com");
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
                        "Failed to obtain User Info from Google OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}",
                        response == null ? "-- null response --" : response.StatusCode.ToString(),
                        response == null ? string.Empty : response.StatusDescription));
            }

            // Lets check to make sure we have some bare minimum data.
            if (string.IsNullOrEmpty(response.Data.Id))
            {
                throw new AuthenticationException(
                    "We were unable to retrieve the User Id from Google API, the user may have denied the authorization.");
            }

            return response.Data;
        }

        #region Implementation of IAuthenticationProvider

        public string Name
        {
            get { return "Google"; }
        }

        public Uri RedirectToAuthenticate(IAuthenticationServiceSettings authenticationServiceSettings)
        {
            if (authenticationServiceSettings == null)
            {
                throw new ArgumentNullException("authenticationServiceSettings");
            }

            if (authenticationServiceSettings.CallBackUri == null)
            {
                throw new ArgumentException("authenticationServiceSettings.CallBackUri");
            }

            // Do we have any scope options?
            // NOTE: Google uses a space-delimeted string for their scope key.
            var scope = (_scope != null && _scope.Count > 0)
                            ? string.Format(ScopeKey, string.Join(" ", _scope))
                            : string.Empty;

            var state = string.IsNullOrEmpty(authenticationServiceSettings.State)
                            ? string.Empty
                            : "&state=" + authenticationServiceSettings.State;

            var oauthDialogUri =
                string.Format(
                    "https://accounts.google.com/o/oauth2/auth?client_id={0}&redirect_uri={1}&response_type=code{2}{3}",
                    _clientId, authenticationServiceSettings.CallBackUri.AbsoluteUri, state, scope);

            return new Uri(oauthDialogUri);
        }

        public IAuthenticatedClient AuthenticateClient(IAuthenticationServiceSettings authenticationServiceSettings,
                                                       NameValueCollection queryStringParameters)
        {
            if (authenticationServiceSettings == null)
            {
                throw new ArgumentNullException("authenticationServiceSettings");
            }

            // First up - an authorization token.
            var authorizationCode = RetrieveAuthorizationCode(queryStringParameters, authenticationServiceSettings.State);

            // Get an Access Token.
            var oAuthAccessToken = RetrieveAccessToken(authorizationCode, authenticationServiceSettings.CallBackUri);

            // Grab the user information.
            var userInfo = RetrieveUserInfo(oAuthAccessToken.AccessToken);

            
            return new AuthenticatedClient(Name.ToLowerInvariant())
            {
                AccessToken = oAuthAccessToken.AccessToken,
                AccessTokenExpiresOn = DateTime.UtcNow.AddSeconds(oAuthAccessToken.ExpiresIn),
                UserInformation = new UserInformation
                {
                    Id = userInfo.Id,
                    Gender = string.IsNullOrEmpty(userInfo.Gender) 
                        ? GenderType.Unknown 
                        : GenderTypeHelpers.ToGenderType(userInfo.Gender),
                    Name = userInfo.Name,
                    Email = userInfo.Email,
                    Locale = userInfo.Locale,
                    Picture = userInfo.Picture,
                    UserName = userInfo.GivenName
                }
            };
        }

        public IAuthenticationServiceSettings DefaultAuthenticationServiceSettings
        {
            get { return new GoogleAuthenticationServiceSettings(); }
        }

        #endregion
    }
}