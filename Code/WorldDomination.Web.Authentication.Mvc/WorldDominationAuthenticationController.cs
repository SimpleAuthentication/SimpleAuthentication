using System;
using System.Web.Mvc;

namespace WorldDomination.Web.Authentication.Mvc
{
    public class WorldDominationAuthenticationController : Controller
    {
        private const string StateKey = "WorldDomination-StateKey-cf92a651-d638-4ce4-a393-f612d3be4c3a";
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthenticationCallbackProvider _callbackProvider;

        public WorldDominationAuthenticationController(IAuthenticationService authenticationService, IAuthenticationCallbackProvider callbackProvider)
        {
            _authenticationService = authenticationService;
            _callbackProvider = callbackProvider;
        }

        public RedirectResult RedirectToProvider(string providerkey, string additionaldata = null)
        {
            if (string.IsNullOrEmpty(providerkey))
            {
                throw new ArgumentException(
                    "You need to supply a valid provider key so we know where to redirect the user.");
            }

            // Grab the required Provider settings.
            var settings = _authenticationService.GetAuthenticateServiceSettings(providerkey, Request.Url);

            // Remember the State value (for CSRF protection).
            Session[StateKey] = settings.State;

            // Determine the provider's end point Url we need to redirect to.
            var  uri = _authenticationService.RedirectToAuthenticationProvider(settings);
            
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
            var settings = _authenticationService.GetAuthenticateServiceSettings(providerkey, Request.Url);

            // Make sure we use our 'previous' State value.
            settings.State = (Session[StateKey] as string) ?? string.Empty;
            
            var model = new AuthenticateCallbackData();

            try
            {
                // Grab the authenticated client information.
                model.AuthenticatedClient = _authenticationService.GetAuthenticatedClient(settings, Request.QueryString);
                Session.Remove(StateKey);
            }
            catch (Exception exception)
            {
                model.Exception = exception;
            }

            return _callbackProvider.Process(HttpContext, model);
        }
    }
}
