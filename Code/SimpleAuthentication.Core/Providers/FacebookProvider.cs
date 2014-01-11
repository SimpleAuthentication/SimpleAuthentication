using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using RestSharp;
using RestSharp.Contrib;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers.Facebook;
using SimpleAuthentication.Core.Tracing;

namespace SimpleAuthentication.Core.Providers
{
    // REFERENCE: https://developers.facebook.com/docs/facebook-login/login-flow-for-web-no-jssdk/

    public class FacebookProvider : BaseOAuth20Provider<AccessTokenResult>
    {
        private const string BaseUrl = "https://graph.facebook.com";
        private bool _isMobile;

        public FacebookProvider(ProviderParams providerParams) : this("Facebook", providerParams)
        {
        }

        protected FacebookProvider(string name, ProviderParams providerParams) : base(name, providerParams)
        {
            DisplayType = DisplayType.Unknown;
            IsMobile = false;
        }

        #region BaseOAuth20Token<AccessTokenResult> Implementation

        protected override string CreateRedirectionQuerystringParameters(Uri callbackUri, string state)
        {
            if (callbackUri == null)
            {
                throw new ArgumentNullException("callbackUri");
            }

            if (string.IsNullOrEmpty(state))
            {
                throw new ArgumentNullException("state");
            }

            var display = DisplayType == DisplayType.Unknown
                              ? string.Empty
                              : "&display=" + DisplayType.ToString().ToLowerInvariant();

            // REFERENCE: https://developers.facebook.com/docs/reference/dialogs/oauth/
            // NOTE: Facebook is case-sensitive anal retentive with regards to their uri + querystring params.
            //       So ... we'll lowercase the entire biatch. Thanks, Facebook :(
            return string.Format("client_id={0}&redirect_uri={1}{2}{3}{4}",
                                 PublicApiKey, callbackUri.AbsoluteUri, GetScope(), GetQuerystringState(state), display)
                         .ToLowerInvariant();
        }

        protected override string RetrieveAuthorizationCode(NameValueCollection queryStringParameters)
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

            var restRequest = new RestRequest("oauth/access_token");
            restRequest.AddParameter("client_id", PublicApiKey);
            restRequest.AddParameter("client_secret", SecretApiKey);
            restRequest.AddParameter("code", authorizationCode);
            restRequest.AddParameter("redirect_uri", redirectUri.AbsoluteUri.ToLowerInvariant());
            restRequest.AddHeader("Content-Type", "application/json");
            restRequest.AddParameter("format", "json");

            var restClient = RestClientFactory.CreateRestClient(BaseUrl);
            TraceSource.TraceVerbose("Retrieving Access Token endpoint: {0}",
                                     restClient.BuildUri(restRequest).AbsoluteUri);

            // Really really sad hack. Facebook send back all their data as Json except
            // this f'ing endpoint. As such, we'll fuck with things here.
            // We'll manually create the data - if possible.
            // How - we will try and recreate the content result.
            restRequest.OnBeforeDeserialization = response =>
            {
                // Grab the content and convert it into json.
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    // Something is wrong - so just leave. This is handled elsewhere.
                    return;
                }

                // Lets do this!
                var querystringData = HttpUtility.ParseQueryString(response.Content);
                var json = new StringBuilder("{"); // Start.
                
                foreach (var key in querystringData.AllKeys)
                {
                    json.AppendFormat("\"{0}\":\"{1}\"", key, querystringData[key]);
                }

                json.Append("}"); // End.

                response.Content = json.ToString();
                response.ContentType = "text/json";
            };

            return restClient.Execute<AccessTokenResult>(restRequest);
        }

        protected override AccessToken MapAccessTokenResultToAccessToken(AccessTokenResult accessTokenResult)
        {
            if (accessTokenResult == null)
            {
                throw new ArgumentNullException("accessTokenResult");
            }

            if (string.IsNullOrEmpty(accessTokenResult.access_token) ||
                accessTokenResult.expires <= 0)
            {
                var errorMessage =
                    string.Format(
                        "Retrieved a Facebook Access Token but there's an error with either the access_token and/or expires_on parameters. Access Token: {0}. Expires In: {1}.",
                        string.IsNullOrEmpty(accessTokenResult.access_token)
                            ? "-no access token-"
                            : accessTokenResult.access_token,
                        accessTokenResult.expires.ToString());

                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return new AccessToken
                   {
                       PublicToken = accessTokenResult.access_token,
                       ExpiresOn = DateTime.UtcNow.AddSeconds(accessTokenResult.expires)
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
                var authenticationException =
                    new AuthenticationException("Failed to retrieve any Me data from the Facebook Api.", exception);
                var errorMessage = authenticationException.RecursiveErrorMessages();
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

        /// <summary>
        /// Are we on a mobile device?
        /// </summary>
        /// <remarks>This will also have the side-effect of auto setting the AuthenticateRedirectionUrl property, based on this set value.</remarks>
        public bool IsMobile
        {
            get { return _isMobile; }
            set
            {
                _isMobile = value;

                // Now auto set the redirection url. 
                AuthenticateRedirectionUrl = IsMobile
                                                 ? new Uri("https://m.facebook.com/dialog/oauth")
                                                 : new Uri("https://www.facebook.com/dialog/oauth");
            }
        }

        public DisplayType DisplayType { get; set; }

        public override IEnumerable<string> DefaultScopes
        {
            get { return new[] {"email"}; }
        }

        public override string ScopeSeparator
        {
            get { return ","; }
        }
    }
}