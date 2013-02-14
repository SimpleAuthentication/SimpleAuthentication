using System;
using System.Collections.Specialized;
using RestSharp;
using WorldDomination.Web.Authentication.ExtraProviders.WindowsLive;

namespace WorldDomination.Web.Authentication.ExtraProviders
{
    public class WindowsLiveProvider : IAuthenticationProvider
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly IRestClientFactory _restClientFactory;
        private const string RedirectUrl = "https://login.live.com/oauth20_authorize.srf?client_id={0}&scope={2}&response_type=code&redirect_uri={1}";
        private readonly string _scope = string.Join(" ", new[] { "wl.signin", "wl.basic", "wl.emails" });

        public WindowsLiveProvider(CustomProviderParams providerParams)
        {
            _clientId = providerParams.Key;
            _clientSecret = providerParams.Secret;
            _restClientFactory = providerParams.RestClientFactory ?? new RestClientFactory();
        }

        public WindowsLiveProvider(string clientId, string clientSecret, IRestClientFactory restClientFactory)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _restClientFactory = restClientFactory ?? new RestClientFactory();
        }

        public string Name { get { return "WindowsLive"; } }
        public Uri CallBackUri { get; private set; }
        public IAuthenticationServiceSettings DefaultAuthenticationServiceSettings
        {
            get { return new WindowsLiveAuthenticationServiceSettings(); }
        }

        public Uri RedirectToAuthenticate(IAuthenticationServiceSettings authenticationServiceSettings)
        {
            CallBackUri = authenticationServiceSettings.CallBackUri;

            var oauthDialogUri = string.Format(RedirectUrl, _clientId, CallBackUri, _scope);

            oauthDialogUri += string.IsNullOrEmpty(authenticationServiceSettings.State)
                            ? string.Empty
                            : "&state=" + authenticationServiceSettings.State;

            return new Uri(oauthDialogUri);
        }

        public IAuthenticatedClient AuthenticateClient(NameValueCollection parameters, string existingState = null)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            if (parameters.Count <= 0)
            {
                throw new ArgumentOutOfRangeException("parameters");
            }

            var state = parameters["state"];

            // CSRF (state) check.
            // NOTE: There is always a state provided. Even if an error is returned.
            if (!string.IsNullOrEmpty(existingState) && state != existingState)
            {
                throw new AuthenticationException(
                    "The states do not match. It's possible that you may be a victim of a CSRF.");
            }

            var reponse = RetrieveToken(parameters);
            var userInfo = RetrieveUserInfo(reponse);

            var result = new AuthenticatedClient(Name)
            {
                AccessToken = reponse.access_token,
                AccessTokenExpiresOn = DateTime.UtcNow.AddSeconds(int.Parse(reponse.expires_in)),
                UserInformation = new UserInformation
                {
                    Name = string.Join(" ", userInfo.first_name, userInfo.last_name),
                    Locale = userInfo.locale,
                    UserName = userInfo.name,
                    Id = userInfo.id,
                    Email = userInfo.emails.preferred,
                    Gender = (GenderType) Enum.Parse(typeof (GenderType), userInfo.gender ?? "Unknown", true)
                }
            };

            return result;
        }

        private AuthenticatedToken RetrieveToken(NameValueCollection parameters)
        {
            var request = new RestRequest("/oauth20_token.srf");
            var client = _restClientFactory.CreateRestClient("https://login.live.com/oauth20_token.srf");
            
            request.AddParameter("client_id", _clientId);
            request.AddParameter("redirect_uri", CallBackUri);
            request.AddParameter("client_secret", _clientSecret);
            request.AddParameter("code", parameters["code"]);
            request.AddParameter("grant_type", "authorization_code");

            return client.Execute<AuthenticatedToken>(request).Data;
        }

        private UserInfo RetrieveUserInfo(AuthenticatedToken reponse)
        {
            var userRequest = new RestRequest("/v5.0/me");
            var userClient = _restClientFactory.CreateRestClient("https://apis.live.net");

            userRequest.AddParameter("access_token", reponse.access_token);

            return userClient.Execute<UserInfo>(userRequest).Data;
        }

        // ReSharper disable InconsistentNaming
        protected class UserInfo
        {
            public string id { get; set; }
            public string name { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string link { get; set; }
            public string gender { get; set; }
            public string locale { get; set; }
            public string updated_time { get; set; }
            public Emails emails { get; set; }
        }

        protected class Emails
        {
            public string preferred { get; set; }
        }

        protected class AuthenticatedToken
        {
            public string token_type { get; set; }
            public string expires_in { get; set; }
            public string scope { get; set; }
            public string access_token { get; set; }
            public string refresh_token { get; set; }
            public string authentication_token { get; set; }
        }
        // ReSharper restore InconsistentNaming
    }
}
