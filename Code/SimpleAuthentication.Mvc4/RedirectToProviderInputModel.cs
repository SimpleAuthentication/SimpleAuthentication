namespace SimpleAuthentication.Mvc
{
    public class RedirectToProviderInputModel
    {
        public string ProviderName { get; set; }
        public string Identifier { get; set; }
        public string ReturnUrl { get; set; }
    }
}