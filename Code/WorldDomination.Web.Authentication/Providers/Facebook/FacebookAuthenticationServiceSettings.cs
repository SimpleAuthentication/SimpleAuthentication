namespace WorldDomination.Web.Authentication.Providers.Facebook
{
    public class FacebookAuthenticationServiceSettings : BaseAuthenticationServiceSettings
    {
        public FacebookAuthenticationServiceSettings() : base("Facebook")
        {
        }

        public bool IsMobile { get; set; }
        public DisplayType Display { get; set; }
    }
}