using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers.OAuth.V20;
using SimpleAuthentication.ExtraProviders.GitHub;

namespace SimpleAuthentication.ExtraProviders
{
    public class GitHubProvider : OAuth20Provider
    {
        public GitHubProvider(ProviderParams providerParams) : base(providerParams)
        {
        }

        #region OAuth20Provider Implementation

        public override string Name
        {
            get { return "GitHub"; }
        }

        protected override IEnumerable<string> DefaultScopes
        {
            get { return new[] {"user:email"}; }
        }

        protected override Uri AccessTokenUri
        {
            get { return new Uri("https://api.github.com/login/oauth/access_token"); }
        }

        public override async Task<RedirectToAuthenticateSettings> GetRedirectToAuthenticateSettingsAsync(
            Uri callbackUrl)
        {
            if (callbackUrl == null)
            {
                throw new ArgumentNullException("callbackUrl");
            }

            var providerAuthenticationUrl = new Uri("https://github.com/login/oauth/authorize");
            return GetRedirectToAuthenticateSettings(callbackUrl, providerAuthenticationUrl);
        }

        protected override AccessToken MapAccessTokenContentToAccessToken(string content)
        {
            return MapAccessTokenContentToAccessTokenForSomeKeyValues(content);
        }

        protected override Uri UserInformationUri(AccessToken accessToken)
        {
            var userInformationUrl = string.Format("https://api.github.com/user?access_token={0}",
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
                UserName = userInfo.Login
            };
        }

        #endregion
    }
}