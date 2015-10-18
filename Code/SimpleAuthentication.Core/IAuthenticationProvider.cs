using System;
using System.Collections.Specialized;
using SimpleAuthentication.Core.Tracing;

namespace SimpleAuthentication.Core
{
    /// <summary>
    /// Defines the contract that an Authentication Provider must impliemnt.
    /// </summary>
    public interface IAuthenticationProvider
    {
        /// <summary>
        /// Provider name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// What type of authentication is this? OAuth? Custom? 
        /// </summary>
        string AuthenticationType { get; }

        /// <summary>
        /// (Optional) Authentication resource/endpoint we should redirect to.
        /// </summary>
        Uri AuthenticateRedirectionUrl { get; set; }

        /// <summary>
        /// Access token.
        /// </summary>
        AccessToken AccessToken { get; set; }

        /// <summary>
        /// (Optional) TraceManager for displaying trace information.
        /// </summary>
        ITraceManager TraceManager { set; }

        /// <summary>
        /// Uri to redirect to the Authentication Provider with all querystring parameters defined.
        /// </summary>
        /// <param name="requestUrl">The current request url. This is used to generate the return uri.</param>
        /// <returns>The redirection details, like the Uri and any Access Token or State data we might need to persist between roundtrips.</returns>
        RedirectToAuthenticateSettings RedirectToAuthenticate(Uri requestUrl);

        /// <summary>
        /// Retrieve the user information from the Authentication Provider, now that we have authenticated.
        /// </summary>
        /// <param name="queryStringParameters">QueryString parameters from the callback.</param>
        /// <param name="state">The (deserialized) state from before we did the redirect to the provider.</param>
        /// <param name="callbackUri">The callback endpoint used for for quthenticating.</param>
        /// <returns>An authenticatedClient with either user information or some error message(s).</returns>
        IAuthenticatedClient AuthenticateClient(NameValueCollection queryStringParameters,
                                                string state,
                                                Uri callbackUri);
    }
}