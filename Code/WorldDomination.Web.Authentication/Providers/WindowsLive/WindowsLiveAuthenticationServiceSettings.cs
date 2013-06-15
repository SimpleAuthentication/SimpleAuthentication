namespace WorldDomination.Web.Authentication.Providers.WindowsLive
{
    public class WindowsLiveAuthenticationServiceSettings : BaseAuthenticationServiceSettings
    {
        public WindowsLiveAuthenticationServiceSettings(bool isFakeProvider = false)
            : base(isFakeProvider ? "FakeWindowsLive" : "WindowsLive")
        {
        }
    }
}