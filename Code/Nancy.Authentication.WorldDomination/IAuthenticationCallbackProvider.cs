namespace Nancy.Authentication.WorldDomination
{
    /// <summary>
    /// Defines the contract for the callback from an Authentication Provider.
    /// </summary>
    public interface IAuthenticationCallbackProvider
    {
        /// <summary>
        /// Custom processing during the callback from an Authentication Provider.
        /// </summary>
        /// <param name="nancyContext">The current context.</param>
        /// <param name="model">Some data related to the callback, such as an error or some authenticated user data.</param>
        /// <returns>The result from the callback Process.</returns>
        ProcessResult Process(NancyContext nancyContext, AuthenticateCallbackData model);
    }
}