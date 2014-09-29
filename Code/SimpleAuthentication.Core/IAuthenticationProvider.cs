using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using SimpleAuthentication.Core.Tracing;

namespace SimpleAuthentication.Core
{
    /// <summary>
    /// Defines the contract that an Authentication Provider must impliemnt.
    /// </summary>
    public interface IAuthenticationProvider
    {
        /// <summary>
        /// Name of this provider.
        /// </summary>
        /// <example>Eg. Google or Fake or Custom Forms Auth, etc.</example>
        string Name { get; }

        /// <summary>
        /// What type of authentication is this?
        /// </summary>
        /// <example>OAuth 2.0 or Custom Forms Authentication.</example>
        string Description { get; }

        /// <summary>
        /// Determine the url (and all querystring params) we require to kick off our authentication process with the external Authentication Provider.
        /// </summary>
        /// <param name="callbackUrl">The current request url. This is used to generate the return url.</param>
        /// <returns>The redirection details, like the Uri and any Access Token or State data we might need to persist between roundtrips.</returns>
        RedirectToAuthenticateSettings GetRedirectToAuthenticateSettings(Uri callbackUrl);

        /// <summary>
        /// Retrieve the user information from the Authentication Provider, now that we have authenticated.
        /// </summary>
        /// <param name="queryString">QueryString parameters from the callback.</param>
        /// <param name="state">The (deserialized) state from before we did the redirect to the provider.</param>
        /// <param name="callbackUrl">OAuth 2.0 Only: The callback endpoint used for for authenticating.</param>
        /// <returns>An authenticatedClient with either user information or some error message(s).</returns>
        Task<IAuthenticatedClient> AuthenticateClientAsync(IDictionary<string, string> queryString,
            string state,
            Uri callbackUrl);
    }
}