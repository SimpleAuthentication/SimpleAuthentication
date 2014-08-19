using Newtonsoft.Json;

namespace SimpleAuthentication.ExtraProviders.GitHub
{
    internal class UserInfo
    {
        public string Id { get; set; }
        public string Login { get; set; }
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }
        [JsonProperty("gravatar_id")]
        public string GravatarId { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public string Blog { get; set; }
        public string Location { get; set; }
        public string Email { get; set; }
    }
}