using System;
using System.Collections.Generic;

namespace SimpleAuthentication.Core
{
    public class ProviderParams
    {
        public ProviderParams(string publicApiKey, 
            string secretApiKey, 
            ICollection<string> scopes = null)
        {
            if (string.IsNullOrWhiteSpace(publicApiKey))
            {
                throw new ArgumentNullException("publicApiKey");
            }

            if (string.IsNullOrWhiteSpace(secretApiKey))
            {
                throw new ArgumentNullException("secretApiKey");
            }

            PublicApiKey = publicApiKey;
            SecretApiKey = secretApiKey;

            // Optional.
            Scopes = scopes;
        }

        /// <summary>
        /// Public provider key. Sometimes known as the Consumer Key.
        /// </summary>
        public string PublicApiKey { get; private set; }

        /// <summary>
        /// Secret provider key. Sometimes known as the Consumer Secret.
        /// </summary>
        public string SecretApiKey { get; private set; }

        /// <summary>
        /// Optional collection of scope items.
        /// </summary>
        /// <remarks>This would be set if you wish to provide your own scope, instead of relying on the detault scope settings.</remarks>
        /// <value>The provider scope value.</value>
        public ICollection<string> Scopes { get; set; }
    }
}