using System;
using System.Text;

namespace WorldDomination.Web.Authentication
{
    public static class SystemHelpers
    {
        public static string RecursiveErrorMessages(this Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            var errorMessages = new StringBuilder();

            // Keep grabbing any error messages while we have some inner exception.
            Exception nextException = exception;
            while (nextException != null)
            {
                if (errorMessages.Length > 0)
                {
                    errorMessages.Append(" ");
                }
                // Append this error message.
                errorMessages.AppendFormat(nextException.Message);

                // Grab the next error message.
                nextException = nextException.InnerException;
            }

            return errorMessages.Length > 0 ? errorMessages.ToString() : null;
        }
    }
}