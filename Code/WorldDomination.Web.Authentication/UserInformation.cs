namespace WorldDomination.Web.Authentication
{
    public class UserInformation
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Locale { get; set; }
        public string Picture { get; set; }
        public GenderType Gender { get; set; }
    }
}