using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using WorldDomination.Web.Authentication.ExtraProviders.WindowsLive;

namespace WorldDomination.Web.Authentication.ExtraProviders
{
    public class WindowsLiveProvider : IAuthenticationProvider
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly IRestClientFactory _restClientFactory;
        private readonly string _url = "https://login.live.com/oauth20_authorize.srf?client_id={0}&scope={2}&response_type=code&redirect_uri={1}";
        private readonly string _scope = string.Join(" ", new[] { "wl.signin", "wl.basic", "wl.emails" });

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

            var oauthDialogUri = string.Format(_url, _clientId, CallBackUri, _scope);

            return new Uri(oauthDialogUri);
        }

        public IAuthenticatedClient AuthenticateClient(NameValueCollection parameters, string existingState)
        {
            var result = new AuthenticatedClient(Name);

            var request = new RestRequest("/oauth20_token.srf");
            request.AddParameter("client_id", _clientId);
            request.AddParameter("redirect_uri", CallBackUri);
            request.AddParameter("client_secret", _clientSecret);
            request.AddParameter("code", parameters["code"]);
            request.AddParameter("grant_type", "authorization_code");
            var client = _restClientFactory.CreateRestClient("https://login.live.com/oauth20_token.srf");

            var reponse = client.Execute<AuthenticatedToken>(request);

            var userRequest = new RestRequest("/v5.0/me");
            var userClient = _restClientFactory.CreateRestClient("https://apis.live.net");
            userRequest.AddParameter("access_token", reponse.Data.access_token);

            var userInfoResponse = userClient.Execute<UserInfo>(userRequest);
            var userInfo = userInfoResponse.Data;

            result.AccessToken = reponse.Data.access_token;
            result.AccessTokenExpiresOn = DateTime.UtcNow.AddSeconds(int.Parse(reponse.Data.expires_in));
            result.UserInformation = new UserInformation
            {
                Name = string.Join(" ", userInfo.first_name, userInfo.last_name),
                Locale = userInfo.locale,
                UserName = userInfo.name,
                Id = userInfo.id,
                Email = userInfo.emails.preferred,
                Gender = (GenderType)Enum.Parse(typeof(GenderType), userInfo.gender ?? "Unknown", true),
                
            };

            return result;
        }

        public class UserInfo
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
        public class Emails
        {
            public string preferred { get; set; }
        }

        public class AuthenticatedToken
        {
            public string token_type { get; set; }
            public string expires_in { get; set; }
            public string scope { get; set; }
            public string access_token { get; set; }
            public string refresh_token { get; set; }
            public string authentication_token { get; set; }
        }
    }
}
