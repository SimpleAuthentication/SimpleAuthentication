using Newtonsoft.Json;

namespace SimpleAuthentication.Core.Providers.WindowsLive
{
    internal class UserInfoResult
    {
        public string Id { get; set; }
        public string Name { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName{ get; set; }
        public string Link { get; set; }
        public string Gender { get; set; }
        public string Locale { get; set; }
        [JsonProperty("updated_time ")]
        public string UpdatedTime { get; set; }
        public Emails Emails { get; set; }
    }
}