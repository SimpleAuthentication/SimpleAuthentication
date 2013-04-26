namespace WorldDomination.Web.Authentication.Providers
{
    public class BaseProvider
    {
        protected IRestClientFactory RestClientFactory;

        public BaseProvider()
        {
            RestClientFactory = new RestClientFactory();
        }
    }
}