using System;
using System.Collections.Generic;
using System.Linq;
using RestSharp;

namespace SimpleAuthentication.Core
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
                    string.IsNullOrEmpty(restClient.BaseUrl)
                        ? Guid.NewGuid().ToString()
                        : restClient.BaseUrl.ToLowerInvariant(),
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

            // If we have provided some restClients, use the one for the baseUrl.
            // Otherwise, just use the first one.
            // ASSUMPTION: We've manually provided at least one restClient, so we never want
            //             to new up a new RestClient(..) .. but use one of those provided ones.
            IRestClient existingRestClient = null;
            if (_restClientDictionary != null)
            {
                existingRestClient = _restClientDictionary.ContainsKey(baseUrl)
                                         ? _restClientDictionary[baseUrl]
                                         : _restClientDictionary.First().Value;
            }
            return existingRestClient ?? new RestClient(baseUrl)
            {
                UserAgent = "SimpleAuthentication"
            };
        }
    }
}