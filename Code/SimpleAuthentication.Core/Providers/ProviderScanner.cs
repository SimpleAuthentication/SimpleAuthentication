using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleAuthentication.Core.Providers
{
    public class ProviderScanner : IProviderScanner
    {
        private readonly IList<Type> _additionalTypes;

        public ProviderScanner()
        {
        }

        public ProviderScanner(IList<Type> additionalTypes = null)
        {
            // Optional.
            _additionalTypes = additionalTypes;
        }

        public static IList<Type> DefaultProviders
        {
            get
            {
                return new List<Type>
                {
                    typeof (GoogleProvider),
                    typeof (FacebookProvider),
                    typeof (TwitterProvider),
                    typeof (WindowsLiveProvider),
                    typeof (FakeProvider)
                };
            }
        }

        public IList<Type> GetDiscoveredProviders()
        {
            var types = (List<Type>)DefaultProviders;
            if (_additionalTypes != null &&
                _additionalTypes.Any())
            {
                types.AddRange(_additionalTypes);
            }

            return types;
        }
    }
}