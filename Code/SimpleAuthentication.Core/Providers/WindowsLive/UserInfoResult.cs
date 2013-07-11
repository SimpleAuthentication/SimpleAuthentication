namespace SimpleAuthentication.Core.Providers.WindowsLive
{
    public class UserInfoResult
    {
        public string id { get; set; }
        public string name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string link { get; set; }
        public string gender { get; set; }
        public string locale { get; set; }
        public string updated_time { get; set; }
        public Emails emails { get; set; }
    }
}