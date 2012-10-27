namespace WorldDomination.Web.Authentication.Facebook
{
    public class FacebookAuthenticationSettings : BaseAuthenticationServiceSettings
    {
        public FacebookAuthenticationSettings() : base("Facebook")
        {
        }

        public bool IsMobile { get; set; }
        public string Display { get; set; }
    }
}