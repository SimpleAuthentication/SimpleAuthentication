using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers.Google;
using SimpleAuthentication.Core.Providers.OAuth.V20;
using SimpleAuthentication.Core.Tracing;

namespace SimpleAuthentication.Core.Providers
{
    public class GoogleProvider : OAuth20Provider
    {
        public GoogleProvider(ProviderParams providerParams) : base(providerParams)
        {
            ScopeSeparator = " ";
        }

        #region IAuthenticationProvider Implementation

        public override string Name
        {
            get { return "Google"; }
        }

        public override async Task<RedirectToAuthenticateSettings> GetRedirectToAuthenticateSettingsAsync(Uri callbackUrl)
        {
            if (callbackUrl == null)
            {
                throw new ArgumentNullException("callbackUrl");
            }

            var providerAuthenticationUrl = new Uri("https://accounts.google.com/o/oauth2/auth");
            return GetRedirectToAuthenticateSettings(callbackUrl, providerAuthenticationUrl);
        }

        #endregion

        #region OAuth20Provider Implementation

        protected override IEnumerable<string> DefaultScopes
        {
            get { return new[] {"profile", "email"}; }
        }

        protected override Uri AccessTokenUri
        {
            get { return new Uri("https://accounts.google.com/o/oauth2/token"); }
        }

        protected override Uri GetUserInformationUri(AccessToken accessToken)
        {
            if (accessToken == null)
            {
                throw new ArgumentNullException();
            }

            var requestUri = string.Format("https://www.googleapis.com/oauth2/v2/userinfo?access_token={0}", 
                accessToken.Token);

            return new Uri(requestUri);
        }

        protected override UserInformation GetUserInformationFromContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException("content");
            }

            var userInfoResult = JsonConvert.DeserializeObject<UserInfoResult>(content);

            return new UserInformation
            {
                Id = userInfoResult.Id,
                Gender = string.IsNullOrWhiteSpace(userInfoResult.Gender)
                    ? GenderType.Unknown
                    : GenderTypeHelpers.ToGenderType(userInfoResult.Gender),
                Name = userInfoResult.Name,
                Email = userInfoResult.Email,
                Locale = userInfoResult.Locale,
                Picture = userInfoResult.Picture,
                UserName = userInfoResult.GivenName
            };
        }

        #endregion
    }
}