namespace WorldDomination.Web.Authentication.Providers.Facebook
{
    public class FacebookAuthenticationServiceSettings : BaseAuthenticationServiceSettings
    {
        public FacebookAuthenticationServiceSettings(bool isFakeProvider = false) 
            : base(isFakeProvider ? "FakeFacebook" : "Facebook")
        {
            Display = DisplayType.Unknown;
            IsMobile = false;
        }

        public bool IsMobile { get; set; }
        public DisplayType Display { get; set; }
    }
}