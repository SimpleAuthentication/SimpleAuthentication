namespace WorldDomination.Web.Authentication
{
    public class CustomProviderParams
    {
        public string Key { get; set; }
        public string Secret { get; set; }
        public IRestClientFactory RestClientFactory { get; set; }
    }
}