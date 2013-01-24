using System;
using WorldDomination.Web.Authentication;

namespace Nancy.Authentication.WorldDomination
{
    public class WorldDominationAuthenticationModule:NancyModule
    {
        private const string StateKey = "WorldDomination-StateKey-cf92a651-d638-4ce4-a393-f612d3be4c3a";

        public WorldDominationAuthenticationModule(IAuthenticationService authenticationService, 
                                                   IAuthenticationCallbackProvider authenticationCallbackProvider)
        {
            Get["/authentication/redirect?{provider}"] = _ =>
            {
                if (string.IsNullOrEmpty(Request.Query.provider))
                {
                    throw new ArgumentException("You need to supply a valid provider key so we know where to redirect the user.");
                }

                var settings = authenticationService.GetAuthenticateServiceSettings((string)_.providerKey);
                var guidString = Guid.NewGuid().ToString();

                Session[StateKey] = guidString;
                settings.State = guidString;
                settings.CallBackUri = GetReturnUrl(Context, "/authentication/authenticatecallback", (string)_.providerKey);

                Uri uri = authenticationService.RedirectToAuthenticationProvider(settings);

                return Response.AsRedirect(uri.AbsoluteUri);
            };

            Get["/authentication/authenticatecallback"] = _ =>
            {
                if (string.IsNullOrEmpty(Request.Query.providerkey))
                {
                    throw new ArgumentException("No provider key was supplied on the callback.");
                }

                return "";
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

            return new Uri(string.Format("{0}://{1}{2}{3}?providerkey={4}",
                                         url.Scheme, url.HostName, port, relativeUrl, provider));
        }
    }
}