using System;
using WorldDomination.Web.Authentication;

namespace Nancy.Authentication.WorldDomination
{
    public class WorldDominationAuthenticationModule : NancyModule
    {
        private const string StateKey = "WorldDomination-StateKey-cf92a651-d638-4ce4-a393-f612d3be4c3a";
        private const string RedirectUrlKey = "WorldDomination-RedirectUrlKey-cf92a651-d638-4ce4-a393-f612d3be4c3a";
        public static string RedirectRoute = "/authentication/redirect/{providerkey}";
        public static string CallbackRoute = "/authentication/authenticatecallback";

        protected Uri RedirectUrl { get; set; }

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
                var providerKey = (string)_.providerkey;
                if (string.IsNullOrEmpty(providerKey))
                {
                    throw new ArgumentException(
                        "You need to supply a valid provider key so we know where to redirect the user.");
                }
                
                // Kthxgo!
                return RedirectToAuthenticationProvider(authenticationService, authenticationCallbackProvider, providerKey);
            };

            Post[RedirectRoute] = _ =>
            {
                var providerKey = (string)_.providerkey;
                if (string.IsNullOrEmpty(providerKey))
                {
                    throw new ArgumentException(
                        "You need to supply a valid provider key so we know where to redirect the user.");
                }

                Uri identifier = null;
                if (string.IsNullOrEmpty(Request.Form.Identifier) ||
                    !Uri.TryCreate(Request.Form.Identifier, UriKind.RelativeOrAbsolute, out identifier))
                {
                    throw new ArgumentException(
                        "You need to POST the identifier to redirect the user. Eg. http://myopenid.com");
                }

                return RedirectToAuthenticationProvider(authenticationService, authenticationCallbackProvider, providerKey, identifier);
            };

            Get[CallbackRoute] = _ =>
            {
                var providerKey = Request != null && Request.Query != null
                                    ? (string)Request.Query.providerkey
                                    : null;

                if (string.IsNullOrEmpty(providerKey))
                {
                    throw new ArgumentException("No provider key was supplied on the callback.");
                }

                var settings = authenticationService.GetAuthenticateServiceSettings(providerKey, Request.Url);

                settings.State = (Session[StateKey] as string) ?? string.Empty;

                var model = new AuthenticateCallbackData();

                try
                {
                    model.AuthenticatedClient = authenticationService.GetAuthenticatedClient(settings, Request.Query);
                    Session.Delete(StateKey); // Clean up :)
                }
                catch (Exception exception)
                {
                    model.Exception = exception;
                }

                var redirectUrl = Session[RedirectUrlKey] as string;
                if (!string.IsNullOrEmpty(redirectUrl))
                {
                    model.RedirectUrl = new Uri(redirectUrl);
                }

                return authenticationCallbackProvider.Process(this, model);
            };
        }

        private Response RedirectToAuthenticationProvider(IAuthenticationService authenticationService,
            IAuthenticationCallbackProvider authenticationCallbackProvider,
            string providerKey, Uri identifier = null)
        {
            if (authenticationService == null)
            {
                throw new ArgumentNullException();
            }

            if (authenticationCallbackProvider == null)
            {
                throw new ArgumentNullException("authenticationCallbackProvider");
            }

            if (string.IsNullOrEmpty(providerKey))
            {
                throw new ArgumentNullException("providerKey");
            }

            // Grab the required Provider settings.

            var settings = authenticationService.GetAuthenticateServiceSettings(providerKey, Request.Url);

            // An OpenId specific settings provided?
            if (identifier != null && 
                settings is IOpenIdAuthenticationServiceSettings)
            {
                ((IOpenIdAuthenticationServiceSettings) settings).Identifier = identifier;
            }

            // Remember the State value (for CSRF protection).
            Session[StateKey] = settings.State;

            //// Convention: If no redirectUrl data has been provided, then default to the Referrer, if one exists.
            //if (RedirectUrl != null &&
            //    !string.IsNullOrEmpty(RedirectUrl.AbsoluteUri))
            //{
            //    // We have extra state information we will need to retrieve.
            //    Session[RedirectUrlKey] = RedirectUrl.AbsoluteUri;
            //}
            //else if (Request != null &&
            //    Request. != null &&
            //    !string.IsNullOrEmpty(Request.UrlReferrer.AbsoluteUri))
            //{
            //    Session[RedirectUrlKey] = Request.UrlReferrer.AbsoluteUri;
            //}

            // Determine the provider's end point Url we need to redirect to.
            // NOTE: It's possible we're trying to goto an OpenId endpoint. But the user has entered
            var uri = authenticationService.RedirectToAuthenticationProvider(settings);
            if (uri == null || string.IsNullOrEmpty(uri.AbsoluteUri))
            {
                return authenticationCallbackProvider.OnRedirectToAuthenticationProviderError(this, "No valid Uri was found - not sure where to redirect to?");
            }

            // Kthxgo!
            return Response.AsRedirect(uri.AbsoluteUri);
        }
    }
}