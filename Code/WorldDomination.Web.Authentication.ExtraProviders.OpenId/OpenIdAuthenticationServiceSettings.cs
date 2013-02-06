namespace WorldDomination.Web.Authentication.ExtraProviders.OpenId
{
    internal class OpenIdAuthenticationServiceSettings : BaseAuthenticationServiceSettings, IOpenIdAuthenticationServiceSettings
    {
        public OpenIdAuthenticationServiceSettings() : base("openid")
        {
        }

        public string Identifier { get; set; }
    }
}