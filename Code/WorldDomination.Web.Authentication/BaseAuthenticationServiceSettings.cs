using System;
using CuttingEdge.Conditions;

namespace WorldDomination.Web.Authentication
{
    public abstract class BaseAuthenticationServiceSettings : IAuthenticationServiceSettings
    {
        protected BaseAuthenticationServiceSettings(string providerKey)
        {
            Condition.Requires(providerKey).IsNotNullOrEmpty();

            ProviderKey = providerKey;
        }

        #region Implementation of IAuthenticationServiceSettings

        public string ProviderKey { get; private set; }

        public ProviderType ProviderType
        {
            get
            {
                if (string.IsNullOrEmpty(ProviderKey))
                {
                    return ProviderType.Unknown;
                }

                ProviderType providerType;
                return Enum.TryParse(ProviderKey, out providerType) ? providerType : ProviderType.Other;
            }
        }

        #endregion
    }
}