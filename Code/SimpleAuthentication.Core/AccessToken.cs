using System;

namespace SimpleAuthentication.Core
{
    public class AccessToken
    {
        /// <summary>
        /// An access token.
        /// </summary>
        public string PublicToken { get; set; }

        /// <summary>
        /// The secret access token.
        /// </summary>
        /// <remarks>Some of the providers aren't using this.. er .. ?</remarks>
        public string SecretToken { get; set; }

        /// <summary>
        /// When the access token expires. Always in UTC.
        /// </summary>
        public DateTime ExpiresOn { get; set; }

        public override string ToString()
        {
            return string.Format("Public Token: {0}. Private Token: {1}. Expires On: {2}.",
                                 string.IsNullOrEmpty(PublicToken) ? "--no public token--" : PublicToken,
                                 string.IsNullOrEmpty(SecretToken) ? "--no secret token--" : SecretToken,
                                 ExpiresOn);
        }
    }
}