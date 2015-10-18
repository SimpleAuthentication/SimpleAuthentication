using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleAuthentication.Tests
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpRequestException _exception;
        private readonly IDictionary<string, HttpResponseMessage> _responses;

        public FakeHttpMessageHandler(string requestUri, HttpResponseMessage httpResponseMessage)
            : this(new Dictionary<string, HttpResponseMessage>
            {
                {requestUri, httpResponseMessage}
            })
        {
        }

        public FakeHttpMessageHandler(IDictionary<string, HttpResponseMessage> httpResponseMessages)
        {
            if (httpResponseMessages == null)
            {
                throw new ArgumentNullException("httpResponseMessages");
            }

            if (!httpResponseMessages.Any())
            {
                throw new ArgumentOutOfRangeException("httpResponseMessages");
            }

            _responses = httpResponseMessages;
        }

        public FakeHttpMessageHandler(HttpResponseMessage httpResponseMessage)
            : this(new Dictionary<string, HttpResponseMessage>
            {
                {"*", httpResponseMessage}
            })
        {
        }

        public FakeHttpMessageHandler(HttpRequestException exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            _exception = exception;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (_exception != null)
            {
                throw _exception;
            }

            var tcs = new TaskCompletionSource<HttpResponseMessage>();

            HttpResponseMessage response;
            var requestUri = request.RequestUri.ToString();

            // Determine the Response message based upon the Request uri.
            // 1. Exact match.
            // 2. Wildcard '*' == I don't care what the Uri is, just use this Response.
            // 3. Starts with == this is so we don't have to have a huge string in our test case. Just keeping code a bit cleaner.
            if (!_responses.TryGetValue(requestUri, out response) &&
                !_responses.TryGetValue("*", out response))
            {
                foreach (var key in _responses.Keys.Where(requestUri.StartsWith))
                {
                    response = _responses[key];
                    break;
                }

                if (response == null)
                {
                    // Nope - no keys found exactly OR starting with...
                    var errorMessage =
                        string.Format(
                            "No HttpResponseMessage found for the Request Uri: {0}. Please provide one in the FakeHttpMessageHandler constructor Or use a '*' for any request uri.",
                            request.RequestUri);
                    throw new InvalidOperationException(errorMessage);
                }
            }

            tcs.SetResult(response);
            return tcs.Task;
        }

        /// <summary>
        /// Helper method to easily return a simple HttpResponseMessage.
        /// </summary>
        public static HttpResponseMessage GetStringHttpResponseMessage(string content,
            HttpStatusCode httpStatusCode = HttpStatusCode.OK,
            string mediaType = "application/json")
        {
            return new HttpResponseMessage
            {
                StatusCode = httpStatusCode,
                Content = new StringContent(content, Encoding.UTF8, mediaType)
            };
        }
    }
}