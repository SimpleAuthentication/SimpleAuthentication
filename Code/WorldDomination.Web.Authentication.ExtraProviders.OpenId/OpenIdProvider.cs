using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using RestSharp;

namespace WorldDomination.Web.Authentication.ExtraProviders.OpenId
{
    public class OpenIdProvider : IAuthenticationProvider
    {
        private const string XrdsHeaderKey = "X-XRDS-Location";
        private readonly IRestClientFactory _restClientFactory;

        public OpenIdProvider(CustomProviderParams providerParams)
        {
            _restClientFactory = providerParams.RestClientFactory ?? new RestClientFactory();
        }

        public OpenIdProvider(IRestClientFactory restClientFactory = null)
        {
            _restClientFactory = restClientFactory;
        }

        public string Name
        {
            get { return "OpenId"; }
        }

        public Uri CallBackUri { get; private set; }

        public IAuthenticationServiceSettings DefaultAuthenticationServiceSettings
        {
            get { return new OpenIdAuthenticationServiceSettings(); }
        }

        public Uri RedirectToAuthenticate(IAuthenticationServiceSettings authenticationServiceSettings)
        {
            IOpenIdAuthenticationServiceSettings settings;
            if ((settings = authenticationServiceSettings as IOpenIdAuthenticationServiceSettings) == null)
            {
                throw new ArgumentNullException("authenticationServiceSettings");
            }

            // First we need to do a YADIS Discover, so we can get the real endpoint.
            var xrdsEndPoint = YadisDiscoverXrdsEndPoint(settings.Identifier);

            if (xrdsEndPoint == null ||
                string.IsNullOrEmpty(xrdsEndPoint.AbsoluteUri))
            {
                // We don't know where to go :(
                return null;
            }

            // //eg: openid.claimed_id=<identifier>%2F&openid.identity=<server>%2F&openid.assoc_handle=%7BHMAC-SHA256%7D%7B5109e243%7D%7BBMe45Q%3D%3D%7D&openid.return_to=http%3A%2F%2Flocalhost%3A6969%2FOpenId%2FAuthenticateCallback%3FproviderKey%3Dopenid%26dnoa.userSuppliedIdentifier%3Dhttp%253A%252F%252Fbendornis.com%252F&openid.realm=http%3A%2F%2Flocalhost%3A6969%2F&openid.mode=checkid_setup&openid.ns=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0&openid.ns.sreg=http%3A%2F%2Fopenid.net%2Fextensions%2Fsreg%2F1.1&openid.sreg.required=&openid.sreg.optional=email%2Cfullname%2Cgender%2Ccountry%2Clanguage&no_ssl=true

            // If we have an endpoint, lets query that!
            var openIdEndPoint = YadisDiscoverOpenIdEndPoint(xrdsEndPoint);

            if (openIdEndPoint == null)
            {
                return null;
            }

            const string claimedId = "openid.claimed_id=http://specs.openid.net/auth/2.0/identifier_select";
            const string identifier = "openid.identity=http://specs.openid.net/auth/2.0/identifier_select";
            var returnTo = "openid.return_to=" + authenticationServiceSettings.CallBackUri.AbsoluteUri;
            var realm = "openid.realm=" + authenticationServiceSettings.CallBackUri.AbsoluteUri;
            const string mode = "openid.mode=checkid_setup";
            const string openidNamespace = "openid.ns=http://specs.openid.net/auth/2.0";
            const string namespaceSReg = "openid.ns.sreg=http://openid.net/extensions/sreg/1.1";
            const string sRegRequird = "openid.sreg.required=";
            const string sRegOptional = "openid.sreg.optional=email,fullname,gender,country,language";
            const string noSsl = "no_ssl=true";
            var x = string.Format("{0}?{1}&{2}&{3}&{4}&{5}&{6}&{7}&{8}&{9}&{10}",
                                  openIdEndPoint.AbsoluteUri,
                                  claimedId,
                                  identifier,
                                  returnTo,
                                  realm,
                                  mode,
                                  openidNamespace,
                                  namespaceSReg,
                                  sRegRequird,
                                  sRegOptional,
                                  noSsl);

            return new Uri(x);
        }

