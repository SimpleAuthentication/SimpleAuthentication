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

            var settings = _authenticationService.GetAuthenticateServiceSettings(providerkey);
            var guidString = Guid.NewGuid().ToString();

            Session[StateKey] = guidString;
            settings.State = guidString;
            settings.CallBackUri = GetReturnUrl("/authentication/authenticatecallback", providerkey);

            Uri uri = _authenticationService.RedirectToAuthenticationProvider(settings);
            
            return Redirect(uri.AbsoluteUri);
        }

        //authentication/redirect
        public ActionResult AuthenticateCallback(string providerkey)
        {
            if (string.IsNullOrEmpty(providerkey))
            {
                throw new ArgumentException("No provider key was supplied on the callback.");
            }

            var existingState = (Session[StateKey] as string) ?? string.Empty;
            var model = new AuthenticateCallbackData();

            try
            {
                model.AuthenticatedClient =
                    _authenticationService.GetAuthenticatedClient(providerkey, Request.QueryString, existingState);
            }
            catch (Exception exception)
            {
                model.Exception = exception;
            }

            return _callbackProvider.Process(HttpContext, model);
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

            if (Request == null || Request.Url == null)
            {
                throw new ArgumentException("Is it even possible for Request/Uri to be null?");
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
