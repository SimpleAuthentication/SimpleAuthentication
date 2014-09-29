using System;
using Nancy;
using Nancy.Responses;
using Nancy.SimpleAuthentication;

namespace SimpleAuthentication.Tests.WebSites.Nancy
{
    public class FakeAuthenticationProviderCallback : IAuthenticationProviderCallback
    {
        public dynamic Process(NancyModule nancyModule, AuthenticateCallbackResult result)
        {
            var model = new UserViewModel
            {
                Name = result.AuthenticatedClient.UserInformation.Name,
                Email = result.AuthenticatedClient.UserInformation.Email
            };

            // User cancelled during the Authentication process.
            if (result.AuthenticatedClient == null)
            {
                return nancyModule.Response.AsRedirect(result.ReturnUrl, RedirectResponse.RedirectType.Temporary);
            }

            // We have a user, so lets do something with their data :)
            if (string.IsNullOrWhiteSpace(result.ReturnUrl))
            {
                return nancyModule.View[model];
            }

            return nancyModule.Response.AsRedirect(result.ReturnUrl);
        }

        public dynamic OnRedirectToAuthenticationProviderError(NancyModule nancyModule, Exception exception)
        {
            throw new NotImplementedException();
        }
    }
}