namespace SimpleAuthentication.ExtraProviders.ArcGISOnline
{
    public class AccessTokenResult
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public string Username { get; set; }
    }
}
