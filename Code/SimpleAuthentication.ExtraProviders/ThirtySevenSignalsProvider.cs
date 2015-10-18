using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Providers;
using SimpleAuthentication.Core.Tracing;
using SimpleAuthentication.ExtraProviders.ThirtySevenSignals;

namespace SimpleAuthentication.ExtraProviders
{
    // REFERENCE: https://github.com/37signals/api/blob/master/sections/authentication.md

    public class ThirtySevenSignalsProvider : BaseOAuth20Provider<AccessTokenResult>
    {
        private const string BaseUri = "https://launchpad.37signals.com";

        public ThirtySevenSignalsProvider(ProviderParams providerParams) : this("ThirtySevenSignals", providerParams)
        {
        }

        protected ThirtySevenSignalsProvider(string name, ProviderParams providerParams) : base(name, providerParams)
        {
            AuthenticateRedirectionUrl = new Uri("https://launchpad.37signals.com/authorization/new");
        }

        #region BaseOAuth20ProviderImplementation

        public override IEnumerable<string> DefaultScopes
        {
            get { return new[] {""}; }
        }

        protected override string CreateRedirectionQuerystringParameters(Uri callbackUri, string state)
        {
            if (callbackUri == null)
            {
                throw new ArgumentNullException("callbackUri");
            }

            if (string.IsNullOrEmpty(state))
            {
                throw new ArgumentNullException("state");
            }

            // Reference: https://launchpad.37signals.com/authorization/new?type=web_server&client_id=your-client-id&redirect_uri=your-redirect-uri

            return string.Format("type=web_server&client_id={0}&redirect_uri={1}{2}",
                                 PublicApiKey, callbackUri, GetQuerystringState(state));
        }

        protected override IRestResponse<AccessTokenResult> ExecuteRetrieveAccessToken(string authorizationCode,
                                                                                       Uri redirectUri)
        {
            if (string.IsNullOrEmpty(authorizationCode))
            {
                throw new ArgumentNullException("authorizationCode");
            }

            if (redirectUri == null ||
                string.IsNullOrEmpty(redirectUri.AbsoluteUri))
            {
                throw new ArgumentNullException("redirectUri");
            }

            // Reference: https://launchpad.37signals.com/authorization/token?type=web_server&client_id=your-client-id&redirect_uri=your-redirect-uri&client_secret=your-client-secret&code=verification-code

            var restRequest = new RestRequest("/authorization/token", Method.POST);
            restRequest.AddParameter("type", "web_server");
            restRequest.AddParameter("client_id", PublicApiKey);
            restRequest.AddParameter("client_secret", SecretApiKey);
            restRequest.AddParameter("redirect_uri", redirectUri.AbsoluteUri);
            restRequest.AddParameter("code", authorizationCode);

            var restClient = RestClientFactory.CreateRestClient(BaseUri);

            TraceSource.TraceVerbose("Retrieving Access Token endpoint: {0}; Post Params: {1}",
                                     restClient.BuildUri(restRequest).AbsoluteUri,
                                     string.Join(", ", restRequest.Parameters));

            return restClient.Execute<AccessTokenResult>(restRequest);
        }

