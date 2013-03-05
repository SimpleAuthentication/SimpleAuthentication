using System;
using System.Text;

namespace WorldDomination.Web.Authentication.Csrf
{
    public class AntiForgery : IAntiForgery
    {
        private const string Delimeter = "|";

        public string CreateToken(string extraData = null)
        {
            // Create the state.
            var state = Guid.NewGuid().ToString();

            // Base64 Encode any extra data.
            if (extraData != null)
            {
                // We're hardocding the delimeter. So, I'm base64 encoding the extra data first, 
                // in case the delimeter might exist in the extra data!
                var encodedExtraValue = Convert.ToBase64String(Encoding.Unicode.GetBytes(extraData));
                state = string.Format("{0}{1}{2}", state, Delimeter, encodedExtraValue);
            }

            return state;
        }

        public TokenData ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException("token");    
            }
            
            var tokenData = new TokenData
                            {
                                State = token
                            };

            // Do we have any extra data?
            if (token.Contains(Delimeter))
            {
                // Yep.
                var delimeterIndex = token.IndexOf(Delimeter, StringComparison.Ordinal);

                // Grab the State part.
                tokenData.State = token.Substring(0, delimeterIndex);

                // Grab the extra data part (and bas64 decode it)
                var extraData = token.Substring(delimeterIndex + 1);
                if (!string.IsNullOrEmpty(extraData))
                {
                    tokenData.ExtraData = Encoding.Unicode.GetString(Convert.FromBase64String(extraData));
                }
            }

            return tokenData;
        }
    }
}