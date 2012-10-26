namespace WorldDomination.Web.Authentication.Facebook
{
    public class FacebookAuthenticationSettings : BaseAuthenticationServiceSettings
    {
        public FacebookAuthenticationSettings(string providerKey) : base(providerKey)
        {
        }

        public bool IsMobile { get; set; }
        public string Display { get; set; }
    }
}