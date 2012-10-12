using Moq;
using WorldDomination.Web.Authentication;

namespace WorldDomination.UnitTests
{
    public static class MoqUtilities
    {
        public static Mock<IWebClientWrapper> MockedIWebClientWrapper(params string[] results)
        {
            const string accessToken =
                "access_token=AAABmZBTPQJVoBAIkLSZAofoC1rRB3tQpmvyiKZBO3dgRxopnVdZCCyvIZBiewnAsc0hZBaPYJ5ZCrEeYBxDhQZCHuZBpzy80NlymZCBaP2ZBGvDtwZDZD&expires=5183995";
            const string json =
                "{\"id\":\"566632497\",\"name\":\"Fuk Chop\",\"first_name\":\"Fuk\",\"last_name\":\"Chop\",\"link\":\"http:\\/\\/www.facebook.com\\/fukchop\",\"username\":\"fukchop\",\"timezone\":10,\"locale\":\"en_GB\",\"verified\":true,\"updated_time\":\"2012-05-13T06:05:21+0000\"}";
            var mockWebClientWrapper = new Mock<IWebClientWrapper>();
            mockWebClientWrapper.Setup(x => x.DownloadString(It.IsAny<string>()))
                .ReturnsInOrder(results == null || results.Length <= 0 ? new [] {accessToken, json} : results);

            return mockWebClientWrapper;
        }
    }
}