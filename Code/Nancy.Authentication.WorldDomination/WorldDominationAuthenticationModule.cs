using System;
using System.Collections.Specialized;
using WorldDomination.Web.Authentication;

namespace Nancy.Authentication.WorldDomination
{
    public class WorldDominationAuthenticationModule : NancyModule
    {
        private const string StateKey = "WorldDomination-StateKey-cf92a651-d638-4ce4-a393-f612d3be4c3a";
        public static string RedirectRoute = "/authentication/redirect/{providerkey}";
        public static string CallbackRoute = "/authentication/authenticatecallback";

        public WorldDominationAuthenticationModule(IAuthenticationService authenticationService)
            : this(authenticationService, null)
        {
            throw new ApplicationException(
                "World Domination requires you implement your own IAuthenticationCallbackProvider.");
        }

        public WorldDominationAuthenticationModule(IAuthenticationService authenticationService,
                                                   IAuthenticationCallbackProvider authenticationCallbackProvider)
        {
            Get[RedirectRoute] = _ =>
            {
                if (string.IsNullOrEmpty((string)_.providerkey))
                {
                    throw new ArgumentException(
                        "You need to supply a valid provider key so we know where to redirect the user.");
                }
                
                var settings = authenticationService.GetAuthenticateServiceSettings((string)_.providerkey);
                var guidString = Guid.NewGuid().ToString();

                Session[StateKey] = guidString;
                settings.State = guidString;
                settings.CallBackUri = GetReturnUrl("/authentication/authenticatecallback",
                                                    (string)_.providerkey);

                Uri uri = authenticationService.RedirectToAuthenticationProvider(settings);

                return Response.AsRedirect(uri.AbsoluteUri);
            };

            Get[CallbackRoute] = _ =>
            {
                if (string.IsNullOrEmpty(Request.Query.providerkey))
                {
                    throw new ArgumentException("No provider key was supplied on the callback.");
                }

                var existingState = (Session[StateKey] as string) ?? string.Empty;
                var model = new AuthenticateCallbackData();

                try
                {
                    model.AuthenticatedClient =
                        authenticationService.GetAuthenticatedClient((string) Request.Query.providerKey, Request.Query, existingState);
                }
                catch (Exception exception)
                {
                    model.Exception = exception;
                }

                return authenticationCallbackProvider.Process(this, model);
            };
        }

        private Uri GetReturnUrl(string relativeUrl, string providerKey)
        {
            if (string.IsNullOrEmpty(relativeUrl))
            {
                throw new ArgumentNullException("relativeUrl");
            }

            if (string.IsNullOrEmpty(providerKey))
            {
                throw new ArgumentNullException("providerKey");
            }

            var builder = new UriBuilder(Request.Url)
                          {
                              Path = relativeUrl,
                              Query = "providerkey=" + providerKey.ToLowerInvariant()
                          };

            // Don't include port 80/443 in the Uri.
            if (builder.Uri.IsDefaultPort)
            {
                builder.Port = -1;
            }

            return builder.Uri;
        }
    }
}