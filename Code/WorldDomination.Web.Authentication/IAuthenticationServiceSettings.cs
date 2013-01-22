using System;

namespace WorldDomination.Web.Authentication
{
    public interface IAuthenticationServiceSettings
    {
        string ProviderKey { get; }
        ProviderType ProviderType { get; }
        Uri CallBackUri { get; set; }
        string State { get; set; }
    }
}