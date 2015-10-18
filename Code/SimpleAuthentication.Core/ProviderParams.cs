using System;
using System.Collections.Generic;

namespace SimpleAuthentication.Core
{
    public class ProviderParams
    {
        /// <summary>
        /// Public provider key.
        /// </summary>
        public string PublicApiKey { get; set; }

        /// <summary>
        /// Secret provider key.
        /// </summary>
        public string SecretApiKey { get; set; }

        /// <summary>
        /// Optional collection of scope items.
        /// </summary>
        /// <remarks>This would be set if you wish to provide your own scope, instead of relying on the detault scope settings.</remarks>
        /// <value>The provider scope value.</value>
        public ICollection<string> Scopes { get; set; }

        public void Validate ()
        {
            if (string.IsNullOrEmpty (PublicApiKey)) 
            {
                throw new ArgumentException(PublicApiKey);
            }

            if (string.IsNullOrEmpty(SecretApiKey)) 
            {
                throw new ArgumentException(SecretApiKey);
            }
        }
    }
}