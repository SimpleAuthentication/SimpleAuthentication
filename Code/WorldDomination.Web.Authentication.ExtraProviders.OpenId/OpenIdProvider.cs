using System;
using System.Collections.Specialized;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.RelyingParty;

namespace WorldDomination.Web.Authentication.ExtraProviders.OpenId
{
    public class OpenIdProvider : IAuthenticationProvider
    {
        private static readonly OpenIdRelyingParty OpenIdRelyingParty =
            new OpenIdRelyingParty(new StandardRelyingPartyApplicationStore());

        public string Name
        {
            get { return "OpenId"; }
        }

        public Uri CallBackUri { get; private set; }
        public IAuthenticationServiceSettings DefaultAuthenticationServiceSettings { get; private set; }

        public Uri RedirectToAuthenticate(IAuthenticationServiceSettings authenticationServiceSettings)
        {
            IOpenIdAuthenticationServiceSettings settings;
            if ((settings = authenticationServiceSettings as IOpenIdAuthenticationServiceSettings) == null)
            {
                throw new ArgumentNullException("authenticationServiceSettings");
            }

            //local server
            string realm = "http://" + settings.CallBackUri.Authority;

            Identifier identifier;
            if (!DotNetOpenAuth.OpenId.Identifier.TryParse(Identifier(settings), out identifier))
            {
                throw new InvalidOperationException();
            }

            //create the openid request
            //and add request for personal information
            var request = OpenIdRelyingParty.CreateRequest(identifier, new Realm(realm), settings.CallBackUri);
            request.AddExtension(new ClaimsRequest
                                 {
                                     FullName = DemandLevel.Request,
                                     Language = DemandLevel.Request,
                                     Country = DemandLevel.Request,
                                     Email = DemandLevel.Request,
                                     Gender = DemandLevel.Request,
                                 });

            var response = request.RedirectingResponse;

            //grab the redirect from the headers of the response
            //eg: https://www.myopenid.com/server?openid.claimed_id=<identifier>%2F&openid.identity=<server>%2F&openid.assoc_handle=%7BHMAC-SHA256%7D%7B5109e243%7D%7BBMe45Q%3D%3D%7D&openid.return_to=http%3A%2F%2Flocalhost%3A6969%2FOpenId%2FAuthenticateCallback%3FproviderKey%3Dopenid%26dnoa.userSuppliedIdentifier%3Dhttp%253A%252F%252Fbendornis.com%252F&openid.realm=http%3A%2F%2Flocalhost%3A6969%2F&openid.mode=checkid_setup&openid.ns=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0&openid.ns.sreg=http%3A%2F%2Fopenid.net%2Fextensions%2Fsreg%2F1.1&openid.sreg.required=&openid.sreg.optional=email%2Cfullname%2Cgender%2Ccountry%2Clanguage&no_ssl=true
            return new Uri(response.Headers["Location"]);
        }

        public IAuthenticatedClient AuthenticateClient(NameValueCollection parameters, string existingState)
        {
            throw new NotImplementedException();
        }

        protected virtual string Identifier(IOpenIdAuthenticationServiceSettings settings)
        {
            return settings.Identifier;
        }
    }
}