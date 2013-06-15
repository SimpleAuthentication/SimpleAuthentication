namespace WorldDomination.Web.Authentication.Providers.Twitter
{
    public class TwitterAuthenticationServiceSettings : BaseAuthenticationServiceSettings
    {
        public TwitterAuthenticationServiceSettings(bool isFakeProvider = false) 
            : base(isFakeProvider ? "FakeTwitter" : "Twitter")
        {
        }
    }
}