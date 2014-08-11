namespace SimpleAuthentication.Core.Providers.Twitter
{
    internal class CallbackResult
    {
        public string Token { get; set; }
        public string Verifier { get; set; }
    }
}