namespace SimpleAuthentication.Core.Providers.Twitter
{
    public class VerifierResult
    {
        public string OAuthToken { get; set; }
        public string OAuthVerifier { get; set; }
    }
}