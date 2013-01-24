using System;
using System.Collections.Specialized;
using WorldDomination.Web.Authentication;

namespace Nancy.Authentication.WorldDomination
{
    public class WorldDominationAuthenticationModule : NancyModule
    {
        private const string StateKey = "WorldDomination-StateKey-cf92a651-d638-4ce4-a393-f612d3be4c3a";

        public WorldDominationAuthenticationModule(IAuthenticationService authenticationService)
            : this(authenticationService, null)
        {
            throw new ApplicationException(
                "World Domination requires you implement your own IAuthenticationCallbackProvider");
        }

        public WorldDominationAuthenticationModule(IAuthenticationService authenticationService,
                                                   IAuthenticationCallbackProvider authenticationCallbackProvider)
        {
            Get["/authentication/redirect/{providerkey}"] = _ =>
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
                settings.CallBackUri = GetReturnUrl(Context, "/authentication/authenticatecallback",
                                                    (string)_.providerkey);

                Uri uri = authenticationService.RedirectToAuthenticationProvider(settings);

                return Response.AsRedirect(uri.AbsoluteUri);
            };

            Get["/authentication/authenticatecallback"] = _ =>
            {
                if (string.IsNullOrEmpty(Request.Query.providerkey))
                {
                    throw new ArgumentException("No provider key was supplied on the callback.");
                }

                var existingState = (Session[StateKey] as string) ?? string.Empty;
                var model = new AuthenticateCallbackData();
                var querystringParameters = new NameValueCollection();

                foreach (var item in Request.Query)
                {
                    querystringParameters.Add(item, Request.Query[item]);
                }

                try
                {
                    model.AuthenticatedClient =
                        authenticationService.GetAuthenticatedClient((string) Request.Query.providerKey,
                                                                     querystringParameters, existingState);
                }
                catch (Exception exception)
                {
                    model.Exception = exception;
                }

                var result = authenticationCallbackProvider.Process(Context, model);

                if (result.Action == ProcessResult.ActionEnum.Redirect)
                {
                    return Response.AsRedirect(result.RedirectTo);
                }

                return View[result.View, result.ViewModel];
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
                                         url.Scheme, url.HostName, port, relativeUrl, provider.ToLower()));
        }
    }
}