using System.Net.Http;

namespace SimpleAuthentication.Core
{
    public static class HttpClientFactory
    {
        public static HttpMessageHandler MessageHandler { get; set; }

        public static HttpClient GetHttpClient()
        {
            return MessageHandler == null
                ? new HttpClient()
                : new HttpClient(MessageHandler);
        }
    }
}