using System;

namespace WorldDomination.Web.Authentication
{
    public class ErrorInformation
    {
        public ErrorInformation() : this(null)
        {
        }

        public ErrorInformation(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
}