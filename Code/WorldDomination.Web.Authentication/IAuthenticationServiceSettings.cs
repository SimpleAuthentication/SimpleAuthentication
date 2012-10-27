namespace WorldDomination.Web.Authentication
{
    public interface IAuthenticationServiceSettings
    {
        string ProviderKey { get; }
        ProviderType ProviderType { get; }
        string State { get; set; }
    }
}