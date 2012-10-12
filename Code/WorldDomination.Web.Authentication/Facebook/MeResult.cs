using Newtonsoft.Json;

namespace WorldDomination.Web.Authentication.Facebook
{
    public class MeResult
    {
        public long Id { get; set; }
        public string Name { get; set; }

        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; set; }

        public string Link { get; set; }
        public string Username { get; set; }
        public int Timezone { get; set; }
        public string Locale { get; set; }
        public bool Verified { get; set; }

        [JsonProperty(PropertyName = "updated_time")]
        public string UpdatedTime { get; set; }
    }
}