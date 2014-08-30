using System.Collections.Generic;

namespace SimpleAuthentication.Core.Providers.Google
{
    internal class UserInfoResult
    {
        public string Id { get; set; }
        public ICollection<Emails> Emails { get; set; }
        public bool VerifiedEmail { get; set; }
        public Name Name { get; set; }
        public string DisplayName { get; set; }
        public Image Image { get; set; }
        public string Gender { get; set; }
        public string Language { get; set; }
    }
}