using System;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using WorldDomination.Web.Authentication.Tracing;

namespace WorldDomination.Web.Authentication
{
    /// <summary>
    /// Defines the contract that an Authentication Provider must impliemnt.
    /// </summary>
    [InheritedExport]
    public interface IAuthenticationProvider
    {
        /// <summary>
        /// Provider name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The default authentication service settings for a Provider.
        /// </summary>
        IAuthenticationServiceSettings DefaultAuthenticationServiceSettings { get; }

        /// <summary>
        /// Uri to redirect to the Authentication Provider with all querystring parameters defined.
        /// </summary>
        /// <param name="authenticationServiceSettings">Specific authentication service settings to be passed along to the Authentication Provider.</param>
        /// <returns>The uri to redirect to.</returns>
        Uri RedirectToAuthenticate(IAuthenticationServiceSettings authenticationServiceSettings);

        /// <summary>
        /// Retrieve the user information from the Authentication Provider, now that we have authenticated.
        /// </summary>
        /// <param name="queryStringParameters">QueryString parameters from the callback.</param>
        /// <param name="authenticationServiceSettings">Specific authentication service settings to be passed along to the Authentication Provider.</param>
        /// <param name="restClientFactory"></param>
        /// <returns>An authenticatedClient with either user information or some error message(s).</returns>
        IAuthenticatedClient AuthenticateClient(IAuthenticationServiceSettings authenticationServiceSettings, NameValueCollection queryStringParameters);

        /// <summary>
        /// (Optional) TraceManager for getting trace information.
        /// </summary>
        ITraceManager TraceManager { set; }
    }
}