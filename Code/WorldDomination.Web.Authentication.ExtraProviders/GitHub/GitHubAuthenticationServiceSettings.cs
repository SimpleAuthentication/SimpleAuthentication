namespace WorldDomination.Web.Authentication.ExtraProviders.GitHub
{
    public class GitHubAuthenticationServiceSettings : BaseAuthenticationServiceSettings
    {
        public GitHubAuthenticationServiceSettings() : base("github")
        {
            Scope = "user:email";
        }

        public string Scope { get; set; }
    }
}