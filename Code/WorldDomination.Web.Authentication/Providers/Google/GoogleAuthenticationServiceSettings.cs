namespace WorldDomination.Web.Authentication.Providers.Google
{
    public class GoogleAuthenticationServiceSettings : BaseAuthenticationServiceSettings
    {
        public GoogleAuthenticationServiceSettings(bool isFakeProvider = false)
            : base(isFakeProvider ? "FakeGoogle" : "Google")
        {
        }
    }
}