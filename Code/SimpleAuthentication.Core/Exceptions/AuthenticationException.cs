using System;

namespace SimpleAuthentication.Core.Exceptions
{
    /// <summary>
    /// An exception that is thrown when some authentication error occurs.
    /// </summary>
    public class AuthenticationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of an AuthenticationException.
        /// </summary>
        /// <param name="message">An error message.</param>
        public AuthenticationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of an AuthenticationException.
        /// </summary>
        /// <param name="message">An error message.</param>
        /// <param name="innerException">Optional: an inner exception of the real error.</param>
        public AuthenticationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}