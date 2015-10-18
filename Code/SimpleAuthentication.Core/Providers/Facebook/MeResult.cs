namespace SimpleAuthentication.Core.Providers.Facebook
{
    public class MeResult
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Link { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public long Timezone { get; set; }
        public string Locale { get; set; }
        public bool Verified { get; set; }
        public string UpdatedTime { get; set; }
    }
}