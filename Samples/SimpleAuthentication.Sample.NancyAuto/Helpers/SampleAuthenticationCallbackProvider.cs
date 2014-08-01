using System;
using Nancy;
using Nancy.SimpleAuthentication;
using SimpleAuthentication.Sample.NancyAuto.Models;

namespace SimpleAuthentication.Sample.NancyAuto.Helpers
{
    public class SampleAuthenticationCallbackProvider : IAuthenticationCallbackProvider
    {
        public dynamic Process(NancyModule nancyModule, AuthenticateCallbackData model)
        {
            return nancyModule.View["AuthenticateCallback", model];
        }

        public dynamic OnRedirectToAuthenticationProviderError(NancyModule nancyModule, Exception exception)
        {
            var model = new IndexViewModel
            {
                ErrorMessage = exception.Message
            };

            return nancyModule.View["index", model];
        }
    }
}