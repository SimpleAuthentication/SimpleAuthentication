using System;

namespace WorldDomination.Web.Authentication
{
    public class LoggingWrapper
    {
        private readonly ILoggingService _loggingService;

        public LoggingWrapper(ILoggingService loggingService)
        {
            // Can be null.
            _loggingService = loggingService;
        }

        public void Debug(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException("message");
            }

            if (_loggingService != null)
            {
                _loggingService.Debug(message);
            }
        }

        public void Info(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException("message");
            }

            if (_loggingService != null)
            {
                _loggingService.Info(message);
            }
        }

        public void Warn(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException("message");
            }

            if (_loggingService != null)
            {
                _loggingService.Warn(message);
            }
        }

        public void Error(string message, Exception exception = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException("message");
            }

            if (_loggingService != null)
            {
                _loggingService.Error(message, exception);
            }
        }

        public void Fatal(string message, Exception exception = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException("message");
            }

            if (_loggingService != null)
            {
                _loggingService.Fatal(message, exception);
            }
        }
    }
}