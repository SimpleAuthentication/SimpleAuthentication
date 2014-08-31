using System;
using System.Threading.Tasks;
using Nancy;
using Nancy.SimpleAuthentication;
using SimpleAuthentication.Sample.NancyAuto.Models;

namespace SimpleAuthentication.Sample.NancyAuto.Helpers
{
    public class SampleAuthenticationCallbackProvider : IAuthenticationCallbackProvider
    {
        public async Task<dynamic> ProcessAsync(NancyModule nancyModule, AuthenticateCallbackResult result)
        {
            var model = new AuthenticationViewModel
            {
                AuthenticatedClient = result.AuthenticatedClient,
                ReturnUrl = result.ReturnUrl
            };
            return await Task.FromResult(nancyModule.View["AuthenticateCallback", model]);
        }

        public dynamic OnRedirectToAuthenticationProviderError(NancyModule nancyModule, 
            Exception exception)
        {
            var model = new AuthenticationViewModel
            {
                ErrorMessage = exception.Message
            };

            return nancyModule.View["AuthenticateCallback", model];
        }
    }
}