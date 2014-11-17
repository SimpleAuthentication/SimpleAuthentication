using System;
using System.Net;

namespace SimpleAuthentication.Core.Exceptions
{
    /// <summary>
    /// An exception that is thrown when some authentication error occurs.
    /// </summary>
    public class AuthenticationException : Exception
    {
        public HttpStatusCode HttpStatusCode { get; private set; }

        /// <summary>
        /// Initializes a new instance of an AuthenticationException.
        /// </summary>
        /// <param name="message">An error message.</param>
        public AuthenticationException(string message) : base(message)
        {
            HttpStatusCode = HttpStatusCode.InternalServerError;
        }

        /// <summary>
        /// Initializes a new instance of an AuthenticationException.
        /// </summary>
        /// <param name="message">An error message.</param>
        /// <param name="innerException">Optional: an inner exception of the real error.</param>
        /// <param name="errorStatusCode">Optional: the status code of this error.</param>
        /// <remarks>Sometimes an error occurs with an associated error message. Eg. Tried to retrieve some data from the Authentication Provider but some bad credentials were provided so a 403 FORBIDDEN was returned.</remarks>
        public AuthenticationException(string message, 
            Exception innerException,
            HttpStatusCode errorStatusCode = HttpStatusCode.InternalServerError) : base(message, innerException)
        {
            HttpStatusCode = errorStatusCode;
        }
    }
}