        protected override AccessToken MapAccessTokenResultToAccessToken(AccessTokenResult accessTokenResult)
        {
            if (accessTokenResult == null)
            {
                throw new ArgumentNullException("accessTokenResult");
            }

            if (string.IsNullOrEmpty(accessTokenResult.access_token) ||
                accessTokenResult.expires_in <= 0)
            {
                const string errorMessage =
                    "Retrieved a 37 Signals Access Token but it doesn't contain one of the following: access_token or expires_in.";
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return new AccessToken
                   {
                       PublicToken = accessTokenResult.access_token,
                       SecretToken = accessTokenResult.refresh_token,
                       ExpiresOn = DateTime.UtcNow.AddSeconds(accessTokenResult.expires_in)
                   };
        }

        protected override UserInformation RetrieveUserInformation(AccessToken accessToken)
        {
            if (accessToken == null)
            {
                throw new ArgumentNullException("accessToken");
            }

            if (string.IsNullOrEmpty(accessToken.PublicToken))
            {
                throw new ArgumentException("accessToken.PublicToken");
            }

            IRestResponse<UserInfoResult> response;

            try
            {
                var restRequest = new RestRequest("/authorization.json", Method.GET);
                restRequest.AddParameter("access_token", accessToken.PublicToken);

                var restClient = RestClientFactory.CreateRestClient("https://launchpad.37signals.com");

                TraceSource.TraceVerbose("Retrieving user information. 37 Signals Endpoint: {0}",
                                         restClient.BuildUri(restRequest).AbsoluteUri);

                response = restClient.Execute<UserInfoResult>(restRequest);
            }
            catch (Exception exception)
            {
                var errorMessage =
                    string.Format("Failed to retrieve any UserInfo data from the 37 Signals Api. Error Messages: {0}",
                                  exception.RecursiveErrorMessages());
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage, exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                var errorMessage = string.Format(
                    "Failed to obtain some UserInfo data from the 37 Signals Api OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}. Error Message: {2}.",
                    response == null ? "-- null response --" : response.StatusCode.ToString(),
                    response == null ? string.Empty : response.StatusDescription,
                    response == null
                        ? string.Empty
                        : response.ErrorException == null
                              ? "--no error exception--"
                              : response.ErrorException.RecursiveErrorMessages());

                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            // Lets check to make sure we have some bare minimum data.
            if (response.Data.Identity == null ||
                string.IsNullOrEmpty(response.Data.Identity.Id))
            {
                const string errorMessage =
                    "We were unable to retrieve the User Id from 37 Signals Api, the user may have denied the authorization.";
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            UserInformation userInformation = null;
            //if (response.Data.Accounts != null &&
            //    response.Data.Accounts.Count > 0)
            //{
            //    userInformation = ExtractUserInformationFromProject(response.Data.Accounts[0], accessToken);
            //}

            return userInformation ?? (new UserInformation
                                       {
                                           Id = response.Data.Identity.Id,
                                           Gender = GenderType.Unknown,
                                           Name =
                                               string.Format("{0} {1}",
                                                             response.Data.Identity.First_name ?? string.Empty,
                                                             response.Data.Identity.Last_name ?? string.Empty),
                                           Email = response.Data.Identity.Email_address
                                       });
        }

        #endregion

        private UserInformation ExtractUserInformationFromProject(Account account, AccessToken accessToken)
        {
            if (account == null)
            {
                throw new ArgumentNullException("account");
            }

            if (accessToken == null)
            {
                throw new ArgumentNullException("accessToken");
            }

            // Determine which particular API we need to hit.
            switch (account.Product.ToLowerInvariant())
            {
                case "basecamp":
                    return ExtractUserInformationFromBasecampClassic(account, accessToken);
                case "bcx":
                    return ExtractUserInformationFromBasecamp(account, accessToken);
                default:
                    return null;
            }
        }

        private UserInformation ExtractUserInformationFromBasecampClassic(Account account, AccessToken accessToken)
        {
            if (account == null)
            {
                throw new ArgumentNullException("account");
            }

            if (accessToken == null)
            {
                throw new ArgumentNullException("accessToken");
            }

            IRestResponse<PersonResult> response;

            try
            {
                //var restRequest = new RestRequest(string.Format("/people/{0}.xml", account.Id), Method.GET);
                var restRequest = new RestRequest("/me.xml", Method.GET);
                restRequest.AddParameter("access_token", accessToken.PublicToken);
                //restRequest.AddHeader("Authorization", "Bearer: " + accessToken.PublicToken);
                restRequest.AddHeader("User-Agent",
                                      "SimpleAuthentication .NET Library. (http://www.github.com/simpleauthentication");
                restRequest.AddHeader("Accept", "application/xml");
                restRequest.AddHeader("Content-Type", "application/xml");
                var restClient = RestClientFactory.CreateRestClient(account.Href);

                TraceSource.TraceVerbose("Retrieving /people/#{{person_id}}.xml data. 37 Signals Endpoint: {0}",
                                         restClient.BuildUri(restRequest).AbsoluteUri);

                // Reference Uri: {https://streetadvisor.basecamphq.com/772576/api/v1/people/me.json?access_token=BAhbByIByHsiZXhwaXJlc19hdCI6IjIwMTMtMDctMjBUMjM6NDc6NTRaIiwidXNlcl9pZHMiOls0NjE2OTUxLDY1NTgwNjUsODYyNDEwNywxMTc2MzU5NV0sImNsaWVudF9pZCI6ImFkZDMyZjZhYTJkNjJmNjUwMzEyY2ExOGM5MDhhYWMyMWE0NzNmMGIiLCJ2ZXJzaW9uIjoxLCJhcGlfZGVhZGJvbHQiOiIwMzY0ZTFmYjk3ZjI3MDEzNThhYjIwYzg5OWJjMGY5MCJ9dToJVGltZQ2XWhzABM1tvw==--f2ad7f5706d2e9c45e2c6dc1f32b5e62db2e56fc}

                response = restClient.Execute<PersonResult>(restRequest);
            }
            catch (Exception exception)
            {
                var errorMessage =
                    string.Format(
                        "Failed to retrieve any Person/me.json data from the 37 Signals Api. Error Messages: {0}",
                        exception.RecursiveErrorMessages());
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage, exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                var errorMessage = string.Format(
                    "Failed to obtain some person/me.json data from the 37 Signals Api OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}. Error Message: {2}.",
                    response == null ? "-- null response --" : response.StatusCode.ToString(),
                    response == null ? string.Empty : response.StatusDescription,
                    response == null
                        ? string.Empty
                        : response.ErrorException == null
                              ? "--no error exception--"
                              : response.ErrorException.RecursiveErrorMessages());

                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return new UserInformation
                   {
                       Email = response.Data.email_address,
                       Id = response.Data.identity_id,
                       Name = response.Data.Name,
                       Picture = response.Data.avatar_url
                   };
        }

        private UserInformation ExtractUserInformationFromBasecamp(Account account, AccessToken accessToken)
        {
            if (account == null)
            {
                throw new ArgumentNullException("account");
            }

            if (accessToken == null)
            {
                throw new ArgumentNullException("accessToken");
            }

            IRestResponse<PersonResult> response;

            try
            {
                var restRequest = new RestRequest(account.Id + "/api/v1/people/me.json", Method.GET);
                restRequest.AddParameter("access_token", accessToken.PublicToken);
                restRequest.AddHeader("User-Agent",
                                      "SimpleAuthentication .NET Library. (http://www.github.com/simpleauthentication");

                var restClient = RestClientFactory.CreateRestClient(account.Href);

                TraceSource.TraceVerbose("Retrieving person/me.json data. 37 Signals Endpoint: {0}",
                                         restClient.BuildUri(restRequest).AbsoluteUri);

                // Reference Uri: {https://streetadvisor.basecamphq.com/772576/api/v1/people/me.json?access_token=BAhbByIByHsiZXhwaXJlc19hdCI6IjIwMTMtMDctMjBUMjM6NDc6NTRaIiwidXNlcl9pZHMiOls0NjE2OTUxLDY1NTgwNjUsODYyNDEwNywxMTc2MzU5NV0sImNsaWVudF9pZCI6ImFkZDMyZjZhYTJkNjJmNjUwMzEyY2ExOGM5MDhhYWMyMWE0NzNmMGIiLCJ2ZXJzaW9uIjoxLCJhcGlfZGVhZGJvbHQiOiIwMzY0ZTFmYjk3ZjI3MDEzNThhYjIwYzg5OWJjMGY5MCJ9dToJVGltZQ2XWhzABM1tvw==--f2ad7f5706d2e9c45e2c6dc1f32b5e62db2e56fc}

                response = restClient.Execute<PersonResult>(restRequest);
            }
            catch (Exception exception)
            {
                var errorMessage =
                    string.Format(
                        "Failed to retrieve any Person/me.json data from the 37 Signals Api. Error Messages: {0}",
                        exception.RecursiveErrorMessages());
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage, exception);
            }

            if (response == null ||
                response.StatusCode != HttpStatusCode.OK)
            {
                var errorMessage = string.Format(
                    "Failed to obtain some person/me.json data from the 37 Signals Api OR the the response was not an HTTP Status 200 OK. Response Status: {0}. Response Description: {1}. Error Message: {2}.",
                    response == null ? "-- null response --" : response.StatusCode.ToString(),
                    response == null ? string.Empty : response.StatusDescription,
                    response == null
                        ? string.Empty
                        : response.ErrorException == null
                              ? "--no error exception--"
                              : response.ErrorException.RecursiveErrorMessages());

                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            return new UserInformation
                   {
                       Email = response.Data.email_address,
                       Id = response.Data.identity_id,
                       Name = response.Data.Name,
                       Picture = response.Data.avatar_url
                   };
        }
    }
}
