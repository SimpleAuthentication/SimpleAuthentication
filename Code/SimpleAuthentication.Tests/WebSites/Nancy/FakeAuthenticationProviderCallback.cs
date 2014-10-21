using System;
using Nancy;
using Nancy.Responses;
using Nancy.Responses.Negotiation;
using Nancy.SimpleAuthentication;
using SimpleAuthentication.Core;

namespace SimpleAuthentication.Tests.WebSites.Nancy
{
    public class FakeAuthenticationProviderCallback : INancyAuthenticationProviderCallback
    {
        public dynamic Process(INancyModule nancyModule, AuthenticateCallbackResult result)
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
                return nancyModule.Response.AsRedirect(result.ReturnUrl, RedirectResponse.RedirectType.Temporary);
            }

                // We have a user, so lets do something with their data :)
            else if (string.IsNullOrWhiteSpace(result.ReturnUrl))
            {
                response = new Negotiator(nancyModule.Context)
                    .WithModel(model)
                    .WithView("FakeView");
                //return nancyModule.View[model];
            }
            else
            {
                response = new Negotiator(nancyModule.Context)
                    .WithHeader("location", result.ReturnUrl)
                    .WithStatusCode(HttpStatusCode.MovedPermanently);
            }

            //return nancyModule.Response.AsRedirect(result.ReturnUrl);

            return response;
        }

        public dynamic OnRedirectToAuthenticationProviderError(INancyModule nancyModule, Exception exception)
        {
            throw new NotImplementedException();
        }
    }
}