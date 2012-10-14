using System.Collections.Generic;
using Newtonsoft.Json;

namespace WorldDomination.Web.Authentication.Twitter
{
    public class Geo
    {
        public List<double> Coordinates { get; set; }
        public string Type { get; set; }
    }

    public class Attributes
    {
    }

    public class BoundingBox
    {
        public List<List<List<double>>> Coordinates { get; set; }
        public string Type { get; set; }
    }

    public class Place
    {
        public Attributes Attributes { get; set; }

        //[JsonProperty(PropertyName = "bounding_box")]
        //public BoundingBox BoundingBox { get; set; }

        public string Country { get; set; }

        [JsonProperty(PropertyName = "country_code")]
        public string CountryCode { get; set; }

        [JsonProperty(PropertyName = "full_name")]
        public string FullName { get; set; }

        public string Id { get; set; }
        public string Name { get; set; }

        [JsonProperty(PropertyName = "place_type")]
        public string PlaceType { get; set; }

        public string Url { get; set; }
    }

    public class Status
    {
        public object Contributors { get; set; }
        public Geo Coordinates { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public string CreatedAt { get; set; }

        public bool Favorited { get; set; }
        public Geo Geo { get; set; }
        public long Id { get; set; }

        [JsonProperty(PropertyName = "Id_str")]
        public string Idtr { get; set; }

        [JsonProperty(PropertyName = "in_reply_to_screen_name")]
        public string InReplyToScreenName { get; set; }

        [JsonProperty(PropertyName = "in_reply_to_status_id")]
        public long InReplyToStatusId { get; set; }

        [JsonProperty(PropertyName = "in_reply_to_status_id_str")]
        public string InReplyToStatusIdStr { get; set; }

        [JsonProperty(PropertyName = "in_reply_to_user_id")]
        public int InReplyToUserId { get; set; }

        [JsonProperty(PropertyName = "in_reply_to_user_id_str")]
        public string InReplyToUserIdStr { get; set; }

        public Place Place { get; set; }

        [JsonProperty(PropertyName = "retweet_count")]
        public int RetweetCount { get; set; }

        public bool Retweeted { get; set; }
        public string Source { get; set; }
        public string Text { get; set; }
        public bool Truncated { get; set; }
    }

    public class VerifyCredentialsResult
    {
        [JsonProperty(PropertyName = "contributors_enabled ")]
        public bool ContributorsEabled { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty(PropertyName = "default_profile")]
        public bool DefaultProfile { get; set; }

        [JsonProperty(PropertyName = "default_profile_image")]
        public bool DefaultProfileImage { get; set; }

        public string Description { get; set; }

        [JsonProperty(PropertyName = "favourites_count")]
        public int FavouritesCount { get; set; }

        [JsonProperty(PropertyName = "follow_request_sent")]
        public bool FollowRequestSent { get; set; }

        [JsonProperty(PropertyName = "followers_count")]
        public int FollowersCount { get; set; }

        public bool Following { get; set; }

        [JsonProperty(PropertyName = "friends_count")]
        public int FriendsCount { get; set; }

        [JsonProperty(PropertyName = "geo_enabled")]
        public bool GeoEnabled { get; set; }

        public int Id { get; set; }

        [JsonProperty(PropertyName = "id_str")]
        public string IdStr { get; set; }

        [JsonProperty(PropertyName = "is_translator")]
        public bool IsTranslator { get; set; }

        public string Lang { get; set; }

        [JsonProperty(PropertyName = "listed_count")]
        public int ListedCount { get; set; }

        public string Location { get; set; }
        public string Name { get; set; }
        public bool Notifications { get; set; }

        [JsonProperty(PropertyName = "profile_background_color")]
        public string ProfileBackgroundColor { get; set; }

        [JsonProperty(PropertyName = "profile_background_image_url")]
        public string ProfileBackgroundImageUrl { get; set; }

        [JsonProperty(PropertyName = "profile_background_image_url_https")]
        public string ProfileBackgroundImageUrlHttps { get; set; }

        [JsonProperty(PropertyName = "profile_background_tile")]
        public bool ProfileBackgroundTile { get; set; }

        [JsonProperty(PropertyName = "profile_image_url")]
        public string ProfileImageUrl { get; set; }

        [JsonProperty(PropertyName = "profile_image_url_https")]
        public string ProfileImageUrlHttps { get; set; }

        [JsonProperty(PropertyName = "profile_link_color")]
        public string ProfileLinkColor { get; set; }

        [JsonProperty(PropertyName = "profile_sidebar_border_color")]
        public string ProfileSidebarBorderColor { get; set; }

        [JsonProperty(PropertyName = "profile_sidebar_fill_color")]
        public string ProfileSidebarFillColor { get; set; }

        [JsonProperty(PropertyName = "profile_text_color")]
        public string ProfileTextColor { get; set; }

        [JsonProperty(PropertyName = "profile_use_background_image")]
        public bool ProfileUseBackgroundImage { get; set; }

        public bool Protected { get; set; }

        [JsonProperty(PropertyName = "screen_name")]
        public string ScreenName { get; set; }

        [JsonProperty(PropertyName = "show_all_inline_media")]
        public bool ShowAllInlineMedia { get; set; }

        public Status Status { get; set; }

        [JsonProperty(PropertyName = "statuses_count")]
        public int StatusesCount { get; set; }

        [JsonProperty(PropertyName = "time_zone")]
        public string TimeZone { get; set; }

        public string Url { get; set; }

        [JsonProperty(PropertyName = "utc_offset")]
        public int UtcOffset { get; set; }

        public bool Verified { get; set; }
    }
}