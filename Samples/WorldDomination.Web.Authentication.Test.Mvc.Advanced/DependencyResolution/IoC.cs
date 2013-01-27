using System.Configuration;
using StructureMap;
using WorldDomination.Web.Authentication.Facebook;
using WorldDomination.Web.Authentication.Google;
using WorldDomination.Web.Authentication.Twitter;

namespace WorldDomination.Web.Authentication.Samples.Mvc.Advanced.DependencyResolution
{
    public static class IoC
    {
        public static IContainer Initialize()
        {
            ObjectFactory.Initialize(x =>
            {
                var authenticationRegistry = new AuthenticationRegistry(
                    new FacebookProvider(ConfigurationManager.AppSettings["FacebookAppId"],
                                         ConfigurationManager.AppSettings["FacebookAppSecret"]),
                    new GoogleProvider(ConfigurationManager.AppSettings["GoogleConsumerKey"],
                                       ConfigurationManager.AppSettings["GoogleConsumerSecret"]),
                    new TwitterProvider(ConfigurationManager.AppSettings["TwitterConsumerKey"],
                                        ConfigurationManager.AppSettings["TwitterConsumerSecret"])
                    );
                x.AddRegistry(authenticationRegistry);
            });
            return ObjectFactory.Container;
        }
    }
}