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
        private const string TwitterConsumerKey = "Rb7qNNPUPsRSYkznFTbF6Q";
        private const string TwitterConsumerSecret = "pP1jBdYOlmCzo08QFJjGIHY4YSyPdGLPO2m1q47hu9c";
        private const string SessionGuidKey = "GUIDKey";
        private readonly AuthenticationService authenticationService;

        public HomeModule()
        {
            var twitterProvider = new TwitterProvider(TwitterConsumerKey, TwitterConsumerSecret,
                                                      new Uri("http://localhost:49409/AuthenticateCallback?providerKey=Twitter"));

            authenticationService = new AuthenticationService();
            authenticationService.AddProvider(twitterProvider);

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