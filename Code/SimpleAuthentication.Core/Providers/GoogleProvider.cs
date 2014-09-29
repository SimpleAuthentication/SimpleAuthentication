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

        public override RedirectToAuthenticateSettings GetRedirectToAuthenticateSettings(Uri callbackUrl)
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

        protected override Uri UserInformationUri(AccessToken accessToken)
        {
            if (accessToken == null)
            {
                throw new ArgumentNullException();
            }

            var requestUri = string.Format("https://www.googleapis.com/plus/v1/people/me?access_token={0}", 
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
                Name = string.Format("{0} {1}",
                    userInfoResult.Name.GivenName,
                    userInfoResult.Name.FamilyName).Trim(),
                Email = userInfoResult.Emails != null &&
                        userInfoResult.Emails.Any()
                    ? userInfoResult.Emails.First().Value
                    : null,
                Locale = userInfoResult.Language,
                Picture = userInfoResult.Image.Url,
                UserName = userInfoResult.DisplayName
            };
        }

        #endregion
    }
}