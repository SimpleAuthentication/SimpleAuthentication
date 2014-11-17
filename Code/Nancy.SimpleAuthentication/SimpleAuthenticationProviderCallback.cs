using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;

namespace Nancy.SimpleAuthentication
{
    /// <summary>
    /// A simple implimentation of the handling the custom callback from the Authentication Provider or for hanlding an error.
    /// </summary>
    /// <remarks>IMPORTANT: This class is **NOT** intended to be used in production, but as a basis for quick prototyping and testing stuff.</remarks>
    public class SimpleAuthenticationProviderCallback : IAuthenticationProviderCallback
    {
        #region IAuthenticationProviderCallback members

        /// <summary>
        /// A simple json output of the authenticated response.
        /// </summary>
        /// <param name="module">The current module.</param>
        /// <param name="result">Some data related to the callback, such as the authenticated user data.</param>
        /// <returns>A json representation of the authenticated user's information.</returns>
        public virtual dynamic Process(INancyModule module, AuthenticateCallbackResult result)
        {
            return module.Response.AsJson(result);
        }

        /// <summary>
        /// A simple json out of any error that occured during the authentication process.
        /// </summary>
        /// <remarks>An example of a valid error would be if the user cancels their permission check or the user doesn't have the correct scope permissions or an access token has been revoked.</remarks>
        /// <param name="module">The current module.</param>
        /// <param name="errorType">Which part of the pipeline did this error occur in.</param>
        /// <param name="exception">The exception error that occured.</param>
        public virtual dynamic OnError(INancyModule module, ErrorType errorType, AuthenticationException exception)
        {
            // NOTE: for any inner exceptions, we're not caring about recursing through and grabbing 
            //       that info. this is just for a quick and dirty output.
            var errorMessage = new
            {
                message = exception.Message,
                source = exception.Source,
                stackTrace = exception.StackTrace,
                innerException = exception.InnerException != null
            };
            return module.Response.AsJson(errorMessage, (HttpStatusCode)exception.HttpStatusCode);
        }

        #endregion
    }
}