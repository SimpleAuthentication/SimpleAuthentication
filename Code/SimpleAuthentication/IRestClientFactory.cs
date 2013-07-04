using RestSharp;

namespace SimpleAuthentication
{
    public interface IRestClientFactory
    {
        IRestClient CreateRestClient(string baseUrl);
    }
}