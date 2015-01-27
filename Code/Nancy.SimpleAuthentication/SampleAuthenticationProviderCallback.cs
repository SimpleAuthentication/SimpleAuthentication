using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;

namespace Nancy.SimpleAuthentication
{
    /// <summary>
    /// A simple implimentation of the handling the custom callback from the Authentication Provider or for hanlding an error.
    /// </summary>
    /// <remarks>IMPORTANT: This class is **NOT** intended to be used in production, but as a basis for quick prototyping and testing stuff.</remarks>
    public class SampleAuthenticationProviderCallback : IAuthenticationProviderCallback
    {
        private const string Notice =
            "You are using the SampleSimpleAuthenticationProviderCallback class that comes with the Nancy.SimpleAuthentication library. This class is generally auto setup with Nancy and is here to provide you with a quick example of what this entire library can do -> just returns the results as Json. It's probably not very helpful for production, though. The recommendation is to create your own class which inherits from IAuthenticationProviderCallback and impliment your own logic OR inherit from this class and impliment your own logic for the methods you decide to override.";

        #region IAuthenticationProviderCallback members

        /// <summary>
        /// A simple json output of the authenticated response.
        /// </summary>
        /// <param name="module">The current module.</param>
        /// <param name="result">Some data related to the callback, such as the authenticated user data.</param>
        /// <returns>A json representation of the authenticated user's information.</returns>
        public virtual dynamic Process(INancyModule module, AuthenticateCallbackResult result)
        {
            var model = new
            {
                Notice,
                AuthenticateCallbackResult = result
            };

            return module.Response.AsJson(model);
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
            var errorModel = new
            {
                message = exception.Message,
                source = exception.Source,
                stackTrace = exception.StackTrace,
                innerException = exception.InnerException != null
            };

            var model = new
            {
                Notice,
                Error = errorModel
            };

            return module.Response.AsJson(model, (HttpStatusCode) exception.HttpStatusCode);
        }

        #endregion
    }
}