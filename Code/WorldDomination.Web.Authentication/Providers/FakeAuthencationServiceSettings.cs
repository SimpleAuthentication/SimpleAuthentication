namespace WorldDomination.Web.Authentication.Providers
{
    public class FakeAuthencationServiceSettings : BaseAuthenticationServiceSettings
    {
        public FakeAuthencationServiceSettings(string providerKey) : base(providerKey)
        {
            State = "This is some fake state";
        }
    }
}