using System;
using System.Collections.Specialized;
using System.Net;
using WorldDomination.Web.Authentication.ExtraProviders.GitHub;
using RestSharp;

namespace WorldDomination.Web.Authentication.ExtraProviders
{
    public class GitHubProvider : IAuthenticationProvider
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly IRestClientFactory _restClientFactory;

        private const string AccessTokenKey = "access_token";
        private const string TokenTypeKey = "token_type";

        public GitHubProvider(CustomProviderParams providerParams)
        {
            _clientId = providerParams.Key;
            _clientSecret = providerParams.Secret;
            _restClientFactory = providerParams.RestClientFactory ?? new RestClientFactory();
        }

        public GitHubProvider(string clientId, string clientSecret, IRestClientFactory restClientFactory)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _restClientFactory = restClientFactory ?? new RestClientFactory();
        }

        private static string RetrieveAuthorizationCode(NameValueCollection parameters, string existingState = null)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            if (parameters.Count <= 0)
            {
                throw new ArgumentOutOfRangeException("parameters");
            }

            /* Documentation:
               Github returns an authorization code to your application if the user grants your application the permissions it requested. 
               The authorization code is returned to your application in the query string parameter code. If the state parameter was included in the request,
               then it is also included in the response. */
            var code = parameters["code"];
            var state = parameters["state"];
            var error = parameters["error"];

            // CSRF (state) check.
            // NOTE: There is always a state provided. Even if an error is returned.
            if (!string.IsNullOrEmpty(existingState) &&
                state != existingState)
            {
                throw new AuthenticationException(
                    "The states do not match. It's possible that you may be a victim of a CSRF.");
            }

            // First check for any errors.
            if (!string.IsNullOrEmpty(error))
            {
                throw new AuthenticationException(
                    "Failed to retrieve an authorization code from GitHub. The error provided is: " + error);
            }

            // Otherwise, we need a code.
            if (string.IsNullOrEmpty(code))
            {
                throw new AuthenticationException("No code parameter provided in the response query string from GitHub.");
            }

            return code;
        }

        private AccessTokenResult RetrieveAccessToken(string authorizationCode)
        {
            if (string.IsNullOrEmpty(authorizationCode))
            {
                throw new ArgumentNullException("authorizationCode");
            }

            IRestResponse<AccessTokenResult> response;

            try
            {
                var request = new RestRequest("/login/oauth/access_token", Method.POST);
                request.AddParameter("client_id", _clientId);
                request.AddParameter("client_secret", _clientSecret);
                if (CallBackUri != null)
                {
                    request.AddParameter("redirect_uri", CallBackUri.AbsoluteUri);
                }
                request.AddParameter("code", authorizationCode);
                request.AddParameter("grant_type", "authorization_code");
                var restClient = _restClientFactory.CreateRestClient("https://github.com");
                response = restClient.Execute<AccessTokenResult>(request);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Failed to obtain an Access Token from GitHub.", exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                throw new AuthenticationException(
                    string.Format(
                        "Failed to obtain an Access Token from GitHub OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}",
                        response == null ? "-- null response --" : response.StatusCode.ToString(),
                        response == null ? string.Empty : response.StatusDescription));
            }

            // Grab the params which should have the request token info.
            if (string.IsNullOrEmpty(response.Data.AccessToken) ||
                string.IsNullOrEmpty(response.Data.TokenType))
            {
                throw new AuthenticationException(
                    string.Format(
                        "Retrieved a GitHub Access Token but it doesn't contain one or more of either: {0} or {1}",
                        AccessTokenKey, TokenTypeKey));
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
                var request = new RestRequest("/user", Method.GET);
                request.AddParameter(AccessTokenKey, accessToken);

                var restClient = _restClientFactory.CreateRestClient("https://api.github.com");
                response = restClient.Execute<UserInfoResult>(request);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Failed to obtain User Info from GitHub.", exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                throw new AuthenticationException(
                    string.Format(
                        "Failed to obtain User Info from GitHub OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}",
                        response == null ? "-- null response --" : response.StatusCode.ToString(),
                        response == null ? string.Empty : response.StatusDescription));
            }

            // Lets check to make sure we have some bare minimum data.
            if (string.IsNullOrEmpty(response.Data.Id.ToString()) ||
                string.IsNullOrEmpty(response.Data.Login) ||
                string.IsNullOrEmpty(response.Data.Name))
            {
                throw new AuthenticationException(
                    "Retrieve some user info from the GitHub Api, but we're missing one or more of either: Id, Login, and Name.");
            }

            return response.Data;
        }

        #region Implementation of IAuthenticationProvider

        public string Name { get { return "GitHub"; } }
        public Uri CallBackUri { get; private set; }

        public IAuthenticationServiceSettings DefaultAuthenticationServiceSettings
        {
            get { return new GitHubAuthenticationServiceSettings(); } 
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

            // Remember the callback uri.
            CallBackUri = authenticationServiceSettings.CallBackUri;

            var state = string.IsNullOrEmpty(authenticationServiceSettings.State)
                            ? string.Empty
                            : "&state=" + authenticationServiceSettings.State;

            var oauthDialogUri =
                string.Format(
                    "https://github.com/login/oauth/authorize?client_id={0}&redirect_uri={1}&response_type=code{2}",
                    _clientId, CallBackUri.AbsoluteUri, state);

            return new Uri(oauthDialogUri);
        }

        public IAuthenticatedClient AuthenticateClient(NameValueCollection parameters, string existingState)
        { // First up - an authorization token.
            var authorizationCode = RetrieveAuthorizationCode(parameters, existingState);

            // Get an Access Token.
            var oAuthAccessToken = RetrieveAccessToken(authorizationCode);

            // Grab the user information.
            var userInfo = RetrieveUserInfo(oAuthAccessToken.AccessToken);

            return new AuthenticatedClient(Name.ToLowerInvariant())
            {
                AccessToken = oAuthAccessToken.AccessToken,
                UserInformation = new UserInformation
                {
                    Id = userInfo.Id.ToString(),
                    Name = userInfo.Name,
                    Email = userInfo.Email??"",
                    Picture = userInfo.AvatarUrl,
                    UserName = userInfo.Login,
                }
            };
        }

        #endregion
    }
}