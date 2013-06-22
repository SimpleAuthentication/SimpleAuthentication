using System;

namespace WorldDomination.Web.Authentication
{
    public class ProviderParams
    {
        public string Key { get; set; }
        public string Secret { get; set; }

        /// <summary>
        /// Comma separated string of scopes
        /// </summary>
        /// <value>The provider scope value.</value>
        public string Scope { private get; set; }

        public string[] GetScopes ()
        {
            if (string.IsNullOrWhiteSpace (Scope)) 
            {
                return new string[]{};
            }

            return Scope.Split (',');
        }

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