namespace WorldDomination.Web.Authentication.Twitter
{
    public class TwitterClient : IAuthenticatedClient
    {
        public string DeniedToken { get; set; }
        public string OAuthToken { get; set; }
        public string OAuthTokenSecret { get; set; }
        public string OAuthVerifier { get; set; }
        public UserInformation UserInformation { get; set; }
    }
}