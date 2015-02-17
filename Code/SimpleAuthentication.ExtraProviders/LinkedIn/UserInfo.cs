using Newtonsoft.Json;

namespace SimpleAuthentication.ExtraProviders.LinkedIn
{
    internal class UserInfo
    {
        [JsonProperty(PropertyName = "emailAddress")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "formattedName")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "pictureUrl")]
        public string AvatarUrl { get; set; }
    }
}