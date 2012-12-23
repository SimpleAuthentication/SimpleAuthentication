using System;
using System.Configuration;
using StructureMap;
using WorldDomination.Web.Authentication.Facebook;
using WorldDomination.Web.Authentication.Google;
using WorldDomination.Web.Authentication.Twitter;

namespace WorldDomination.Web.Authentication.Test.Mvc.Advanced.DependencyResolution
{
    public static class IoC
    {
        public static IContainer Initialize()
        {
            ObjectFactory.Initialize(x =>
                                     {
                                         var authenticationRegistry = new AuthenticationRegistry(
                                             new FacebookProvider(ConfigurationManager.AppSettings["FacebookAppId"],
                                                                  ConfigurationManager.AppSettings["FacebookAppSecret"],
                                                                  new Uri(ConfigurationManager.AppSettings["FacebookRedirectUri"])),
                                             new GoogleProvider(ConfigurationManager.AppSettings["GoogleConsumerKey"],
                                                                ConfigurationManager.AppSettings["GoogleConsumerSecret"],
                                                                new Uri(ConfigurationManager.AppSettings["GoogleConsumerRedirectUri"])),
                                             new TwitterProvider(ConfigurationManager.AppSettings["TwitterConsumerKey"],
                                                                 ConfigurationManager.AppSettings["TwitterConsumerSecret"],
                                                                 new Uri(ConfigurationManager.AppSettings["TwitterConsumerRedirectUri"]))
                                             );
                                         x.AddRegistry(authenticationRegistry);
                                     });
            return ObjectFactory.Container;
        }
    }
}