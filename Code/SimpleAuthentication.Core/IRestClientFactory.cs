using RestSharp;

namespace SimpleAuthentication.Core
{
    public interface IRestClientFactory
    {
        IRestClient CreateRestClient(string baseUrl);
    }
}