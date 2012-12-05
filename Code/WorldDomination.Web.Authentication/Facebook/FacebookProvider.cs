﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using CuttingEdge.Conditions;
using RestSharp;
using RestSharp.Contrib;
using WorldDomination.Web.Authentication.Config;

namespace WorldDomination.Web.Authentication.Facebook
{
    // REFERENCE: http://developers.facebook.com/docs/authentication/server-side/

    public class FacebookProvider : IAuthenticationProvider
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly Uri _redirectUri;
        private readonly IRestClient _restClient;
        private readonly IList<string> _scope;

        public FacebookProvider(ProviderKey providerKey, Uri redirectUri)
        {
            _clientId = providerKey.Key;
            _clientSecret = providerKey.Secret;
            _redirectUri = redirectUri;
        }

        public FacebookProvider(string clientId, string clientSecret, Uri redirectUri)
            : this(clientId, clientSecret, redirectUri, null, null)
        {
        }

        public FacebookProvider(string clientId, string clientSecret, Uri redirectUri, IRestClient restClient)
            : this(clientId, clientSecret, redirectUri, null, restClient)
        {
        }

        public FacebookProvider(string clientId, string clientSecret, Uri redirectUri, IList<string> scope)
            : this(clientId, clientSecret, redirectUri, scope, null)
        {
        }

        public FacebookProvider(string clientId, string clientSecret, Uri redirectUri, IList<string> scope,
                                IRestClient restClient)
        {
            Condition.Requires(clientId).IsNotNullOrEmpty();
            Condition.Requires(clientSecret).IsNotNullOrEmpty();
            Condition.Requires(redirectUri).IsNotNull();

            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUri = redirectUri;

            // Optionals.
            _scope = scope ?? new List<string> {"email"};
            _restClient = restClient ?? new RestClient("https://graph.facebook.com");
        }

        private static string RetrieveAuthorizationCode(NameValueCollection parameters, string existingState = null)
        {
            Condition.Requires(parameters).IsNotNull().IsLongerThan(0);

            // Is this a facebook callback?
            var code = parameters["code"];
            var state = parameters["state"];

            // CSRF (state) check.
            if (!string.IsNullOrEmpty(state) &&
                state != existingState)
            {
                throw new AuthenticationException(
                    "The states do not match. It's possible that you may be a victim of a CSRF.");
            }

            // Maybe we have an error?
            var errorReason = parameters["error_reason"];
            var error = parameters["error"];
            var errorDescription = parameters["error_description"];
            if (!string.IsNullOrEmpty(errorReason) &&
                !string.IsNullOrEmpty(error) &&
                !string.IsNullOrEmpty(errorDescription))
            {
                throw new AuthenticationException(string.Format("Reason: {0}. Error: {1}. Description: {2}.",
                                                                errorReason,
                                                                error,
                                                                errorDescription));
            }

            if (string.IsNullOrEmpty(code))
            {
                throw new AuthenticationException("No code parameter provided in the response query string from Facebook.");
            }

            return code;
        }

        private string RetrieveAccessToken(string code)
        {
            Condition.Requires(code).IsNotNullOrEmpty();

            IRestResponse response;
            try
            {
                var restRequest = new RestRequest("oauth/access_token");
                restRequest.AddParameter("client_id", _clientId);
                restRequest.AddParameter("client_secret", _clientSecret);
                restRequest.AddParameter("code", code);
                restRequest.AddParameter("redirect_uri", _redirectUri.AbsoluteUri);

                response = _restClient.Execute(restRequest);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Failed to retrieve an oauth access token from Facebook.", exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                // {"error":{"message":"Error validating verification code. Please make sure your redirect_uri is identical to the one you used in the OAuth dialog request","type":"OAuthException","code":100}}

                throw new AuthenticationException(
                    string.Format(
                        "Failed to obtain an Access Token from Facebook OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}. Error Content: {2}",
                        response == null ? "-- null response --" : response.StatusCode.ToString(),
                        response == null ? string.Empty : response.StatusDescription,
                        response == null ? string.Empty : response.Content));
            }

            var querystringParameters = HttpUtility.ParseQueryString(response.Content);
            var accessToken = querystringParameters["access_token"];
            int expires;
            var expiresOn = int.TryParse(querystringParameters["expires"], out expires)
                                ? DateTime.UtcNow.AddSeconds(expires)
                                : DateTime.MinValue;

            if (string.IsNullOrEmpty(accessToken) ||
                expiresOn <= DateTime.UtcNow)
            {
                throw new AuthenticationException(
                    "Retrieved a Facebook Access Token but it doesn't contain both the access_token and expires_on parameters.");
            }

            return accessToken;
        }

