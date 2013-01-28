using RestSharp;

namespace WorldDomination.Web.Authentication
{
    public interface IRestClientFactory
    {
        IRestClient CreateRestClient(string baseUrl);
    }
}