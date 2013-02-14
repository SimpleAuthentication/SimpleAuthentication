using System;

namespace WorldDomination.Web.Authentication.ExtraProviders.OpenId
{
    public class OpenIdAuthenticationServiceSettings : BaseAuthenticationServiceSettings,
                                                       IOpenIdAuthenticationServiceSettings
    {
        public OpenIdAuthenticationServiceSettings() : base("openid")
        {
        }

        public Uri Identifier { get; set; }
    }
}