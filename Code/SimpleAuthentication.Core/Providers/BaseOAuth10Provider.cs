using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Core.Tracing;

namespace SimpleAuthentication.Core.Providers
{
    public abstract class BaseOAuth10Provider<TRequestTokenResult> : BaseProvider, IPublicPrivateKeyProvider
        where TRequestTokenResult : class, new()
    {
        protected BaseOAuth10Provider(string name, ProviderParams providerParams, string description = null)
            : base(name, description)
        {

            PublicApiKey = providerParams.PublicApiKey;
            SecretApiKey = providerParams.SecretApiKey;
        }

        #region IPublicPrivateKeyProvider Implementation

        public string PublicApiKey { get; protected set; }
        public string SecretApiKey { get; protected set; }

        #endregion

        #region IAuthenticationProvider Members

        public override RedirectToAuthenticateSettings GetRedirectToAuthenticateSettings(Uri callbackUrl)
        {
            if (callbackUrl == null)
            {
                throw new ArgumentNullException("callbackUrl");
            }

            // Validations.
            if (AuthenticateRedirectionUrl == null)
            {
                throw new AuthenticationException(
                    "AuthenticationRedirectUrl has no value. Please set the authentication Url location to redirect to.");
            }

            if (string.IsNullOrWhiteSpace(PublicApiKey))
            {
                throw new AuthenticationException("PublicApiKey has no value. Please set this value.");
            }

            // Generate some state which will be used in the redirection uri and used for CSRF checks.
            var state = Guid.NewGuid().ToString();

            // 1st, we need to get a request token. Once we have this, then we can get a the access token
            // which is used in the redirect uri.
            // NOTE: This method is not awaited :/
            var requestToken = GetRequestTokenAsync(callbackUrl, state).Result;

            TraceSource.TraceInformation("Request Token received.");

            // Now the redirection uri.
            var redirectUri = CreateRedirectUri(requestToken);
            TraceSource.TraceInformation("{0} redirection uri: {1}.",
                Name,
                redirectUri);

            return new RedirectToAuthenticateSettings
            {
                RedirectUri = redirectUri,
                State = state
            };
        }

        public override async Task<IAuthenticatedClient> AuthenticateClientAsync(
            IDictionary<string, string> queryString,
            string state,
            Uri callbackUri)
        {
            #region Parameter checks

            if (queryString == null ||
                !queryString.Any())
            {
                throw new ArgumentNullException("queryString");
            }

            if (string.IsNullOrWhiteSpace(state))
            {
                throw new ArgumentNullException("state");
            }

            if (callbackUri == null)
            {
                throw new ArgumentNullException("callbackUri");
            }

            #endregion

            TraceSource.TraceInformation("Callback parameters: " +
                                         string.Join("&",
                                             queryString.Keys.Select(
                                                 key => key + "=" + queryString[key]).ToArray()));

            #region Cross Site Request Forgery checks -> state == state?

            // Start with the Cross Site Request Forgery check.
            var callbackState = queryString[StateKey];
            if (string.IsNullOrWhiteSpace(callbackState))
            {
                var errorMessage =
                    "The callback querystring doesn't include a state key/value parameter. We need one of these so we can to a CSRF check. Please check why the request url from the provider is missing the parameter: " +
                    StateKey + ". eg. &state=something...";
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            #endregion

            TraceSource.TraceVerbose("Retrieving the Access Token.");
            var accessToken = await GetAccessTokenAsync(queryString, callbackUri.AbsoluteUri);
            TraceSource.TraceVerbose("Access Token retrieved.");

            if (accessToken == null)
            {
                const string errorMessage = "No access token retrieved from provider. Unable to continue.";
                TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            TraceSource.TraceVerbose("Retrieving user information.");
            var userInformation = await RetrieveUserInformationAsync(accessToken);
            TraceSource.TraceVerbose("User information retrieved.");

            //var authenticatedClient = new AuthenticatedClient(Name.ToLowerInvariant())
            //{
            //    AccessToken = accessToken,
            //    UserInformation = userInformation
            //};

            //TraceSource.TraceInformation(authenticatedClient.ToString());

            //return authenticatedClient;

            throw new NotImplementedException();
        }

        #endregion

        protected abstract Task<TRequestTokenResult> GetRequestTokenAsync(Uri callbackUri, string state);

        protected abstract Task<AccessToken> GetAccessTokenAsync(IDictionary<string, string> queryString,
            string callbackUrl);

        protected abstract Task<UserInformation> RetrieveUserInformationAsync(AccessToken accessToken);

        protected abstract Uri CreateRedirectUri(TRequestTokenResult requestToken);

        // Attribution / Source: http://www.i-avington.com/Posts/Post/making-a-twitter-oauth-api-call-using-c
        protected string CreateAuthorizationHeader(string providerUrl, 
            string callbackUrl,
            string token = null)
        {
            if (string.IsNullOrWhiteSpace(providerUrl))
            {
                throw new ArgumentNullException("providerUrl");
            }

            if (string.IsNullOrWhiteSpace(callbackUrl))
            {
                throw new ArgumentNullException("callbackUrl");
            }

            var nonce = Convert.ToBase64String(new ASCIIEncoding()
                .GetBytes(DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture)));
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            var timeStamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString(CultureInfo.InvariantCulture);

            var oauthParameters = CreateOAuthParameters(callbackUrl, 
                PublicApiKey, 
                nonce, 
                timeStamp,
                token);

            var signature = CreateAuthorizationHeaderSignature(providerUrl,
                callbackUrl,
                PublicApiKey,
                SecretApiKey,
                nonce,
                timeStamp);

            oauthParameters.Add("oauth_signature", signature);

            var encodedParameters = EncodeOAuthParameters(oauthParameters, ", ");

            return string.Format("OAuth {0}", encodedParameters);
        }

        private static string CreateAuthorizationHeaderSignature(string providerUrl,
            string callbackUrl,
            string publicApiKey,
            string secretApiKey,
            string nonce,
            string timeStamp)
        {
            var signatureBase = CreateSignatureBase(providerUrl, callbackUrl, publicApiKey, nonce, timeStamp);
            var signatureHash = CreateSignatureHash(publicApiKey, secretApiKey);

            // Hash the values.
            var computedHash = signatureHash.ComputeHash(new ASCIIEncoding().GetBytes(signatureBase));
            var signatureString = Convert.ToBase64String(computedHash);

            return signatureString;
        }

        private static string CreateSignatureBase(string providerUrl,
            string callbackUrl,
            string publicApiKey,
            string nonce,
            string timeStamp)
        {
            var oauthParams = CreateOAuthParameters(callbackUrl, publicApiKey, nonce, timeStamp);
            var encodedParameters = EncodeOAuthParameters(oauthParams, "&");
            
            // NOTE: Second time we're encoding .. which (yes) means we've double enoded the main value, per key.
            //       Them the rules...
            return string.Format("POST&{0}&{1}",
                Uri.EscapeDataString(providerUrl),
                Uri.EscapeDataString(encodedParameters));
        }

        private static SortedDictionary<string, string> CreateOAuthParameters(string callbackUrl,
            string publicApiKey,
            string nonce,
            string timeStamp,
            string token = null)
        {
            var dictionary = new SortedDictionary<string, string>
            {
                {"oauth_callback", callbackUrl},
                {"oauth_version", "1.0"},
                {"oauth_consumer_key", publicApiKey},
                {"oauth_nonce", nonce},
                {"oauth_signature_method", "HMAC-SHA1"},
                {"oauth_timestamp", timeStamp}
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                dictionary.Add("oauth_token", token);
            }

            return dictionary;
        }

        private static string EncodeOAuthParameters(SortedDictionary<string, string> oauthParams,
            string delimeter)
        {
            if (oauthParams == null ||
                !oauthParams.Any())
            {
                throw new ArgumentNullException("oauthParams");
            }

            var result = new StringBuilder();
            foreach (var keyValue in oauthParams)
            {
                if (result.Length > 0)
                {
                    result.Append(delimeter);
                }
                // NOTE: First (of two places) where we have to encode the value.
                result.AppendFormat("{0}={1}", keyValue.Key, Uri.EscapeDataString((keyValue.Value)));
            }

            return result.ToString();
        }

        private static HashAlgorithm CreateSignatureHash(string publicApiKey, string secretApiKey)
        {
            // Generation the signature key the hash will use.
            string signatureKey = string.Format("{0}&{1}",
                Uri.EscapeDataString(publicApiKey),
                Uri.EscapeDataString(secretApiKey));

            return new HMACSHA1(new ASCIIEncoding().GetBytes(signatureKey));
        }

        protected async Task<HttpResponseMessage> GetResponseForPostAsync(string requestUrl,
            string callbackUrl,
            string token = null,
            HttpContent postBody = null)
        {
            using (var client = HttpClientFactory.GetHttpClient())
            {
                var oAuth10AuthenticationHeader = CreateAuthorizationHeader(requestUrl,
                    callbackUrl,
                    token);
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(oAuth10AuthenticationHeader);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", oAuth10AuthenticationHeader);

                TraceSource.TraceVerbose("Retrieving response from endpoint: {0}", requestUrl);
                return await client.PostAsync(requestUrl, postBody);
            }
        }
    }
}
