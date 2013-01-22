using System;
using CuttingEdge.Conditions;
using WorldDomination.Web.Authentication.Facebook;
using WorldDomination.Web.Authentication.Google;
using WorldDomination.Web.Authentication.Twitter;

namespace WorldDomination.Web.Authentication
{
    public static class AuthenticationServiceSettingsFactory
    {
        public static IAuthenticationServiceSettings GetAuthenticateServiceSettings(string providerKey)
        {
            Condition.Requires(providerKey).IsNotNullOrEmpty();

            // Convert the string to an enumeration.
            ProviderType providerType;
            if (!Enum.TryParse(providerKey, true, out providerType))
            {
                return null;
            }

            switch (providerType)
            {
                case ProviderType.Facebook:
                    return new FacebookAuthenticationServiceSettings();
                case ProviderType.Google:
                    return new GoogleAuthenticationServiceSettings();
                case ProviderType.Twitter:
                    return new TwitterAuthenticationServiceSettings();
                default:
                    throw new AuthenticationException(
                        "Unhandled provider type while trying to determine which AuthenticationServiceSettings to instanciate.");
            }
        }
    }
}