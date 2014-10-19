using System;
using Nancy;
using Nancy.Responses;
using Nancy.Responses.Negotiation;
using Nancy.SimpleAuthentication;
using SimpleAuthentication.Core;

namespace SimpleAuthentication.Tests.WebSites.Nancy
{
    public class FakeAuthenticationProviderCallback : IAuthenticationProviderCallback
    {
        private readonly INancyModule _nancyModule;

        public FakeAuthenticationProviderCallback(INancyModule nancyModule)
        {
            if (nancyModule == null)
            {
                throw new ArgumentNullException("nancyModule");
            }

            _nancyModule = nancyModule;
        }

        public dynamic Process(AuthenticateCallbackResult result)
        {
            var model = new UserViewModel
            {
                Name = result.AuthenticatedClient.UserInformation.Name,
                Email = result.AuthenticatedClient.UserInformation.Email
            };

            Negotiator response;

            // User cancelled during the Authentication process.
            if (result.AuthenticatedClient == null)
            {
                //response = new Negotiator(nancyModule.Context)
                //    .WithHeader("location", result.ReturnUrl)
                //    .WithStatusCode(HttpStatusCode.TemporaryRedirect);
                return _nancyModule.Response.AsRedirect(result.ReturnUrl, RedirectResponse.RedirectType.Temporary);
            }

                // We have a user, so lets do something with their data :)
            else if (string.IsNullOrWhiteSpace(result.ReturnUrl))
            {
                response = new Negotiator(_nancyModule.Context)
                    .WithModel(model)
                    .WithView("FakeView");
                //return nancyModule.View[model];
            }
            else
            {
                response = new Negotiator(_nancyModule.Context)
                    .WithHeader("location", result.ReturnUrl)
                    .WithStatusCode(HttpStatusCode.MovedPermanently);
            }

            //return nancyModule.Response.AsRedirect(result.ReturnUrl);

            return response;
        }

        public dynamic OnRedirectToAuthenticationProviderError(Exception exception)
        {
            throw new NotImplementedException();
        }
    }
}