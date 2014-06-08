namespace Nancy.SimpleAuthentication
{
    /// <summary>
    /// Defines the contract for the callback from an Authentication Provider.
    /// </summary>
    public interface IAuthenticationCallbackProvider
    {
        /// <summary>
        /// Custom processing during the callback from an Authentication Provider.
        /// </summary>
        /// <param name="nancyModule">The current module.</param>
        /// <param name="model">Some data related to the callback, such as the authenticated user data (if available) or an error.</param>
        /// <returns>What do we do once we've authenticated? Redirect somewhere? A view? a status code?</returns>
        dynamic Process(NancyModule nancyModule, AuthenticateCallbackData model);

        /// <summary>
        /// If an error occurs during the authentication process, this is where you can handle it.
        /// </summary>
        /// <remarks>A good example of overriding this method would be to send the user to a custom view to explain that something bad happened.</remarks>
        /// <param name="nancyModule">The current module.</param>
        /// <param name="errorMessage">The error that occured.</param>
        /// <returns></returns>
        dynamic OnRedirectToAuthenticationProviderError(NancyModule nancyModule, string errorMessage);
    }
}