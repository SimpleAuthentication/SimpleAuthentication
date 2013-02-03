using System;
using System.Collections.Specialized;
using Nancy;
using WorldDomination.Web.Authentication.Samples.NancyFX.Model;

namespace WorldDomination.Web.Authentication.Samples.NancyFX.Modules
{
    public class HomeModule : NancyModule
    {
        private const string SessionGuidKey = "GUIDKey";

        public HomeModule(IAuthenticationService authenticationService)
        {
            Get["/"] = parameters => View["login"];

            Get["/RedirectToAuthenticate/{providerKey}"] = parameters =>
            {
                // State key.
                Session[SessionGuidKey] = Guid.NewGuid();

                var settings =
                    authenticationService.GetAuthenticateServiceSettings(parameters.providerKey.Default<string>());

                settings.State = Session[SessionGuidKey].ToString();
                settings.CallBackUri = GetReturnUrl(Context, "/AuthenticateCallback", parameters.providerKey);

                Uri uri = authenticationService.RedirectToAuthenticationProvider(settings);

                return Response.AsRedirect(uri.AbsoluteUri);
            };

            Get["/AuthenticateCallback"] = parameters =>
            {
                if (string.IsNullOrEmpty(Request.Query.providerKey))
                {
                    throw new ArgumentNullException("providerKey");
                }

                // Retrieve the state for the XSS check.
                // It's possible that a person might hit this resource directly, before any session value
                // has been set. As such, we should just fake some state up, which will not match the
                // CSRF check.
                var existingState = (Guid) (Session[SessionGuidKey] ?? Guid.NewGuid());
                var model = new AuthenticateCallbackViewModel();
                
                try
                {
                    model.AuthenticatedClient =
                        authenticationService.GetAuthenticatedClient(Request.Query.providerKey,
                                                                     Request.Query,
                                                                     existingState.ToString());
                }
                catch (Exception exception)
                {
                    model.Exception = exception;
                }

                return View["AuthenticateCallback", model];
            };
        }

        private Uri GetReturnUrl(NancyContext context, string relativeUrl, string provider)
        {
            if (!relativeUrl.StartsWith("/"))
            {
                relativeUrl = relativeUrl.Insert(0, "/");
            }

            var url = context.Request.Url;
            var port = url.Port != 80 ? (":" + url.Port) : string.Empty;

            return new Uri(string.Format("{0}://{1}{2}{3}?providerKey={4}",
                                         url.Scheme, url.HostName, port, relativeUrl, provider));
        }
    }
}