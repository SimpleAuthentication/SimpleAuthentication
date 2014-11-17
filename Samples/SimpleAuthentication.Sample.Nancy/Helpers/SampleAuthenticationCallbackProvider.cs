using Nancy;
using Nancy.Responses.Negotiation;
using Nancy.SimpleAuthentication;
using SimpleAuthentication.Core;
using SimpleAuthentication.Core.Exceptions;
using SimpleAuthentication.Sample.Nancy.Models;

namespace SimpleAuthentication.Sample.Nancy.Helpers
{
    public class SampleAuthenticationCallbackProvider : SimpleAuthenticationProviderCallback
    {
        public override dynamic Process(INancyModule module, AuthenticateCallbackResult result)
        {
            var model = new AuthenticationViewModel
            {
                AuthenticatedClient = result.AuthenticatedClient,
                ReturnUrl = result.ReturnUrl
            };

            // Usually, magic stuff with a database happens here ...
            // but for this demo, we'll just dump the result back..


            return module.View["AuthenticateCallback", model];

            // --Or--
            // you can use a Negotiator...
            //return new Negotiator(module.Context)
            //    .WithModel(model)
            //    .WithView("AuthenticateCallback");
        }

        public override dynamic OnError(INancyModule module, ErrorType errorType, AuthenticationException exception)
        {
            var model = new AuthenticationViewModel
            {
                ErrorMessage = exception.Message
            };

            //return nancyModule.Response.AsRedirect("/");

            if (errorType != ErrorType.UserInformation)
            {
                return new Negotiator(module.Context)
                    .WithModel(model)
                    .WithView("AuthenticateCallback");
            }

            var errorModel = new
            {
                errorMessage = exception.Message
            };

            return module.Response.AsJson(errorModel, (HttpStatusCode) exception.HttpStatusCode);
        }
    }
}