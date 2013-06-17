namespace WorldDomination.Web.Authentication.ExtraProviders.GitHub
{
    public class GitHubAuthenticationServiceSettings : BaseAuthenticationServiceSettings
    {
        private string scope = "user:email";

        public GitHubAuthenticationServiceSettings() : base("github")
        {
        }

        public string Scope
        {
            get { return scope; }
            set { scope = value; } 
        }
        
    }
}