using System;

namespace WorldDomination.Web.Authentication.ExtraProviders.OpenId
{
    public interface IOpenIdAuthenticationServiceSettings : IAuthenticationServiceSettings
    {
        Uri Identifier { get; set; }
    }
}