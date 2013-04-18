using System;

namespace WorldDomination.Web.Authentication
{
    public interface ILoggingService
    {
        void Debug(string message);
        void Info(string message);
        void Warn(string message);
        void Error(string message, Exception exception = null);
        void Fatal(string message, Exception exception = null);
    }
}