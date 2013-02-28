using System;
using System.Collections.Specialized;
using System.Web.Mvc;

namespace WorldDomination.Web.Authentication.Mvc
{
    public class WorldDominationAuthenticationController : Controller
    {
        private const string StateKey = "WorldDomination-StateKey-cf92a651-d638-4ce4-a393-f612d3be4c3a";
        private const string RedirectUrlKey = "WorldDomination-RedirectUrlKey-cf92a651-d638-4ce4-a393-f612d3be4c3a";

        protected IAuthenticationService AuthenticationService { get; private set; }
        public IAuthenticationCallbackProvider CallbackProvider { get; private set; }

        protected Uri RedirectUrl { get; set; }

        public WorldDominationAuthenticationController(IAuthenticationService authenticationService, 
                                                       IAuthenticationCallbackProvider callbackProvider)
        {
            if (authenticationService == null)
            {
                throw new ArgumentNullException("authenticationService");
            }

            if (callbackProvider == null)
            {
                throw new ArgumentNullException("callbackProvider");
            }

            AuthenticationService = authenticationService;
            CallbackProvider = callbackProvider;
        }

        public RedirectResult RedirectToProvider(string providerkey)
        {
            if (string.IsNullOrEmpty(providerkey))
            {
                throw new ArgumentException(
                    "You need to supply a valid provider key so we know where to redirect the user.");
            }

            // Grab the required Provider settings.
            var settings = AuthenticationService.GetAuthenticateServiceSettings(providerkey, Request.Url);

            // Remember the State value (for CSRF protection).
            Session[StateKey] = settings.State;

            // Convention: If no redirectUrl data has been provided, then default to the Referrer, if one exists.
            if (RedirectUrl != null &&
                !string.IsNullOrEmpty(RedirectUrl.AbsoluteUri))
            {
                // We have extra state information we will need to retrieve.
                Session[RedirectUrlKey] = RedirectUrl.AbsoluteUri;
            }
            else if (Request != null &&
                Request.UrlReferrer != null &&
                !string.IsNullOrEmpty(Request.UrlReferrer.AbsoluteUri))
            {
                Session[RedirectUrlKey] = Request.UrlReferrer.AbsoluteUri;
            }

            // Determine the provider's end point Url we need to redirect to.
            var  uri = AuthenticationService.RedirectToAuthenticationProvider(settings);
            
            // Kthxgo!
            return Redirect(uri.AbsoluteUri);
        }

        public ActionResult AuthenticateCallback(string providerkey)
        {
            if (string.IsNullOrEmpty(providerkey))
            {
                throw new ArgumentException("No provider key was supplied on the callback.");
            }

            // Determine which settings we need, based on the Provider.
            var settings = AuthenticationService.GetAuthenticateServiceSettings(providerkey, Request.Url);

            // Make sure we use our 'previous' State value.
            settings.State = (Session[StateKey] as string) ?? string.Empty;
            
            var model = new AuthenticateCallbackData();

            try
            {
                // Grab the authenticated client information.
                model.AuthenticatedClient = AuthenticationService.GetAuthenticatedClient(settings, Request.QueryString);
                Session.Remove(StateKey);
            }
            catch (Exception exception)
            {
                model.Exception = exception;
            }

            // If we have a redirect Url, lets grab this :)
            var redirectUrl = Session[RedirectUrlKey] as string;
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                model.RedirectUrl = new Uri(redirectUrl);
            }
            
            Session.Remove(RedirectUrlKey);

            // Finally! We can hand over the logic to the consumer to do whatever they want.
            return CallbackProvider.Process(HttpContext, model);
        }
    }
}
