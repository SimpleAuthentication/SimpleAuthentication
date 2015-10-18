using System;
using Nancy;
using Nancy.SimpleAuthentication;
using SimpleAuthentication.Sample.NancyAuto.Models;

namespace SimpleAuthentication.Sample.NancyAuto.Helpers
{
    public class SampleAuthenticationCallbackProvider : IAuthenticationCallbackProvider
    {
        public dynamic Process(NancyModule nancyModule, AuthenticateCallbackResult result)
        {
            var model = new AuthenticationViewModel
            {
                AuthenticatedClient = result.AuthenticatedClient,
                ReturnUrl = result.ReturnUrl
            };
            return nancyModule.View["AuthenticateCallback", model];
        }

        public dynamic OnRedirectToAuthenticationProviderError(NancyModule nancyModule, Exception exception)
        {
            var model = new AuthenticationViewModel
            {
                ErrorMessage = exception.Message
            };

            return nancyModule.View["AuthenticateCallback", model];
        }
    }
}