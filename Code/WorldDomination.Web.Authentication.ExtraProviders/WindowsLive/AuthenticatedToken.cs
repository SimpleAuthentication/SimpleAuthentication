namespace WorldDomination.Web.Authentication.ExtraProviders.WindowsLive
{
    public class AuthenticatedToken
    {
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string scope { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string authentication_token { get; set; }
    }
}