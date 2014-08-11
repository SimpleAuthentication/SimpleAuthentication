using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public static Uri CreateCallBackUri(string providerKey,
            Uri requestUrl,
            string path)
        {
            if (string.IsNullOrWhiteSpace(providerKey))
            {
                throw new ArgumentNullException("providerKey");
            }

            if (requestUrl == null)
            {
                throw new ArgumentNullException("requestUrl");
            }

            if (string.IsNullOrWhiteSpace(path))
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

        public static IDictionary<string, string> ConvertKeyValueContentToDictionary(string content,
            char delimeter = '&')
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException("content");
            }

            var parameters = content.Split(new[] { delimeter });
            return parameters
                .Select(parameter => parameter.Split(new[] { '=' }))
                .ToDictionary(keyValue => keyValue[0], keyValue => keyValue[1]);
        }
    }
}