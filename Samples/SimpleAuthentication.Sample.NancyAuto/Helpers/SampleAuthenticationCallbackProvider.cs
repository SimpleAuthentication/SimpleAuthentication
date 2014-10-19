using System;
using Nancy;
using Nancy.Responses.Negotiation;
using Nancy.SimpleAuthentication;
using SimpleAuthentication.Core;
using SimpleAuthentication.Sample.NancyAuto.Models;

namespace SimpleAuthentication.Sample.NancyAuto.Helpers
{
    public class SampleAuthenticationCallbackProvider : INancyAuthenticationProviderCallback
    {
        public dynamic Process(INancyModule module, AuthenticateCallbackResult result)
        {
            var model = new AuthenticationViewModel
            {
                AuthenticatedClient = result.AuthenticatedClient,
                ReturnUrl = result.ReturnUrl
            };

            // Usually, magic stuff with a database happens here ...
            // but for this demo, we'll just dump the result back..

            //return new Negotiator(nancyModule.Context)
            //    .WithModel(model)
            //    .WithView("AuthenticateCallback");

            return module.View["AuthenticateCallback", model];
        }

        public dynamic OnRedirectToAuthenticationProviderError(INancyModule module,
            Exception exception)
        {
            var model = new AuthenticationViewModel
            {
                ErrorMessage = exception.Message
            };

            //return nancyModule.Response.AsRedirect("/");

            return new Negotiator(module.Context)
                .WithModel(model)
                .WithView("AuthenticateCallback");
        }
    }
}