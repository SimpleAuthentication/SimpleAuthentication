using System;
using RestSharp;

namespace WorldDomination.Web.Authentication
{
    public class RestClientFactory : IRestClientFactory
    {
        private readonly IRestClient _restClient;

        public RestClientFactory()
        {
        }

        public RestClientFactory(IRestClient restClient)
        {
            if (restClient == null)
            {
                throw new ArgumentNullException("restClient");
            }

            _restClient = restClient;
        }

        public IRestClient CreateRestClient(string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ArgumentNullException("baseUrl");
            }

            // Use the provided one (which in effect, ignores the provided baseUrl) 
            // OR new up a new instance.
            return _restClient ?? new RestClient(baseUrl);
        }
    }
}