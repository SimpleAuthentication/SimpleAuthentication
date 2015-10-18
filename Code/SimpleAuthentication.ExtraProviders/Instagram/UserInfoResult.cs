namespace SimpleAuthentication.ExtraProviders.Instagram
{
    public class UserInfoResult
    {
        public UserData Data { get; set; }

        public class UserData
        {
            public string Id { get; set; }
            public string Username { get; set; }
            public string FullName { get; set; }
            public string ProfilePicture { get; set; }
            public string Bio { get; set; }
            public string Website { get; set; }
        }
    }
}