        public IAuthenticatedClient AuthenticateClient(NameValueCollection parameters, string existingState)
        {
            throw new NotImplementedException();
        }

        protected virtual Uri Identifier(IOpenIdAuthenticationServiceSettings settings)
        {
            return settings.Identifier;
        }

        private Uri YadisDiscoverXrdsEndPoint(Uri identifier)
        {
            if (identifier == null ||
                string.IsNullOrEmpty(identifier.AbsoluteUri))
            {
                throw new ArgumentNullException("identifier");
            }

            // Try and retrieve an XRDS.
            IRestResponse restResponse;
            try
            {
                var restRequest = new RestRequest(Method.GET);
                var restClientFactory = _restClientFactory ?? new RestClientFactory();
                var restClient = restClientFactory.CreateRestClient(identifier.AbsoluteUri);
                restResponse = restClient.Execute(restRequest);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException(
                    "Error occured while trying to determine Xrds endpoint for identity [" + identifier.AbsoluteUri +
                    "].", exception);
            }

            if (restResponse == null)
            {
                throw new AuthenticationException(
                    "No response was created while trying to determine the Xrds endpoint for identity [" +
                    identifier.AbsoluteUri + "].");
            }

            // If we have a 301 or 302, lets recurse.
            if (restResponse.StatusCode == HttpStatusCode.MovedPermanently ||
                restResponse.StatusCode == HttpStatusCode.Redirect)
            {
                // We need to move to a new location. But where?
                var newLocation = restResponse.Headers.SingleOrDefault(x => x.Name == "Location");
                return newLocation == null
                           ? null // No idea where to go, so we can't continue.
                           : YadisDiscoverXrdsEndPoint(new Uri((string) newLocation.Value));
            }

            // Lets check the header to see if we can score the XDRS location, now.
            var endpoint = restResponse.Headers.SingleOrDefault(x => x.Name == XrdsHeaderKey);
            return endpoint == null
                       ? null
                       : new Uri((string) endpoint.Value);
        }

        private Uri YadisDiscoverOpenIdEndPoint(Uri xrdsUri)
        {
            if (xrdsUri == null ||
                string.IsNullOrEmpty(xrdsUri.AbsoluteUri))
            {
                throw new ArgumentNullException("xrdsUri");
            }

            IRestResponse restResponse;
            try
            {
                var restRequest = new RestRequest(Method.GET);
                var restClientFactory = _restClientFactory ?? new RestClientFactory();
                var restClient = restClientFactory.CreateRestClient(xrdsUri.AbsoluteUri);
                restResponse = restClient.Execute(restRequest);
            }
            catch (Exception exception)
            {
                throw new AuthenticationException(
                    "Error occured while trying to determine OpenId endpoint for identity [" + xrdsUri.AbsoluteUri +
                    "].", exception);
            }

            if (restResponse == null)
            {
                throw new AuthenticationException(
                    "No response was created while trying to determine the OpenId endpoint for identity [" +
                    xrdsUri.AbsoluteUri + "].");
            }

            var content = restResponse.Content;
            if (string.IsNullOrEmpty(content))
            {
                throw new AuthenticationException("Retrieved an Xrds document but there was no content!");
            }

            // Find the first URI element in .. wait for it.. wait for it .. some XML! ffs.
            return ParseXrdsDocument(content);
        }

        private Uri ParseXrdsDocument(string xmlContent)
        {
            if (string.IsNullOrEmpty(xmlContent))
            {
                throw new ArgumentNullException("xmlContent");
            }

            // Find the first URI element.
            var xDocument = XDocument.Parse(xmlContent);


            /* Xrds Namespace in the Xml doc.
               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
               xmlns:xrds="xri://$xrds"
               xmlns:ux="http://specs.openid.net/extensions/ux/1.0"
               xmlns="xri://$xrd*($v*2.0)">
            */
            XNamespace ns = "xri://$xrd*($v*2.0)";

            // Find the first URI element.
            var uris = xDocument.Descendants(ns + "URI").ToList();
            return uris.Count <= 0 ? null : new Uri(uris.First().Value);
        }
    }
}