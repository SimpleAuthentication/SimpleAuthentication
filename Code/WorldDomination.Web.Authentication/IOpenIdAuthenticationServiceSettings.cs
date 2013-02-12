using System;

namespace WorldDomination.Web.Authentication
{
    public interface IOpenIdAuthenticationServiceSettings : IAuthenticationServiceSettings
    {
        Uri Identifier { get; set; }
    }
}