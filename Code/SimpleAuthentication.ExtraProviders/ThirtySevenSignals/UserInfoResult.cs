using System.Collections.Generic;

namespace SimpleAuthentication.ExtraProviders.ThirtySevenSignals
{
    public class UserInfoResult
    {
        public string expires_at { get; set; }
        public Identity Identity { get; set; }
        public List<Account> Accounts { get; set; }
    }
}