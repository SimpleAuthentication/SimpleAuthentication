using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers.Facebook;
using SimpleAuthentication.Core.Providers.OAuth.V20;

namespace SimpleAuthentication.Core.Providers
{
    // REFERENCE: https://developers.facebook.com/docs/facebook-login/login-flow-for-web-no-jssdk/

    public class FacebookProvider : OAuth20Provider
    {
        public FacebookProvider(ProviderParams providerParams) : base(providerParams)
        {
            DisplayType = DisplayType.Unknown;
            IsMobile = false;
        }

        public DisplayType DisplayType { get; set; }
        public bool IsMobile { get; set; }

        #region IAuthenticationProvider Implementation

        public override string Name
        {
            get { return "Facebook"; }
        }

        #endregion

        #region OAuth20Provider Implementation

        protected override IEnumerable<string> DefaultScopes
        {
            get { return new[] {"public_profile", "email"}; }
        }

        protected override Uri AccessTokenUri
        {
            get { return new Uri("https://graph.facebook.com/oauth/access_token"); }
        }

        protected override Uri GetUserInformationUri(AccessToken accessToken)
        {
            var requestUri = string.Format(
                "https://graph.facebook.com/v2.0/me?fields=id,name,gender,email,link,locale&access_token={0}",
                accessToken.Token);

            return new Uri(requestUri);
        }

        public override async Task<RedirectToAuthenticateSettings> GetRedirectToAuthenticateSettingsAsync(
            Uri callbackUrl)
        {
            if (callbackUrl == null)
            {
                throw new ArgumentNullException("callbackUrl");
            }

            var url = IsMobile
                ? "https://m.facebook.com/dialog/oauth"
                : "https://www.facebook.com/dialog/oauth";

            var providerAuthenticationUrl = new Uri(url);
            var settings = GetRedirectToAuthenticateSettings(callbackUrl, providerAuthenticationUrl);

            // Don't forget to append this Facebook specific option: DisplayType.
            if (DisplayType != DisplayType.Unknown)
            {
                settings.RedirectUri = SystemHelpers.CreateUri(settings.RedirectUri,
                    new Dictionary<string, string>
                    {{"display", DisplayType.ToString().ToLowerInvariant()}});
            }

            return settings;
        }

        protected override AccessToken MapAccessTokenContentToAccessToken(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException("content");
            }

            // The data is in a key/value format. So lets split this data out.
            var keyValues = SystemHelpers.ConvertKeyValueContentToDictionary(content);
            if (keyValues == null ||
                !keyValues.Any())
            {
                var errorMessage =
                    string.Format("Failed to extract the access token from the body content. Content returned: {0}",
                        content);
                throw new AuthenticationException(errorMessage);
            }

            const string tokenKey = "access_token";
            const string expiresOnKey = "expires";

            if (!keyValues.ContainsKey(tokenKey) &&
                !keyValues.ContainsKey(expiresOnKey))
            {
                var errorMessage =
                    string.Format(
                        "Failed to extract the access token from the body content. Content needs both the '{0}' and '{1}' keys.",
                        tokenKey,
                        expiresOnKey);
                throw new AuthenticationException(errorMessage);
            }

            var expiresIn = Convert.ToDouble(keyValues[expiresOnKey], CultureInfo.InvariantCulture);
            return new AccessToken
            {
                Token = keyValues[tokenKey],
                ExpiresOn = expiresIn > 0
                    ? DateTime.UtcNow.AddSeconds(expiresIn)
                    : DateTime.MaxValue
            };
        }

        protected override UserInformation GetUserInformationFromContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException("content");
            }

            var meResult = JsonConvert.DeserializeObject<MeResult>(content);

            // Note: UserName has been removed from API >= 2.0.
            // REF: https://developers.facebook.com/docs/apps/upgrading
            //      => "The /me/username field has been removed."
            return new UserInformation
            {
                Id = meResult.Id < 0
                    ? "0"
                    : meResult.Id.ToString(),
                Name = meResult.Name.Trim(),
                Email = meResult.Email,
                Locale = meResult.Locale,
                Gender = string.IsNullOrWhiteSpace(meResult.Gender)
                    ? GenderType.Unknown
                    : GenderTypeHelpers.ToGenderType(meResult.Gender),
                Picture = string.Format("https://graph.facebook.com/{0}/picture", meResult.Id)
            };
        }

        #endregion
    }
}