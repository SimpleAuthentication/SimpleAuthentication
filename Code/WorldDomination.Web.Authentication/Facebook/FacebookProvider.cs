using System;
using System.Collections.Generic;
using System.Web.Mvc;
using CuttingEdge.Conditions;
using Newtonsoft.Json;

namespace WorldDomination.Web.Authentication.Facebook
{
    // REFERENCE: http://developers.facebook.com/docs/authentication/server-side/

    public class FacebookProvider
    {
        private const string OauthDialogUri = "https://www.facebook.com/dialog/oauth?client_id={0}&redirect_uri={1}";

        private const string TokenUri =
            "https://graph.facebook.com/oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}";

        private const string MeUri = "https://graph.facebook.com/me?access_token={0}";

        private const string ScopeKey = "&scope={0}";
        private const string StateKey = "&state={0}";
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly Uri _redirectUri;
        private readonly IList<string> _scope;
        private readonly IWebClientWrapper _webClient;

        public FacebookProvider(string clientId, string clientSecret, Uri redirectUri, IWebClientWrapper webClient) : this(clientId, clientSecret, redirectUri, null, webClient)
        {
        }

        public FacebookProvider(string clientId, string clientSecret, Uri redirectUri, IList<string> scope, IWebClientWrapper webClient)
        {
            Condition.Requires(clientId).IsNotNullOrEmpty();
            Condition.Requires(clientSecret).IsNotNullOrEmpty();
            Condition.Requires(redirectUri).IsNotNull();
            Condition.Requires(webClient).IsNotNull();

            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUri = redirectUri;
            _webClient = webClient;

            // Optionals.
            _scope = scope;
        }

        public RedirectResult RedirectToAuthenticate(string state)
        {
            Condition.Requires(state).IsNotNullOrEmpty();

            var oauthDialogUri = string.Format(OauthDialogUri, _clientId, _redirectUri.AbsoluteUri);

            // Do we have any scope options?
            if (_scope != null && _scope.Count > 0)
            {
                oauthDialogUri += string.Format(ScopeKey, string.Join(",", _scope));
            }

            oauthDialogUri += string.Format(StateKey, state);

            return new RedirectResult(oauthDialogUri);
        }

        public void RetrieveAccessToken(FacebookClient facebookClient)
        {
            Condition.Requires(facebookClient).IsNotNull();
            Condition.Requires(facebookClient.Code).IsNotNullOrEmpty();

            var tokenUri = string.Format(TokenUri, _clientId, _redirectUri, _clientSecret, facebookClient.Code);
            var responseBody = _webClient.DownloadString(tokenUri);

            if (string.IsNullOrEmpty(responseBody))
            {
                throw new InvalidOperationException("No access token was received from Facebook.");
            }

            // Extract the relevant data.
            ExtractContent(responseBody, facebookClient);

            // Grab the user's information.
            facebookClient.UserInformation = RetrieveUserInformation(facebookClient.AccessToken);
        }

        private static void ExtractContent(string responseBody, FacebookClient facebookClient)
        {
            const string accessTokenKey = "access_token=";
            const string expiresKey = "expires=";

            Condition.Requires(responseBody).IsNotNullOrEmpty();
            Condition.Requires(facebookClient).IsNotNull();

            var data = responseBody.Split(new[] {'&'}, StringSplitOptions.RemoveEmptyEntries);
            Condition.Requires(data).IsNotNull().HasLength(2);
            int foundItems = 0;
            foreach (var content in data)
            {
                // What do we have?
                if (content.StartsWith(accessTokenKey))
                {
                    facebookClient.AccessToken = ExtractContent(content, accessTokenKey);
                    foundItems++;
                }
                else if (content.StartsWith(expiresKey))
                {
                    facebookClient.ExpiresOn =
                        DateTime.UtcNow.AddSeconds(Convert.ToInt32(ExtractContent(content, expiresKey)));
                    foundItems++;
                }
            }

            // Check to make sure we've correctly extracted all the required data.
            Condition.Requires(foundItems).IsEqualTo(2);
        }

        private static string ExtractContent(string content, string tokenKey)
        {
            return content.Substring(content.IndexOf(tokenKey, StringComparison.Ordinal) + tokenKey.Length);
        }

        private UserInformation RetrieveUserInformation(string accessToken)
        {
            Condition.Requires(accessToken).IsNotNull();

            var responseBody = _webClient.DownloadString(string.Format(MeUri, accessToken));
            if (string.IsNullOrEmpty(responseBody))
            {
                throw new InvalidOperationException("No user information was received from Facebook.");
            }

            try
            {
                var data = JsonConvert.DeserializeObject<MeResult>(responseBody);
                return new UserInformation
                       {
                           Id = data.Id,
                           Name = (data.FirstName + " " + data.LastName).Trim(),
                           Locale = data.Locale,
                           UserName = data.Username
                       };
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Failed to deserialize the json user information result from Facebook.", exception);
            }
        }
    }
}