using System;
using System.Text;

namespace WorldDomination.Web.Authentication.Csrf
{
    public class AntiForgery : IAntiForgery
    {
        private const string Delimeter = "|";

        public string DefaultCookieName { get { return "__WorldDomination.Web.Authentication.Mvc.CsrfToken"; } }

        public TokenPair CreateToken(string extraData = null)
        {
            // Create the state.
            var toSend = Guid.NewGuid().ToString();
            var toKeep = toSend;

            // Base64 Encode any extra data.
            if (extraData != null)
            {
                // We're hardocding the delimeter. So, I'm base64 encoding the extra data first, 
                // in case the delimeter might exist in the extra data!
                var encodedExtraValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(extraData));
                toKeep = string.Format("{0}{1}{2}", toKeep, Delimeter, encodedExtraValue);
            }

            return new TokenPair(toSend, toKeep);
        }

        public string ValidateToken(string keptToken, string recievedToken)
        {
            if (string.IsNullOrEmpty(keptToken))
            {
                throw new ArgumentNullException("keptToken");    
            }

            if (string.IsNullOrEmpty(recievedToken))
             {
                throw new ArgumentNullException("recievedToken");
             }
            

            // Do we have any extra data?
            string state = keptToken;
            string extraData = null;
            if (keptToken.Contains(Delimeter))
            {
                // Yep.
                var delimeterIndex = keptToken.IndexOf(Delimeter, StringComparison.Ordinal);

                // Grab the extra data part (and bas64 decode it)
                var dataString = keptToken.Substring(delimeterIndex + 1);
                if (!string.IsNullOrEmpty(dataString))
                {
                    extraData = Encoding.UTF8.GetString(Convert.FromBase64String(dataString));
                }
                state = keptToken.Substring(0, delimeterIndex);
            }

            // Validate the state
            if (!String.Equals(recievedToken, state, StringComparison.Ordinal))
            {
                throw new AuthenticationException("CSRF token does not match!");
             }
 
            return extraData;
        }
    }
}