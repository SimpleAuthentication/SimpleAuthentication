using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using RestSharp;
using RestSharp.Contrib;
using WorldDomination.Web.Authentication.Providers.Facebook;
using WorldDomination.Web.Authentication.Tracing;

namespace WorldDomination.Web.Authentication.Providers
{
    // REFERENCE: http://developers.facebook.com/docs/authentication/server-side/

    public class FacebookProvider : BaseOAuth20Provider
    {
        private const string BaseUrl = "https://graph.facebook.com";
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly IList<string> _scope;

        public FacebookProvider(ProviderParams providerParams)
        {
            providerParams.Validate();

            _clientId = providerParams.Key;
            _clientSecret = providerParams.Secret;

            // Optionals.
            _scope = new List<string> {"email"};
        }

        #region Implementation of IAuthenticationProvider

        public override string Name
        {
            get { return "Facebook"; }
        }

        public override IAuthenticationServiceSettings DefaultAuthenticationServiceSettings
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

        public override Uri RedirectToAuthenticate(IAuthenticationServiceSettings authenticationServiceSettings)
        {
            if (authenticationServiceSettings == null)
            {
                throw new ArgumentNullException("authenticationServiceSettings");
            }

            var facebookAuthenticationSettings = authenticationServiceSettings as FacebookAuthenticationServiceSettings;
            if (facebookAuthenticationSettings == null)
            {
                throw new InvalidOperationException(
                    "AuthenticationServiceSettings instance is not of type FacebookAuthenticationServiceSettings.");
            }

            var baseUri = facebookAuthenticationSettings.IsMobile
                              ? "https://m.facebook.com"
                              : "https://www.facebook.com";
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
                                               baseUri, _clientId, state, scope, display,
                                               authenticationServiceSettings.CallBackUri.AbsoluteUri);

            TraceSource.TraceInformation("Facebook redirection uri: {0}", oauthDialogUri);

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

            // Is this a facebook callback?
            var code = queryStringParameters["code"];

            // Maybe we have an error?
            var errorReason = queryStringParameters["error_reason"];
            var error = queryStringParameters["error"];
            var errorDescription = queryStringParameters["error_description"];
            if (!string.IsNullOrEmpty(errorReason) &&
                !string.IsNullOrEmpty(error) &&
                !string.IsNullOrEmpty(errorDescription))
            {
                var errorMessage = string.Format("Reason: {0}. Error: {1}. Description: {2}.",
                                                 string.IsNullOrEmpty(errorReason) ? "-no error reason-" : errorReason,
                                                 string.IsNullOrEmpty(error) ? "-no error-" : error,
                                                 string.IsNullOrEmpty(errorDescription)
                                                     ? "-no error description-"
                                                     : errorDescription);
                TraceSource.TraceVerbose(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            if (string.IsNullOrEmpty(code))
            {
                const string errorMessage = "No code parameter provided in the response query string from Facebook.";
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return code;
        }

        protected override AccessToken RetrieveAccessToken(string code, Uri redirectUri)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }

            if (redirectUri == null ||
                string.IsNullOrEmpty(redirectUri.AbsoluteUri))
            {
                throw new ArgumentNullException("redirectUri");
            }

            IRestResponse response;
            try
            {
                var restRequest = new RestRequest("oauth/access_token");
                restRequest.AddParameter("client_id", _clientId);
                restRequest.AddParameter("client_secret", _clientSecret);
                restRequest.AddParameter("code", code);
                restRequest.AddParameter("redirect_uri", redirectUri.AbsoluteUri);

                var restClient = RestClientFactory.CreateRestClient(BaseUrl);
                TraceSource.TraceVerbose("Retrieving Access Token endpoint: {0}",
                                         restClient.BuildUri(restRequest).AbsoluteUri);

                response = restClient.Execute(restRequest);
            }
            catch (Exception exception)
            {
                var errorMessage =
                    string.Format("Failed to retrieve an oauth access token from Facebook. Error Messages: {0}",
                                  exception.RecursiveErrorMessages());
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage, exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                // {"error":{"message":"Error validating verification code. Please make sure your redirect_uri is identical to the one you used in the OAuth dialog request","type":"OAuthException","code":100}}

                var errorMessage = string.Format(
                    "Failed to obtain an Access Token from Facebook OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}. Error Content: {2}. Error Message: {3}.",
                    response == null ? "-- null response --" : response.StatusCode.ToString(),
                    response == null ? string.Empty : response.StatusDescription,
                    response == null ? string.Empty : response.Content,
                    response == null
                        ? string.Empty
                        : response.ErrorException == null
                              ? "--no error exception--"
                              : response.ErrorException.RecursiveErrorMessages());

                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
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
                var errorMessage =
                    "Retrieved a Facebook Access Token but it doesn't contain both the access_token and expires_on parameters. Response.Content: "
                    + (string.IsNullOrEmpty(response.Content) ? "-no response content-" : response.Content);

                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return new AccessToken
                   {
                       PublicToken = accessToken,
                       ExpiresOn = expiresOn
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

            IRestResponse<MeResult> response;

            try
            {
                var restRequest = new RestRequest("me");
                restRequest.AddParameter("access_token", accessToken.PublicToken);

                var restClient = RestClientFactory.CreateRestClient(BaseUrl);

                TraceSource.TraceVerbose("Retrieving user information. Facebook Endpoint: {0}",
                                         restClient.BuildUri(restRequest).AbsoluteUri);

                response = restClient.Execute<MeResult>(restRequest);
            }
            catch (Exception exception)
            {
                var errorMessage =
                    string.Format("Failed to retrieve any Me data from the Facebook Api. Error Messages: {0}",
                                  exception.RecursiveErrorMessages());
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage, exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK ||
                response.Data == null)
            {
                var errorMessage = string.Format(
                    "Failed to obtain some 'Me' data from the Facebook api OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}. Error Message: {2}.",
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

            var id = response.Data.Id < 0 ? 0 : response.Data.Id;
            var name = (string.IsNullOrEmpty(response.Data.FirstName)
                            ? string.Empty
                            : response.Data.FirstName) + " " +
                       (string.IsNullOrEmpty(response.Data.LastName)
                            ? string.Empty
                            : response.Data.LastName).Trim();

            return new UserInformation
                   {
                       Id = id.ToString(),
                       Name = name,
                       Email = response.Data.Email,
                       Locale = response.Data.Locale,
                       UserName = response.Data.Username,
                       Picture = string.Format("https://graph.facebook.com/{0}/picture", id)
                   };
        }

        #endregion
    }
}