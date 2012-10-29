namespace WorldDomination.Web.Authentication.Facebook
{
    public class FacebookAuthenticationServiceSettings : BaseAuthenticationServiceSettings
    {
        public FacebookAuthenticationServiceSettings() : base("Facebook")
        {
        }

        public bool IsMobile { get; set; }
        public string Display { get; set; }
    }
}