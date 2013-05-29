using System;
using System.Collections.Specialized;
using System.Net;
using RestSharp;
using WorldDomination.Web.Authentication.ExtraProviders.Amazon;
using WorldDomination.Web.Authentication.Providers;

namespace WorldDomination.Web.Authentication.ExtraProviders
{
    //https://images-na.ssl-images-amazon.com/images/G/01/lwa/dev/docs/website-developer-guide._TTH_.pdf
    //http://login.amazon.com
    public class AmazonProvider : BaseProvider, IAuthenticationProvider
    {
        private const string AccessTokenKey = "access_token";
        private readonly string _clientId;
        private readonly string _clientSecret;

        public AmazonProvider(ProviderParams providerParams)
        {
            providerParams.Validate();

            _clientId = providerParams.Key;
            _clientSecret = providerParams.Secret;
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

            var code = parameters["code"];
            var error = parameters["error"];

            // First check for any errors.
            if (!string.IsNullOrEmpty(error))
            {
                throw new AuthenticationException(
                    "Failed to retrieve an authorization code from Amazon. The error provided is: " + error);
            }

            // Otherwise, we need a code.
            if (string.IsNullOrEmpty(code))
            {
                throw new AuthenticationException("No code parameter provided in the response query string from Amazon.");
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
                var request = new RestRequest("/auth/o2/token", Method.POST);
                
                request.AddParameter("client_id", _clientId);
                request.AddParameter("client_secret", _clientSecret);
                request.AddParameter("code", authorizationCode);
                request.AddParameter("grant_type", "authorization_code");
                request.AddParameter("redirect_uri", redirectUri);
                
                var restClient = RestClientFactory.CreateRestClient("https://api.amazon.com");
                response = restClient.Execute<AccessTokenResult>(request);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Failed to obtain an Access Token from Amazon.", exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                throw new AuthenticationException(
                    string.Format(
                        "Failed to obtain an Access Token from Amazon OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}",
                        response == null ? "-- null response --" : response.StatusCode.ToString(),
                        response == null ? string.Empty : response.StatusDescription));
            }

            if (string.IsNullOrEmpty(response.Data.AccessToken))
            {
                throw new AuthenticationException("AccessToken returned but was null or empty value");
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
                var request = new RestRequest("/ap/user/profile", Method.GET);
                request.AddParameter(AccessTokenKey, accessToken);

                var restClient = RestClientFactory.CreateRestClient("https://www.amazon.com");
                response = restClient.Execute<UserInfoResult>(request);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException("Failed to obtain User Info from Amazon.", exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                throw new AuthenticationException(
                    string.Format(
                        "Failed to obtain User Info from Amazon OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}",
                        response == null ? "-- null response --" : response.StatusCode.ToString(),
                        response == null ? string.Empty : response.StatusDescription));
            }

            // Lets check to make sure we have some bare minimum data.
            if (string.IsNullOrEmpty(response.Data.Profile.CustomerId))
            {
                throw new AuthenticationException(
                    "Retrieve some user info from the Amazon Api, but we're missing: CustomerId.");
            }

            return response.Data;
        }

        #region Implementation of IAuthenticationProvider

        public string Name
        {
            get { return "Amazon"; }
        }

        public IAuthenticationServiceSettings DefaultAuthenticationServiceSettings
        {
            get { return new AmazonAuthenticationServiceSettings(); }
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

            var state = string.IsNullOrEmpty(authenticationServiceSettings.State)
                            ? string.Empty
                            : "&state=" + authenticationServiceSettings.State;

            var uriEncoded = Uri.EscapeUriString(authenticationServiceSettings.CallBackUri.AbsoluteUri);
            
            var oauthDialogUri =
                string.Format(
                    "https://www.amazon.com/ap/oa?client_id={0}&scope=profile&redirect_uri={1}&response_type=code{2}",
                    _clientId, uriEncoded, state);
            
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
                UserInformation = new UserInformation
                {
                    Id = userInfo.Profile.CustomerId,
                    Name = userInfo.Profile.Name,
                    Email = userInfo.Profile.PrimaryEmail
                }
            };
        }

        #endregion
    }
}