using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Providers.OAuth.V20;
using SimpleAuthentication.ExtraProviders.LinkedIn;

namespace SimpleAuthentication.ExtraProviders
{
    public class LinkedInProvider : OAuth20Provider
    {
        public LinkedInProvider(ProviderParams providerParams)
            : base(providerParams)
        {
            ScopeSeparator = " ";
        }

        #region OAuth20Provider Implementation

        public override string Name
        {
            get { return "LinkedIn"; }
        }

        protected override IEnumerable<string> DefaultScopes
        {
            get { return new[] { "r_basicprofile", "r_emailaddress" }; }
        }

        protected override Uri AccessTokenUri
        {
            get { return new Uri("https://www.linkedin.com/uas/oauth2/accessToken"); }
        }

        public override async Task<RedirectToAuthenticateSettings> GetRedirectToAuthenticateSettingsAsync(
            Uri callbackUrl)
        {
            if (callbackUrl == null)
            {
                throw new ArgumentNullException("callbackUrl");
            }

            var providerAuthenticationUrl = new Uri("https://www.linkedin.com/uas/oauth2/authorization");
            return GetRedirectToAuthenticateSettings(callbackUrl, providerAuthenticationUrl);
        }

        protected override AccessToken MapAccessTokenContentToAccessToken(string content)
        {
            return MapAccessTokenContentToAccessTokenForSomeJson(content);
        }

        protected override Uri UserInformationUri(AccessToken accessToken)
        {
            var userInformationUrl = string.Format("https://api.linkedin.com/v1/people/~:(id,formatted-name,email-address,picture-url)?oauth2_access_token={0}&format=json",
                accessToken.Token);
            return new Uri(userInformationUrl);
        }

        protected override string UserAgent
        {
            get { return "SimpleAuthentication-App"; }
        }

        protected override UserInformation GetUserInformationFromContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException("content");
            }

            var userInfo = JsonConvert.DeserializeObject<UserInfo>(content);

            return new UserInformation
            {
                Id = userInfo.Id,
                Name = userInfo.Name,
                Email = userInfo.Email,
                Picture = userInfo.AvatarUrl,
            };
        }

        #endregion
    }
}