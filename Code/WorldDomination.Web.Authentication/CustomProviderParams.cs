using System;

namespace WorldDomination.Web.Authentication
{
    public class ProviderParams
    {
        public string Key { get; set; }
        public string Secret { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Key))
            {
                throw new ArgumentException(Key);
            }

            if (string.IsNullOrEmpty(Secret))
            {
                throw new ArgumentException(Secret);
            }
        }
    }
}