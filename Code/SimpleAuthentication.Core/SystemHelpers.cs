using System;
using System.Text;

namespace SimpleAuthentication.Core
{
    public static class SystemHelpers
    {
        public static string RecursiveErrorMessages(this Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            var errorMessages = new StringBuilder();

            // Keep grabbing any error messages while we have some inner exception.
            Exception nextException = exception;
            while (nextException != null)
            {
                if (errorMessages.Length > 0)
                {
                    errorMessages.Append(" ");
                }
                // Append this error message.
                errorMessages.AppendFormat(nextException.Message);

                // Grab the next error message.
                nextException = nextException.InnerException;
            }

            return errorMessages.Length > 0 ? errorMessages.ToString() : null;
        }

        public static Uri CreateCallBackUri(string providerKey, Uri requestUrl,
                                            string path = "/authentication/authenticatecallback")
        {
            if (string.IsNullOrEmpty(providerKey))
            {
                throw new ArgumentNullException("providerKey");
            }

            if (requestUrl == null)
            {
                throw new ArgumentNullException("requestUrl");
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            var builder = new UriBuilder(requestUrl)
            {
                Path = path,
                Query = "providerkey=" + providerKey.ToLowerInvariant()
            };

            // Don't include port 80/443 in the Uri.
            if (builder.Uri.IsDefaultPort)
            {
                builder.Port = -1;
            }

            return builder.Uri;
        }

        public static Uri CreateRoute(Uri requestUrl, string path, string query = null)
        {
            if (requestUrl == null)
            {
                throw new ArgumentNullException("requestUrl");
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            var builder = new UriBuilder(requestUrl)
            {
                Path = path,
                Query = query
            };

            // Don't include port 80/443 in the Uri.
            if (builder.Uri.IsDefaultPort)
            {
                builder.Port = -1;
            }

            return builder.Uri;
        }
    }
}