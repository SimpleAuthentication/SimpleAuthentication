using Moq;
using RestSharp;
using WorldDomination.Web.Authentication.Twitter;

namespace WorldDomination.UnitTests
{
    // ReSharper disable InconsistentNaming

    public class TwitterProviderFacts
    {
        public class ApiVerifyCredentialsFacts
        {
            //public void GivenValidAuthenticationData_ApiVerifyCredentials_ReturnsAVerifyCredentialsResult()
            //{
            //    // Arrange.
            //    var mockRestClient = new Mock<IRestClient>();
            //    mockRestClient.Setup(x => x.Execute<VerifyCredentialsResult>(It.IsAny<IRestRequest>()))
            //        .Returns()
            //    var json =
            //        "{\"name\": \"Matt Harris\",\"profile_sidebar_border_color\": \"C0DEED\",\"profile_background_tile\": false,\"profile_sidebar_fill_color\": \"DDEEF6\",\"location\": \"San Francisco\",\"profile_image_url\": \"http://a1.twimg.com/profile_images/554181350/matt_normal.jpg\",\"created_at\": \"Sat Feb 17 20:49:54 +0000 2007\",\"profile_link_color\": \"0084B4\",\"favourites_count\": 95,\"url\": \"http://themattharris.com\",\"contributors_enabled\": false,\"utc_offset\": -28800,\"id\": 777925,\"profile_use_background_image\": true,\"profile_text_color\": \"333333\",\"protected\": false,\"followers_count\": 1025,\"lang\": \"en\",\"verified\": false,\"profile_background_color\": \"C0DEED\",\"geo_enabled\": true,\"notifications\": false,\"description\": \"Developer Advocate at Twitter. Also a hacker and British expat who is married to @cindyli and lives in San Francisco.\",\"time_zone\": \"Tijuana\",\"friends_count\": 294,\"statuses_count\": 2924,\"profile_background_image_url\": \"http://s.twimg.com/a/1276711174/images/themes/theme1/bg.png\",\"status\": {\"coordinates\": {\"coordinates\": [-122.40075845,37.78264991],\"type\": \"Point\"},\"favorited\": false,\"created_at\": \"Tue Jun 22 18:17:48 +0000 2010\",\"truncated\": false,\"text\": \"Going through and updating @twitterapi documentation\",\"contributors\": null,\"id\": 16789004997,\"geo\": {\"coordinates\": [37.78264991,-122.40075845],\"type\": \"Point\"},\"in_reply_to_user_id\": null,\"place\": null,\"source\": \"<a href=\\\"http://itunes.apple.com/app/twitter/id333903271?mt=8\\\" rel=\\\"nofollow\\\">Twitter for iPhone</a>\",\"in_reply_to_screen_name\": null,\"in_reply_to_status_id\": null},\"screen_name\": \"themattharris\",\"following\": false}";
            //    var twitterProvider = new TwitterProvider("a", "b");
            //    var twitterClient = new TwitterClient
            //                        {
            //                            OAuthToken = "c",
            //                            OAuthVerifier = "d"
            //                        };

            //    // Arrange.
            //    var result = twitterProvider.ApiVerifyCredentials(twitterClient);
            //}
        }
    }

    // ReSharper restore InconsistentNaming
}