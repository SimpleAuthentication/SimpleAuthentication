﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using RestSharp;
using WorldDomination.Web.Authentication.ExtraProviders.OpenId;

namespace WorldDomination.Web.Authentication.ExtraProviders
{
    public class OpenIdProvider : IAuthenticationProvider
    {
        private const string XrdsHeaderKey = "X-XRDS-Location";
        private readonly IRestClientFactory _restClientFactory;
        private static readonly IDictionary<string, Uri> YadisXrdsEndPointUris = new Dictionary<string, Uri>();
        private static readonly IDictionary<string, Uri> YadisOpenIdEndPointUris = new Dictionary<string, Uri>();

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
            var settings = authenticationServiceSettings as IOpenIdAuthenticationServiceSettings;

            if (settings == null)
            {
                throw new ArgumentException("authenticationServiceSettings is null or not of type IOpenIdAuthenticationServiceSettings", "authenticationServiceSettings");
            }

            // First we need to do a YADIS Discover, so we can get the real endpoint.
            var xrdsEndPoint = YadisDiscoverXrdsEndPoint(settings.Identifier);

            if (xrdsEndPoint == null || string.IsNullOrEmpty(xrdsEndPoint.AbsoluteUri))
            {
                // We don't know where to go :(
                return null;
            }

            // If we have an endpoint, lets query that!
            var openIdEndPoint = YadisDiscoverOpenIdEndPoint(xrdsEndPoint);

            if (openIdEndPoint == null)
            {
                return null;
            }

            var urlParts = new[]
            {
                "openid.claimed_id=http://specs.openid.net/auth/2.0/identifier_select",
                "openid.identity=http://specs.openid.net/auth/2.0/identifier_select",
                "openid.mode=checkid_setup",
                "openid.ns=http://specs.openid.net/auth/2.0",
                "openid.ns.sreg=http://openid.net/extensions/sreg/1.1",
                "openid.sreg.required=nickname",
                "openid.sreg.optional=email,fullname,gender,language",
                "no_ssl=true",
                "openid.return_to=" + authenticationServiceSettings.CallBackUri.AbsoluteUri,
                "openid.realm=" + authenticationServiceSettings.CallBackUri.AbsoluteUri
            };

            var url = string.Concat(openIdEndPoint.AbsoluteUri, "?", string.Join("&", urlParts));

            return new Uri(url);
        }

        public IAuthenticatedClient AuthenticateClient(NameValueCollection parameters, string existingState)
        {
            /*
             *  Sample Query String results - Failure
               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                providerkey:openid
                openid.mode:cancel
                openid.ns:http://specs.openid.net/auth/2.0
              
               Sample Query String results - Success
               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                providerkey:openid
                openid.assoc_handle:{HMAC-SHA1}{511b67b9}{DpzYWQ==}
                openid.claimed_id:http://username.myopenid.com/
                openid.identity:http://username.myopenid.com/
                openid.mode:id_res
                openid.ns:http://specs.openid.net/auth/2.0
                openid.ns.sreg:http://openid.net/extensions/sreg/1.1
                openid.op_endpoint:http://www.myopenid.com/server
                openid.response_nonce:2013-02-13T10:15:21ZhAJdyG
                openid.return_to:http://localhost:7000/authentication/authenticatecallback?providerkey=openid
                openid.sig:1+pvowBKpRFQFoxIVx7KDGDsGSg=
                openid.signed:assoc_handle,claimed_id,identity,mode,ns,ns.sreg,op_endpoint,response_nonce,return_to,signed,sreg.country,sreg.email,sreg.fullname,sreg.gender,sreg.language
                openid.sreg.email:someuser@blah.blah.com
                openid.sreg.fullname:FirstName Surname
                openid.sreg.gender:F
                openid.sreg.language:EN
             */
            if (parameters == null || !parameters.AllKeys.Any(x => x.StartsWith("openid.")))
            {
                throw new ArgumentException("No openid.XXX Query String paramters found.");
            }

            // Check if this is a cancel.
            var mode = parameters.AllKeys.SingleOrDefault(x => x == "openid.mode");

            if (string.IsNullOrEmpty(mode) || parameters[mode] == "cancel")
            {
                throw new AuthenticationException(
                    "OpenId provider returned a Cancel state. No user information was (therefore) provided.");
            }

            return new AuthenticatedClient(Name.ToLowerInvariant())
            {
                UserInformation = RetrieveMe(parameters)
            };
        }

