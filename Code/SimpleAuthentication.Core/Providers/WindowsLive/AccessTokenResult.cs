namespace SimpleAuthentication.Core.Providers.WindowsLive
{
    public class AccessTokenResult
    {
        public string TokenType { get; set; }
        public string ExpiresIn { get; set; }
        public string Scope { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string AuthenticationToken { get; set; }
    }
}