using System;
using System.Collections.Generic;
using RestSharp;

namespace WorldDomination.Web.Authentication
{
    public class RestClientFactory : IRestClientFactory
    {
        private readonly IDictionary<string, IRestClient> _restClientDictionary;

        public RestClientFactory()
        {
        }

        public RestClientFactory(IRestClient restClient)
        {
            if (restClient == null)
            {
                throw new ArgumentNullException("restClient");
            }

            _restClientDictionary = new Dictionary<string, IRestClient>
                                    {
                                        {
                                            restClient.BaseUrl.ToLowerInvariant(),
                                            restClient
                                        }
                                    };
        }

        public RestClientFactory(ICollection<IRestClient> restClients)
        {
            if (restClients == null ||
                restClients.Count <= 0)
            {
                throw new ArgumentNullException("restClients");
            }

            _restClientDictionary = new Dictionary<string, IRestClient>();
            foreach (var restClient in restClients)
            {
                _restClientDictionary.Add(restClient.BaseUrl.ToLowerInvariant(), restClient);
            }
        }

        public IRestClient CreateRestClient(string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ArgumentNullException("baseUrl");
            }

            // Safety conversion: Convert before we try to use it.
            baseUrl = baseUrl.ToLowerInvariant();

            // Use the provided one (which in effect, ignores the provided baseUrl) 
            // OR new up a new instance.
            IRestClient existingRestClient = null;
            if (_restClientDictionary != null &&
                _restClientDictionary.ContainsKey(baseUrl))
            {
                existingRestClient = _restClientDictionary[baseUrl];
            }
            return existingRestClient ?? new RestClient(baseUrl);
        }
    }
}