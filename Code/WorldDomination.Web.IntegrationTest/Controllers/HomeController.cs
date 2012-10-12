using System;
using System.Web.Mvc;
using WorldDomination.Web.Authentication;
using WorldDomination.Web.Authentication.Facebook;
using WorldDomination.Web.IntegrationTest.Models;

namespace WorldDomination.Web.IntegrationTest.Controllers
{
    public class HomeController : Controller
    {
        private const string FacebookAppId = "113220502168922";
        private const string FacebookAppSecret = "b09592a5904746646f3d402178ce9c0f";

        private FacebookProvider FacebookProvider { get; set; }
        private AuthenticationService AuthenticationService { get; set; }
        public HomeController()
        {
            FacebookProvider = new FacebookProvider(FacebookAppId,
                                                    FacebookAppSecret,
                                                    new Uri("http://localhost:1337/home/authenticateCallback"),
                                                    new WebClientWrapper());

            AuthenticationService = new AuthenticationService(FacebookProvider);
        }

        public ActionResult Index()
        {
            return View();
        }

        public RedirectResult FacebookAuthentication()
        {
            Session.Add("a", "a"); // Keep the SessionId constant.
            return AuthenticationService.RedirectToFacebookAuthentication(Session.SessionID);
        }

        public ActionResult AuthenticateCallback()
        {
            var client = AuthenticationService.CheckCallback(Request, Session.SessionID);

            var model = new AuthenticateCallbackViewModel();
            
            if (client is FacebookClient)
            {
                var facebookClient = client as FacebookClient;
                model.AccessToken = facebookClient.AccessToken;
                model.Name =
                    (facebookClient.UserInformation.FirstName + " " + facebookClient.UserInformation.LastName).Trim();
                model.UserName = facebookClient.UserInformation.UserName;
                model.Message = "Authenticated with Facebook successfully.";
            }

            return View(model);
        }
    }
}