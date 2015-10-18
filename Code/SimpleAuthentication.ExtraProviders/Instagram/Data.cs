using Newtonsoft.Json;

namespace SimpleAuthentication.ExtraProviders.Instagram
{
    internal class Data
    {
        public string Id { get; set; }
        public string UserName { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("profile_picture")]
        public string ProfilePicture { get; set; }
    }
}