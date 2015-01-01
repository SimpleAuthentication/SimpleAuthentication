using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleAuthentication.Core.Exceptions;

namespace SimpleAuthentication.Core
{
    public static class SystemHelpers
    {
        public static IDictionary<string, string> ConvertKeyValueContentToDictionary(string content,
            bool unescapeKeysAndValues = false,
            char delimeter = '&')
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException("content");
            }

            var parameters = content.Split(new[] {delimeter});

            return (from p in parameters
                let kv = p.Split(new[] {'='})
                where kv.Length == 2
                select kv)
                .ToDictionary(keyValue => unescapeKeysAndValues
                    ? Uri.UnescapeDataString(keyValue[0])
                    : keyValue[0],
                    keyValue => unescapeKeysAndValues
                        ? Uri.UnescapeDataString(keyValue[1])
                        : keyValue[1]);
        }

        public static Uri CreateUri(Uri sourceUrl,
            IDictionary<string, string> querystringParameters)
        {
            if (sourceUrl == null)
            {
                throw new ArgumentNullException("sourceUrl");
            }

            var result = new UriBuilder(sourceUrl);

            if (querystringParameters == null ||
                !querystringParameters.Any())
            {
                return new Uri(result.ToString());    
            }

            // HERE: We have some extra query string params so we need to join them
            //       into the existing query string -or- make them the querystring.

            // REF: http://msdn.microsoft.com/en-us/library/system.uribuilder.query(v=vs.110).aspx
            // NOTE: first character is the '?'. So we need to skip it.
            var existingQuery = string.IsNullOrWhiteSpace(result.Query)
                ? null
                : result.Query.Substring(1);

            if (string.IsNullOrWhiteSpace(existingQuery))
            {
                // No existing query - so just 
                result.Query = JoinQueryStringParameters(querystringParameters);
            }
            else
            {
                // We have some existing query, so we need to figure out if there's any 
                var keyValues = ConvertKeyValueContentToDictionary(existingQuery, true);
                if (keyValues == null ||
                    !keyValues.Any())
                {
                    var errorMessage =
                        String.Format(
                            "Tried to convert the Query string '{0}' to a KeyValue collection, but it's either null or contains no items. We expected at least one key/value item.",
                            String.IsNullOrWhiteSpace(existingQuery));
                    throw new Exception(errorMessage);
                }

                // NOTE: If we have an existing key, we need to ovewrite the value.
                foreach (var parameter in querystringParameters)
                {
                    var key = Uri.EscapeDataString(parameter.Key);
                    if (keyValues.ContainsKey(key))
                    {
                        keyValues[key] = parameter.Value;
                    }
                    else
                    {
                        // Key doesn't exist, so we need to add it in.
                        keyValues.Add(parameter);
                    }
                }

                result.Query = JoinQueryStringParameters(keyValues);
            }

            return new Uri(result.ToString());
        }

        public static void CrossSiteRequestForgeryCheck(IDictionary<string, string> querystring,
            string state,
            string stateKey)
        {
            // NOTE: queryString can be null or empty.
            //       Means we didn't have any (which is not good - because we require some stuff
            //       but it's possible that there is none because a person hit the route directly, etc)

            if (string.IsNullOrWhiteSpace(stateKey))
            {
                throw new ArgumentNullException("stateKey");
            }

            // Start with the Cross Site Request Forgery check.
            if (querystring == null ||
                !querystring.ContainsKey(stateKey))
            {
                var errorMessage = string.Format(
                    "The callback querystring doesn't include a state key/value parameter. We need one of these so we can do a CSRF check. Please check why the request url from the provider is missing the parameter: '{0}'. eg. &{0}=something...",
                    stateKey);
                //TraceSource.TraceError(errorMessage);
                throw new AuthenticationException(errorMessage);
            }

            var callbackState = querystring[stateKey];
            if (!callbackState.Equals(state, StringComparison.InvariantCultureIgnoreCase))
            {
                // Replace all remembered state characters with stars. This is so a hacker can't probe the server for the rememebered value.
                var hashedState = "";
                for (int i = 0; i < state.Length; i++)
                {
                    hashedState += "*";
                }

                var errorMessage =
                    string.Format(
                        "CSRF check fails: The callback '{0}' value '{1}' doesn't match the server's *remembered* state value '{2}'.",
                        stateKey,
                        callbackState,
                        hashedState);
                throw new AuthenticationException(errorMessage);
            }
        }

        public static string JoinQueryStringParameters(IDictionary<string, string> querystringParameters)
        {
            if (querystringParameters == null)
            {
                throw new ArgumentNullException();
            }

            if (!querystringParameters.Any())
            {
                throw new ArgumentOutOfRangeException();
            }

            var parametersToAppend = querystringParameters
                .Select(x => String.Format("{0}={1}",
                    Uri.EscapeDataString(x.Key),
                    Uri.EscapeDataString(x.Value)))
                .ToArray();

            return String.Join("&", parametersToAppend);
        }
    }
}