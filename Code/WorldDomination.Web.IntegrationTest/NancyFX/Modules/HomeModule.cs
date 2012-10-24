using System;
using System.Collections.Specialized;
using Nancy;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Facebook;
using WorldDomination.Web.Authentication.Google;
using WorldDomination.Web.Authentication.Twitter;
using WorldDomination.Web.IntegrationTest.NancyFX.Model;

namespace WorldDomination.Web.IntegrationTest.NancyFX.Modules
{
    public class HomeModule : NancyModule
    {
        private const string FacebookAppId = "159181340893340";
        private const string FacebookAppSecret = "97c4e4d0fa548232cf8f9c68a7adcff9";
        private const string TwitterConsumerKey = "HSqyaOQ8LXiiiL1gksigfw";
        private const string TwitterConsumerSecret = "nKHU1vBXA3yijGZs1qpbeRBgEb4boquGGNHRMfcus";
        private const string GoogleConsumerKey = "587140099194.apps.googleusercontent.com";
        private const string GoogleConsumerSecret = "npk1_gx-gqJmLiJRPFooxCEY";
        private const string SessionGuidKey = "GUIDKey";
        private readonly AuthenticationService _authenticationService;

        public HomeModule()
        {
            var facebookProvider = new FacebookProvider(FacebookAppId, FacebookAppSecret,
                                                        new Uri(
                                                            "http://localhost:6969/AuthenticateCallback?providerKey=facebook"));

            var twitterProvider = new TwitterProvider(TwitterConsumerKey, TwitterConsumerSecret,
                                                      new Uri(
                                                          "http://localhost:6969/AuthenticateCallback?providerKey=twitter"));

            var googleProvider = new GoogleProvider(GoogleConsumerKey, GoogleConsumerSecret,
                                                    new Uri(
                                                        "http://localhost:6969/AuthenticateCallback?providerKey=google"));

            _authenticationService = new AuthenticationService();
            _authenticationService.AddProvider(facebookProvider);
            _authenticationService.AddProvider(twitterProvider);
            _authenticationService.AddProvider(googleProvider);

            Get["/"] = parameters => View["login"];

            Get["/RedirectToAuthenticate/{providerKey}"] = parameters =>
                                                           {
                                                               Session[SessionGuidKey] = Guid.NewGuid();
                                                               Uri uri =
                                                                   _authenticationService.
                                                                       RedirectToAuthenticationProvider(
                                                                           parameters.providerKey,
                                                                           Session[SessionGuidKey].ToString());

                                                               return Response.AsRedirect(uri.AbsoluteUri);
                                                           };

            Get["/AuthenticateCallback"] = parameters =>
                                           {
                                               if (string.IsNullOrEmpty(Request.Query.providerKey))
                                               {
                                                   throw new ArgumentNullException("providerKey");
                                               }

                                               var model = new AuthenticateCallbackViewModel();

                                               var querystringParameters = new NameValueCollection();
                                               foreach (var item in Request.Query)
                                               {
                                                   querystringParameters.Add(item, Request.Query[item]);
                                               }

                                               try
                                               {
                                                   model.AuthenticatedClient =
                                                       _authenticationService.CheckCallback(Request.Query.providerKey,
                                                                                            querystringParameters,
                                                                                            Session[SessionGuidKey].
                                                                                                ToString());
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