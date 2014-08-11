using System;

namespace SimpleAuthentication.Core
{
    public class AccessToken
    {
        /// <summary>
        /// An access token.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// When the access token expires. Always in UTC.
        /// </summary>
        public DateTime ExpiresOn { get; set; }

        public override string ToString()
        {
            return string.Format("Token: {0}. Expires On: {1}.",
                string.IsNullOrEmpty(Token)
                    ? "--no token--"
                    : Token,
                ExpiresOn);
        }
    }
}