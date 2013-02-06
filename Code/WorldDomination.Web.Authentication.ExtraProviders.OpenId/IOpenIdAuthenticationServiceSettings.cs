namespace WorldDomination.Web.Authentication.ExtraProviders.OpenId
{
    public interface IOpenIdAuthenticationServiceSettings : IAuthenticationServiceSettings
    {
        string Identifier { get; set; }
    }
}