using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace WorldDomination.Web.Authentication
{
    /// <summary>
    ///     Defines the contract that an authentication service must impliment.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        ///     Returns a list of Unique Providers that are currently configured
        /// </summary>
        IDictionary<string, IAuthenticationProvider> AuthenticationProviders { get; }

        /// <summary>
        ///     Registering a provider with this service.
        /// </summary>
        /// <param name="provider">An Authentication Provider.</param>
        /// <param name="replaceExisting">true by default, specifies if it should replace existing configured provider</param>
        void AddProvider(IAuthenticationProvider provider, bool replaceExisting = true);

        /// <summary>
        ///     Registering a multiple providers with this service.
        /// </summary>
        /// <param name="providers">An collection of Authentication Providers.</param>
        /// <param name="replaceExisting">true by default, specifies if it should replace existing configured provider</param>
        void AddProviders(IEnumerable<IAuthenticationProvider> providers, bool replaceExisting = true);

        /// <summary>
        ///     Remove a configured provider from this service.
        /// </summary>
        /// <param name="providerName">the provider name (case insensitive)</param>
        void RemoveProvider(string providerName);

        /// <summary>
        ///     Determine the uri which is used to redirect to a given Provider.
        /// </summary>
        /// <param name="providerKey">A Provider keyname.</param>
        /// <param name="callBackUri">The uri to call back to, after the Authentication Provider has completed it's authentication process.</param>
        /// <returns>The uri to redirect to.</returns>
        Uri RedirectToAuthenticationProvider(string providerKey, Uri callBackUri = null);

        /// <summary>
        ///     Determine the uri which is used to redirect to a given Provider.
        /// </summary>
        /// <param name="authenticationServiceSettings">Specific authentication service settings to be passed along to the Authentication Provider.</param>
        /// <returns>The uri to redirect to.</returns>
        Uri RedirectToAuthenticationProvider(IAuthenticationServiceSettings authenticationServiceSettings);

        /// <summary>
        ///     Retrieve the user information from the Authentication Provider.
        /// </summary>
        /// <param name="authenticationServiceSettings">Specific authentication service settings to be passed along to the Authentication Provider.</param>
        /// <param name="queryStringParameters">QueryString parameters from the callback uri.</param>
        /// <returns>An authenticatedClient with either user information or some error message(s).</returns>
        IAuthenticatedClient GetAuthenticatedClient(IAuthenticationServiceSettings authenticationServiceSettings,
                                                    NameValueCollection queryStringParameters);

        /// <summary>
        ///     Retrieve the user information from the Authentication Provider.
        /// </summary>
        /// <param name="authenticationServiceSettings">Specific authentication service settings to be passed along to the Authentication Provider.</param>
        /// <param name="requestParameters">QueryString parameters from the callback uri (Used by NancyFX).</param>
        /// <returns>An authenticatedClient with either user information or some error message(s).</returns>
        IAuthenticatedClient GetAuthenticatedClient(IAuthenticationServiceSettings authenticationServiceSettings,
                                                    dynamic requestParameters);

        /// <summary>
        ///     Retrieves the settings for an authentication service.
        /// </summary>
        /// <param name="providerKey">A Provider keyname.</param>
        /// <param name="requestUrl"></param>
        /// /// <param name="path"></param>
        /// <returns>The authentication service settings.</returns>
        IAuthenticationServiceSettings GetAuthenticateServiceSettings(string providerKey,
                                                                      Uri requestUrl,
                                                                      string path = "/authentication/authenticatecallback");
    }
}