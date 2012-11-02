using System;
using System.Web.Mvc;
using WorldDomination.Web.Authentication.Facebook;
using WorldDomination.Web.Authentication.Google;
using WorldDomination.Web.Authentication.Twitter;

namespace WorldDomination.Web.Authentication.Test.Mvc.Advanced.App_Start
{
    public class AuthenticationConfig
    {
        public static void RegisterAuthenticationProviders()
        {

            var facebookProvider = new FacebookProvider(FacebookAppId, FacebookAppSecret,
                                                        new Uri(
                                                            "http://localhost:1337/home/AuthenticateCallback?providerKey=facebook"));

            var twitterProvider = new TwitterProvider(TwitterConsumerKey, TwitterConsumerSecret,
                                                      new Uri(
                                                          "http://localhost:1337/home/AuthenticateCallback?providerKey=twitter"));

            var googleProvider = new GoogleProvider(GoogleConsumerKey, GoogleConsumerSecret,
                                                    new Uri(
                                                        "http://localhost:1337/home/AuthenticateCallback?providerKey=google"));

            _authenticationService = new AuthenticationService();
            _authenticationService.AddProvider(facebookProvider);
            _authenticationService.AddProvider(twitterProvider);
            _authenticationService.AddProvider(googleProvider);



        //    public static void RegisterAuth()
        //{
        //    // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
        //    // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

        //    //OAuthWebSecurity.RegisterMicrosoftClient(
        //    //    clientId: "",
        //    //    clientSecret: "");

        //    //OAuthWebSecurity.RegisterTwitterClient(
        //    //    consumerKey: "",
        //    //    consumerSecret: "");

        //    //OAuthWebSecurity.RegisterFacebookClient(
        //    //    appId: "",
        //    //    appSecret: "");

        //    //OAuthWebSecurity.RegisterGoogleClient();
        //}
        }
    }
}