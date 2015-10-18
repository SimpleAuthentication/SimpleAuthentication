namespace SimpleAuthentication.ExtraProviders.ThirtySevenSignals
{
    public class PersonResult
    {
        public long Id { get; set; }
        public string identity_id { get; set; }
        public string Name { get; set; }
        public string email_address { get; set; }
        public bool Admin { get; set; }
        public string avatar_url { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
    }
}