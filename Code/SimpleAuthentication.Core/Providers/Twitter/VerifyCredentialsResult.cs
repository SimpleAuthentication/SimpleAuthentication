using System.Collections.Generic;

namespace SimpleAuthentication.Core.Providers.Twitter
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
        //public BoundingBox BoundingBox { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string FullName { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string PlaceType { get; set; }
        public string Url { get; set; }
    }

    public class Status
    {
        public object Contributors { get; set; }
        public Geo Coordinates { get; set; }
        public string CreatedAt { get; set; }
        public bool Favorited { get; set; }
        public Geo Geo { get; set; }
        public long Id { get; set; }
        public string Idtr { get; set; }
        public string InReplyToScreenName { get; set; }
        public long InReplyToStatusId { get; set; }
        public string InReplyToStatusIdStr { get; set; }
        public long InReplyToUserId { get; set; }
        public string InReplyToUserIdStr { get; set; }
        public Place Place { get; set; }
        public long RetweetCount { get; set; }
        public bool Retweeted { get; set; }
        public string Source { get; set; }
        public string Text { get; set; }
        public bool Truncated { get; set; }
    }

    public class VerifyCredentialsResult
    {
        public bool ContributorsEabled { get; set; }
        public string CreatedAt { get; set; }
        public bool DefaultProfile { get; set; }
        public bool DefaultProfileImage { get; set; }
        public string Description { get; set; }
        public long FavouritesCount { get; set; }
        public bool? FollowRequestSent { get; set; }
        public long FollowersCount { get; set; }
        public bool? Following { get; set; }
        public long FriendsCount { get; set; }
        public bool GeoEnabled { get; set; }
        public long Id { get; set; }
        public string IdStr { get; set; }
        public bool IsTranslator { get; set; }
        public string Lang { get; set; }
        public long ListedCount { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
        public bool? Notifications { get; set; }
        public string ProfileBackgroundColor { get; set; }
        public string ProfileBackgroundImageUrl { get; set; }
        public string ProfileBackgroundImageUrlHttps { get; set; }
        public bool ProfileBackgroundTile { get; set; }
        public string ProfileImageUrl { get; set; }
        public string ProfileImageUrlHttps { get; set; }
        public string ProfileLinkColor { get; set; }
        public string ProfileSidebarBorderColor { get; set; }
        public string ProfileSidebarFillColor { get; set; }
        public string ProfileTextColor { get; set; }
        public bool ProfileUseBackgroundImage { get; set; }
        public bool Protected { get; set; }
        public string ScreenName { get; set; }
        public bool ShowAllInlineMedia { get; set; }
        public Status Status { get; set; }
        public long StatusesCount { get; set; }
        public string TimeZone { get; set; }
        public string Url { get; set; }
        public long UtcOffset { get; set; }
        public bool Verified { get; set; }
    }
}