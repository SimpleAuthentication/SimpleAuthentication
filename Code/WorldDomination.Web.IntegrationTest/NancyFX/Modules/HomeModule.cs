using System;
using System.Collections.Specialized;
using System.Web.UI.WebControls;
using Nancy;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Twitter;
using WorldDomination.Web.IntegrationTest.NancyFX.Model;

namespace WorldDomination.Web.IntegrationTest.NancyFX.Modules
{
    public class HomeModule : NancyModule
    {
        private const string SessionGuidKey = "GUIDKey";

        public HomeModule(IAuthenticationService authenticationService)
        {
            Get["/"] = parameters => View["login"];

            Get["/RedirectToAuthenticate/{providerKey}"] = parameters =>
                {
                    Session[SessionGuidKey] = Guid.NewGuid();
                    Uri uri = authenticationService.RedirectToAuthenticationProvider(parameters.providerKey, Session[SessionGuidKey].ToString());

                    return Response.AsRedirect(uri.AbsoluteUri);
                };

            Get["/AuthenticateCallback"] = parameters =>
                {
                    if (string.IsNullOrEmpty(this.Request.Query.providerKey))
                    {
                        throw new ArgumentNullException("providerKey");
                    }

                    var model = new AuthenticateCallbackViewModel();

                    NameValueCollection coll = new NameValueCollection();
                    foreach (var item in this.Request.Query)
                    {
                        coll.Add(item, this.Request.Query[item]);
                    }

                    try
                    {
                        model.AuthenticatedClient = authenticationService.CheckCallback(this.Request.Query.providerKey, coll, Session[SessionGuidKey].ToString());
                    }
                    catch (Exception exception)
                    {
                        model.Exception = exception;
                    }

                    return View["AuthenticateCallback", model];
                };
        }
    }
}