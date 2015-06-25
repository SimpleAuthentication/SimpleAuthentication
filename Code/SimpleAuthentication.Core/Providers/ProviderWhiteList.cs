using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SimpleAuthentication.Core.Providers
{
    public class ProviderWhiteList : IProviderWhiteList
    {
        public ICollection<Type> DefaultProviders
        {
            get
            {
                return new Collection<Type>
                {
                    typeof (FacebookProvider),
                    typeof (GoogleProvider),
                    typeof (TwitterProvider),
                    typeof (WindowsLiveProvider)
                };
            }
        }

        public ICollection<Type> ProvidersToAllow { get; set; }
    }
}