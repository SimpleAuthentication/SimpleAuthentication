namespace WorldDomination.Web.Authentication.WindowsLive
{
    public class AuthenticatedToken
    {
        public string TokenType { get; set; }
        public string ExpiresIn { get; set; }
        public string Scope { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string AuthenticationToken { get; set; }
    }
}