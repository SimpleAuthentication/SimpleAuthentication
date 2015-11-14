using System.Collections.Generic;

namespace SimpleAuthentication.Core.Providers.Google
{
    public class UserInfoResult
    {
        public string Id { get; set; }
        public List<Email> Emails { get; set; }
        public string DisplayName { get; set; }
        public Name Name  { get; set; }
        public string Url { get; set; }
        public Image Image { get; set; }
        public string Gender { get; set; }
        public string Language { get; set; }
    }
}