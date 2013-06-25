using System;
using System.Collections.Generic;

namespace WorldDomination.Web.Authentication
{
    public class ProviderParams
    {
        /// <summary>
        /// Public provider key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Secret provider key.
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// Optional collection of scope items.
        /// </summary>
        /// <remarks>This would be set if you wish to provide your own scope, instead of relying on the detault scope settings.</remarks>
        /// <value>The provider scope value.</value>
        public ICollection<string> Scope { get; set; }

        public void Validate ()
        {
            if (string.IsNullOrEmpty (Key)) 
            {
                throw new ArgumentException (Key);
            }

            if (string.IsNullOrEmpty (Secret)) 
            {
                throw new ArgumentException (Secret);
            }
        }
    }
}