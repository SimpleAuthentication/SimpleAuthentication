using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers.OAuth.V10a;
using SimpleAuthentication.Core.Providers.Twitter;

namespace SimpleAuthentication.Core.Providers
{
    public class TwitterProvider : OAuth10Provider
    {
        public TwitterProvider(ProviderParams providerParams)
            : base(providerParams)
        {
        }

        #region IAuthenticationProvider Implementation

        public override string Name
        {
            get { return "Twitter"; }
        }

        public override string Description
        {
            get { return "OAuth 1.0a"; }
        }

        public override async Task<RedirectToAuthenticateSettings> GetRedirectToAuthenticateSettingsAsync(
            Uri callbackUrl)
        {
            //TraceSource.TraceVerbose("Retrieving the Request Token.");

            if (callbackUrl == null)
            {
                throw new ArgumentNullException("callbackUrl");
            }

            var state = Guid.NewGuid();
            var updatedCallbackUrl = SystemHelpers.CreateUri(callbackUrl,
                new Dictionary<string, string> {{"state", state.ToString()}});

            var settings = await GetRedirectToAuthenticateSettingsAsync(updatedCallbackUrl,
                new Uri("http://twitter.com/oauth/request_token"));

            settings.State = state.ToString();
            return settings;
        }

        #endregion

        protected override Uri GetRedirectToProviderUri(RequestToken requestToken)
        {
            if (requestToken == null)
            {
                throw new ArgumentNullException("requestToken");
            }

            var redirectUri = string.Format("https://twitter.com/oauth/authenticate?oauth_token={0}",
                requestToken.OAuthToken);

            return new Uri(redirectUri);
        }

        protected override AccessToken GetAccessTokenFromResponseContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException("content");
            }

            var keyValues = SystemHelpers.ConvertKeyValueContentToDictionary(content);
            if (keyValues == null)
            {
                var errorMessage =
                    string.Format(
                        "Access Token response content from Twitter expected some key/value content but none were retrieved. Content: {0}.",
                        string.IsNullOrWhiteSpace(content)
                            ? "-- no content --"
                            : content);
                throw new AuthenticationException(errorMessage);
            }

            const string tokenKey = "oauth_token";
            const string secretKey = "oauth_token_secret";

            foreach (var key in new[] {tokenKey, secretKey})
            {
                if (!keyValues.ContainsKey(key))
                {
                    var errorMessage =
                        string.Format(
                            "Access token response content from Twitter expected the key/value '{0}' but none was retrieved. Content: {1}",
                            key,
                            string.IsNullOrWhiteSpace(content)
                                ? "-- no content --"
                                : content);
                    throw new AuthenticationException(errorMessage);
                }
            }

            // NOTE: Twitter doesn't use the expires on. This means an access token can be reused forever until the individual
            //       app or accessToken has been revoked.
            return new AccessToken
            {
                Token = keyValues[tokenKey],
                Secret = keyValues[secretKey],
                ExpiresOn = DateTime.MaxValue
            };
        }

        protected override UserInformation GetUserInformationFromContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException("content");
            }

            TwitterUserInformation verifyCredentials;
            try
            {
                verifyCredentials = JsonConvert.DeserializeObject<TwitterUserInformation>(content);
            }
            catch (Exception exception)
            {
                var errorMessage =
                    string.Format(
                        "Failed to deserialize the Twitter Verify Credentials response json content. Possibly because the content isn't json? Content attempted: {0}",
                        string.IsNullOrWhiteSpace(content)
                            ? "-- no content"
                            : content);
                throw new AuthenticationException(errorMessage, exception);
            }

            if (verifyCredentials == null)
            {
                var errorMessage =
                    string.Format(
                        "Failed to deserialize the Twitter Verify Credentials response json content. No content returned? Content attempted: {0}",
                        string.IsNullOrWhiteSpace(content)
                            ? "-- no content"
                            : content);

                throw new AuthenticationException(errorMessage);
            }

            return new UserInformation
            {
                Name = verifyCredentials.name,
                Id = verifyCredentials.id.ToString(),
                Locale = verifyCredentials.lang,
                UserName = verifyCredentials.screen_name,
                Picture = verifyCredentials.profile_image_url
            };
        }
    }
}