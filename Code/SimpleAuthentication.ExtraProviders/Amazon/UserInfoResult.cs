namespace SimpleAuthentication.ExtraProviders.Amazon
{
    public class UserInfoResult
    {
        public string RequestId { get; set; }

        public ProfileResult Profile { get; set; }

        public class ProfileResult
        {
            public string CustomerId { get; set; }
            public string PrimaryEmail { get; set; }
            public string Name { get; set; }
        }
    }
}