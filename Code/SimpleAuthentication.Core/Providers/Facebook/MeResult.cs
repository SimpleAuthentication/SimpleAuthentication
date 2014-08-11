using Newtonsoft.Json;

namespace SimpleAuthentication.Core.Providers.Facebook
{
    public class MeResult
    {
        public long Id { get; set; }
        public string Name { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }
        public string Link { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public long Timezone { get; set; }
        public string Locale { get; set; }
        public bool Verified { get; set; }
        [JsonProperty("updated_time")]
        public string UpdatedTime { get; set; }
    }
}