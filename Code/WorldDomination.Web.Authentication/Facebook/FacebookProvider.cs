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

        public FacebookProvider(string clientId, string clientSecret, Uri redirectUri, IWebClientWrapper webClient)
        {
            Condition.Requires(clientId).IsNotNullOrEmpty();
            Condition.Requires(clientSecret).IsNotNullOrEmpty();
            Condition.Requires(redirectUri).IsNotNull();
            Condition.Requires(webClient).IsNotNull();

            ClientId = clientId;
            ClientSecret = clientSecret;
            RedirectUri = redirectUri;
            WebClient = webClient;
        }

        public string ClientId { get; private set; }
        public string ClientSecret { get; private set; }
        public Uri RedirectUri { get; private set; }
        public IList<string> Scope { get; set; }
        public string State { get; private set; }
        private IWebClientWrapper WebClient { get; set; }

        public RedirectResult RedirectToAuthenticate(string state)
        {
            Condition.Requires(state).IsNotNullOrEmpty();
            State = state;

            var oauthDialogUri = string.Format(OauthDialogUri, ClientId, RedirectUri.AbsoluteUri);

            // Do we have any scope options?
            if (Scope != null && Scope.Count > 0)
            {
                oauthDialogUri += string.Format(ScopeKey, string.Join(",", Scope));
            }

            oauthDialogUri += string.Format(StateKey, State);

            return new RedirectResult(oauthDialogUri);
        }

        public void RetrieveAccessToken(FacebookClient facebookClient)
        {
            Condition.Requires(facebookClient).IsNotNull();
            Condition.Requires(facebookClient.Code).IsNotNullOrEmpty();

            var tokenUri = string.Format(TokenUri, ClientId, RedirectUri, ClientSecret, facebookClient.Code);
            var responseBody = WebClient.DownloadString(tokenUri);

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

            var responseBody = WebClient.DownloadString(string.Format(MeUri, accessToken));
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
                           FirstName = data.FirstName,
                           LastName = data.LastName,
                           Locale = data.Locale,
                           Name = data.Username
                       };
            }
            catch(Exception exception)
            {
                throw new InvalidOperationException("Failed to deserialize the json user information result from Facebook.", exception);
            }
        }
    }
}