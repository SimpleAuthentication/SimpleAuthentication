using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;

namespace Nancy.SimpleAuthentication
{
    /// <summary>
    /// Defines the contract for the callback from an Authentication Provider.
    /// </summary>
    public interface IAuthenticationProviderCallback
    {
        /// <summary>
        /// Custom processing during the callback from an Authentication Provider.
        /// </summary>
        /// <param name="module">The current module.</param>
        /// <param name="result">Some data related to the callback, such as the authenticated user data.</param>
        /// <returns>What do we do once we've authenticated? Redirect somewhere? A view? a status code?</returns>
        dynamic Process(INancyModule module, AuthenticateCallbackResult result);

        /// <summary>
        /// If an error occurs during the authentication process, this is where you can handle it.
        /// </summary>
        /// <remarks>An example of a valid error would be if the user cancels their permission check or the user doesn't have the correct scope permissions or an access token has been revoked.</remarks>
        /// <param name="module">The current module.</param>
        /// <param name="errorType">Which part of the pipeline did this error occur in.</param>
        /// <param name="exception">The exception error that occured.</param>
        /// <returns></returns>
        dynamic OnError(INancyModule module,
            ErrorType errorType,
            AuthenticationException exception);
    }
}