namespace WorldDomination.Web.Authentication.Csrf
{
    public class TokenPair
    {
        /// <summary>
        /// Gets the token to send along with the request.
        /// </summary>
        public string ToSend { get; private set; }

        /// <summary>
        /// Gets the token to keep on the client (in a Cookie, for example)
        /// </summary>
        public string ToKeep { get; private set; }

        public TokenPair(string toSend, string toKeep)
        {
            ToSend = toSend;
            ToKeep = toKeep;
        }
    }
}