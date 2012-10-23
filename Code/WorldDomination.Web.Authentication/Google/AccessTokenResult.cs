namespace WorldDomination.Web.Authentication.Google
{
    public class AccessTokenResult
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string IdToken { get; set; }
    }
}