        private UserInformation RetrieveMe(string accessToken)
        {
            Condition.Requires(accessToken).IsNotNull();

            IRestResponse<MeResult> response;

            try
            {
                var restRequest = new RestRequest("me");
                restRequest.AddParameter("access_token", accessToken);

                response = _restClient.Execute<MeResult>(restRequest);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Failed to retrieve any Me data from the Facebook Api.", exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                throw new AuthenticationException(
                    string.Format(
                        "Failed to obtain some Me data from the Facebook api OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}",
                        response == null ? "-- null response --" : response.StatusCode.ToString(),
                        response == null ? string.Empty : response.StatusDescription));
            }

            return new UserInformation
                   {
                       Id = response.Data.Id.ToString(),
                       Name = (response.Data.FirstName + " " + response.Data.LastName).Trim(),
                       Email = response.Data.Email,
                       Locale = response.Data.Locale,
                       UserName = response.Data.Username,
                       Picture = string.Format("https://graph.facebook.com/{0}/picture", response.Data.Username)
                   };
        }

        #region Implementation of IAuthenticationProvider

        public string Name
        {
            get { return "Facebook"; }
        }

        public Uri RedirectToAuthenticate(IAuthenticationServiceSettings authenticationServiceSettings)
        {
            Condition.Requires(authenticationServiceSettings).IsNotNull();
            
            var facebookAuthenticationSettings = authenticationServiceSettings as FacebookAuthenticationServiceSettings;
            Condition.Requires(facebookAuthenticationSettings).IsNotNull();

            var baseUri = facebookAuthenticationSettings.IsMobile ? "https://m.facebook.com" : "https://www.facebook.com";
            var scope = (_scope != null && _scope.Count > 0)
                            ? "&scope=" + string.Join(",", _scope)
                            : string.Empty;
            var state = !string.IsNullOrEmpty(facebookAuthenticationSettings.State)
                            ? "&state=" + facebookAuthenticationSettings.State
                            : string.Empty;
            var display = facebookAuthenticationSettings.Display == DisplayType.Unknown
                              ? string.Empty
                              : "&display=" + facebookAuthenticationSettings.Display.ToString().ToLowerInvariant();

            // REFERENCE: https://developers.facebook.com/docs/reference/dialogs/oauth/
            // NOTE: Facebook is case-sensitive anal retentive with regards to their uri + querystring params.
            //       So ... we'll lowercase the entire biatch. Thanks, Facebook :(
            var oauthDialogUri = string.Format("{0}/dialog/oauth?client_id={1}{2}{3}{4}&redirect_uri={5}",
                                               baseUri, _clientId, state, scope, display, _redirectUri.AbsoluteUri);

            return new Uri(oauthDialogUri);
        }

        public IAuthenticatedClient AuthenticateClient(NameValueCollection parameters, string existingState)
        {
            var authorizationCode = RetrieveAuthorizationCode(parameters, existingState);

            var accessToken = RetrieveAccessToken(authorizationCode);

            var userInformation = RetrieveMe(accessToken);

            return new AuthenticatedClient(ProviderType.Facebook)
                   {
                       AccessToken = accessToken,
                       AccessTokenExpiresOn = DateTime.UtcNow,
                       UserInformation = userInformation
                   };
        }

        public IAuthenticationServiceSettings DefaultAuthenticationServiceSettings
        {
            get
            {
                return new FacebookAuthenticationServiceSettings
                       {
                           Display = DisplayType.Unknown,
                           IsMobile = false
                       };
            }
        }

        #endregion
    }
}