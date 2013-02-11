using System;

namespace WorldDomination.Web.Authentication
{
    public interface IOpenIdAuthenticationProvider : IAuthenticationProvider
    {
        /// <summary>
        /// Uri to redirect to the Authentication Provider with all querystring parameters defined.
        /// </summary>
        /// <param name="authenticationServiceSettings">Specific authentication service settings to be passed along to the Authentication Provider.</param>
        /// <param name="identifier">Unique identifier</param>
        /// <returns>The uri to redirect to.</returns>
        Uri RedirectToAuthenticate(IAuthenticationServiceSettings authenticationServiceSettings, string identifier);
    }
}