namespace WorldDomination.Web.Authentication.Csrf
{
    public interface IAntiForgery
    {
        string DefaultCookieName { get; }
        TokenPair CreateToken(string existingKeptToken, string extraData = null);
 
        /// <summary>
        /// Validates the token pair and returns the extra data
        /// </summary>
        /// <param name="keptToken"></param>
        /// <param name="recievedToken"></param>
        /// <returns></returns>
        string ValidateToken(string keptToken, string recievedToken);
    }
}