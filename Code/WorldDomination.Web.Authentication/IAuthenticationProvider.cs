using System;
using System.Collections.Specialized;
using System.ComponentModel.Composition;

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
        /// Where to call back after the Provider has completed (either succesfully or not) the authentication process on their end.
        /// </summary>
        Uri CallBackUri { get; }

        /// <summary>
        /// The default authentication service settings for a Provider.
        /// </summary>
        IAuthenticationServiceSettings DefaultAuthenticationServiceSettings { get; }

        /// <summary>
        /// Retrieve the user information from the Authentication Provider, now that we have authenticated.
        /// </summary>
        /// <param name="parameters">QueryString parameters from the callback.</param>
        /// <param name="existingState">Any optional state value. (Can be null for no state checks)</param>
        /// <returns>An authenticatedClient with either user information or some error message(s).</returns>
        IAuthenticatedClient AuthenticateClient(NameValueCollection parameters, string existingState);
    }
}