        protected virtual Uri Identifier(IOpenIdAuthenticationServiceSettings settings)
        {
            return settings.Identifier;
        }

        private Uri YadisDiscoverXrdsEndPoint(Uri identifier)
        {
            if (identifier == null || string.IsNullOrEmpty(identifier.AbsoluteUri))
            {
                throw new ArgumentNullException("identifier");
            }

            // Have we cached this?
            if (YadisXrdsEndPointUris.ContainsKey(identifier.AbsoluteUri))
            {
                return YadisXrdsEndPointUris[identifier.AbsoluteUri];
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
                    "Error occured while trying to determine Xrds endpoint for identity [" + identifier.AbsoluteUri + "].", exception);
            }

            if (restResponse == null)
            {
                throw new AuthenticationException(
                    "No response was created while trying to determine the Xrds endpoint for identity [" + identifier.AbsoluteUri + "].");
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
            var endPointUri = endpoint == null
                                  ? null
                                  : new Uri((string) endpoint.Value);
            
            // Cache this result :)
            YadisXrdsEndPointUris.Add(identifier.AbsoluteUri, endPointUri);

            return endPointUri;
        }

        private Uri YadisDiscoverOpenIdEndPoint(Uri xrdsUri)
        {
            if (xrdsUri == null ||
                string.IsNullOrEmpty(xrdsUri.AbsoluteUri))
            {
                throw new ArgumentNullException("xrdsUri");
            }

            // Is this already cached?
            if (YadisOpenIdEndPointUris.ContainsKey(xrdsUri.AbsoluteUri))
            {
                return YadisOpenIdEndPointUris[xrdsUri.AbsoluteUri];
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
                    "Error occured while trying to determine OpenId endpoint for identity [" + xrdsUri.AbsoluteUri + "].", exception);
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
            var openIdUri = ParseXrdsDocument(content);

            YadisOpenIdEndPointUris.Add(xrdsUri.AbsoluteUri, openIdUri);

            return openIdUri;
        }

        private static Uri ParseXrdsDocument(string xmlContent)
        {
            if (string.IsNullOrEmpty(xmlContent))
            {
                throw new ArgumentNullException("xmlContent");
            }

            // Find the first URI element.
            var xDocument = XDocument.Parse(xmlContent);

            /* 
               Xrds Namespace in the Xml doc.
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

        private static UserInformation RetrieveMe(NameValueCollection parameters)
        {
            // SIMPLE REGISTRATION Extension reference (ie. what user data can come back): http://openid.net/specs/openid-simple-registration-extension-1_0.html#response_format

            //openid.claimed_id:http://username.myopenid.com/
            //openid.sreg.email:someuser@blah.blah.com
            //openid.sreg.fullname:FirstName Surname
            //openid.sreg.gender:F
            //openid.sreg.language:EN

            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            var genderCharacter = parameters["openid.sreg.gender"];
            GenderType gender;

            switch (genderCharacter)
            {
                case "F":
                    gender = GenderType.Female;
                    break;
                case "M":
                    gender = GenderType.Male;
                    break;
                default:
                    gender = GenderType.Unknown;
                    break;
            }

            return new UserInformation
            {
                Email = parameters["openid.sreg.email"],
                Gender = gender,
                Id = parameters["openid.claimed_id"],
                Locale = parameters["openid.sreg.language"],
                Name = parameters["openid.sreg.fullname"],
                UserName = parameters["openid.sreg.nickname"]
            };
        }
    }
}