namespace WorldDomination.Web.Authentication.Providers
{
    public class BaseProvider
    {
        public IRestClientFactory RestClientFactory;

        public BaseProvider()
        {
            RestClientFactory = new RestClientFactory();
        }
    }
}