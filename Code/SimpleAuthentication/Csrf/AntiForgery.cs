using System;
using System.Text;

namespace WorldDomination.Web.Authentication.Csrf
{
    public class AntiForgery : IAntiForgery
    {
        private const string Delimeter = "|";

        public virtual string DefaultCookieName
        {
            get { return "__WorldDomination.Web.Authentication.Mvc.CsrfToken"; }
        }

        public virtual string ValidateToken(string keptToken, string recievedToken)
        {
            if (string.IsNullOrEmpty(keptToken))
            {
                throw new ArgumentNullException("keptToken");
            }

            if (string.IsNullOrEmpty(recievedToken))
            {
                throw new ArgumentNullException("recievedToken");
            }

            string pureToken;
            string tokenWithExtraData;
            SelectTokens(keptToken, recievedToken, out pureToken, out tokenWithExtraData);


            // Do we have any extra data?
            string extraData;
            string state;
            ParseToken(tokenWithExtraData, out state, out extraData);

            // Validate the state
            CheckToken(pureToken, state);

            return extraData;
        }

        public virtual TokenPair CreateToken(string existingToKeepToken, string extraData = null)
        {
            // Create the state.
            string toSend;
            string toKeep;
            GenerateTokens(existingToKeepToken, out toSend, out toKeep);

            // Base64 Encode any extra data.
            if (extraData != null)
            {
                EncodeExtraData(extraData, ref toKeep, ref toSend);
            }

            return new TokenPair(toSend, toKeep);
        }

        protected virtual void CheckToken(string pureToken, string state)
        {
            if (!String.Equals(pureToken, state, StringComparison.Ordinal))
            {
                throw new AuthenticationException("CSRF token does not match!");
            }
        }

        protected virtual void SelectTokens(string keptToken, string recievedToken, out string pureToken,
                                            out string tokenWithExtraData)
        {
            pureToken = recievedToken;
            tokenWithExtraData = keptToken;
        }

        protected virtual void EncodeExtraData(string extraData, ref string toKeep, ref string toSend)
        {
            toKeep = GenerateExtraData(extraData, toKeep);
        }

        protected virtual string GenerateExtraData(string extraData, string token)
        {
            // We're hardocding the delimeter. So, I'm base64 encoding the extra data first, 
            // in case the delimeter might exist in the extra data!
            var encodedExtraValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(extraData));
            return string.Format("{0}{1}{2}", token, Delimeter, encodedExtraValue);
        }

        protected virtual void GenerateTokens(string existingToKeepToken, out string toSend, out string toKeep)
        {
            toSend = Guid.NewGuid().ToString();
            toKeep = toSend;
        }

        protected virtual void ParseToken(string tokenWithExtraData, out string state, out string extraData)
        {
            state = tokenWithExtraData;
            extraData = null;

            if (tokenWithExtraData.Contains(Delimeter))
            {
                // Yep.
                var delimeterIndex = tokenWithExtraData.IndexOf(Delimeter, StringComparison.Ordinal);

                // Grab the extra data part (and bas64 decode it)
                var dataString = tokenWithExtraData.Substring(delimeterIndex + 1);
                if (!string.IsNullOrEmpty(dataString))
                {
                    extraData = Encoding.UTF8.GetString(Convert.FromBase64String(dataString));
                }
                state = tokenWithExtraData.Substring(0, delimeterIndex);
            }
        }
    }
}