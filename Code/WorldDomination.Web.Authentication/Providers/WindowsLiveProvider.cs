﻿using System;
using System.Collections.Specialized;
using RestSharp;
using WorldDomination.Web.Authentication.Providers.WindowsLive;

namespace WorldDomination.Web.Authentication.Providers
{
    public class WindowsLiveProvider : BaseProvider, IAuthenticationProvider
    {
        // *********************************************************************
        // REFERENCE: http://msdn.microsoft.com/en-us/library/live/hh243647.aspx
        // *********************************************************************


        private const string RedirectUrl =
            "https://login.live.com/oauth20_authorize.srf?client_id={0}&scope={2}&response_type=code&redirect_uri={1}";

        private readonly string _clientId;
        private readonly string _clientSecret;

        private readonly string _scope = string.Join(" ", new[] {"wl.signin", "wl.basic", "wl.emails"});

        public WindowsLiveProvider(ProviderParams providerParams)
        {
            providerParams.Validate();

            _clientId = providerParams.Key;
            _clientSecret = providerParams.Secret;
        }

        private AuthenticatedToken RetrieveToken(NameValueCollection queryStringParameters, Uri redirectUri)
        {
            if (queryStringParameters == null)
            {
                throw new ArgumentNullException();
            }

            if (redirectUri == null ||
                string.IsNullOrEmpty(redirectUri.AbsoluteUri))
            {
                throw new ArgumentNullException();
            }

            var request = new RestRequest("/oauth20_token.srf");
            var client = RestClientFactory.CreateRestClient("https://login.live.com/oauth20_token.srf");

            request.AddParameter("client_id", _clientId);
            request.AddParameter("redirect_uri", redirectUri);
            request.AddParameter("client_secret", _clientSecret);
            request.AddParameter("code", queryStringParameters["code"]);
            request.AddParameter("grant_type", "authorization_code");

            return client.Execute<AuthenticatedToken>(request).Data;
        }

        private UserInfo RetrieveUserInfo(AuthenticatedToken reponse)
        {
            var userRequest = new RestRequest("/v5.0/me");
            var userClient = RestClientFactory.CreateRestClient("https://apis.live.net");

            userRequest.AddParameter("access_token", reponse.AccessToken);

            return userClient.Execute<UserInfo>(userRequest).Data;
        }

        #region Implementation of IAuthenticationProvider

        public string Name
        {
            get { return "WindowsLive"; }
        }

        public IAuthenticationServiceSettings DefaultAuthenticationServiceSettings
        {
            get { return new WindowsLiveAuthenticationServiceSettings(); }
        }

        public Uri RedirectToAuthenticate(IAuthenticationServiceSettings authenticationServiceSettings)
        {
            var oauthDialogUri = string.Format(RedirectUrl, _clientId, authenticationServiceSettings.CallBackUri.AbsoluteUri, _scope);

            oauthDialogUri += string.IsNullOrEmpty(authenticationServiceSettings.State)
                                  ? string.Empty
                                  : "&state=" + authenticationServiceSettings.State;

            return new Uri(oauthDialogUri);
        }

        public IAuthenticatedClient AuthenticateClient(IAuthenticationServiceSettings authenticationServiceSettings,
                                                       NameValueCollection queryStringParameters)
        {
            if (authenticationServiceSettings == null)
            {
                throw new ArgumentNullException("authenticationServiceSettings");
            }

            if (queryStringParameters == null)
            {
                throw new ArgumentNullException("queryStringParameters");
            }

            if (queryStringParameters.Count <= 0)
            {
                throw new ArgumentOutOfRangeException("queryStringParameters");
            }

            var reponse = RetrieveToken(queryStringParameters, authenticationServiceSettings.CallBackUri);
            var userInfo = RetrieveUserInfo(reponse);

            var result = new AuthenticatedClient(Name)
                         {
                             AccessToken = reponse.AccessToken,
                             AccessTokenExpiresOn = DateTime.UtcNow.AddSeconds(int.Parse(reponse.ExpiresIn)),
                             UserInformation = new UserInformation
                                               {
                                                   Name = string.Join(" ", userInfo.first_name, userInfo.last_name),
                                                   Locale = userInfo.locale,
                                                   UserName = userInfo.name,
                                                   Id = userInfo.id,
                                                   Email = userInfo.emails.Preferred,
                                                   Gender =
                                                       (GenderType)
                                                       Enum.Parse(typeof (GenderType), userInfo.gender ?? "Unknown",
                                                                  true)
                                               }
                         };

            return result;
        }

        #endregion
    }
}