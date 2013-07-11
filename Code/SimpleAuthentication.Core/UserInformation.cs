namespace SimpleAuthentication.Core
{
    /// <summary>
    /// Common user information.
    /// </summary>
    public class UserInformation
    {
        /// <summary>
        /// Unique Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name. (Usually their real or common name).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Alias or Display name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Culture locale (eg. en-au).
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// Url to a avatar/picture.
        /// </summary>
        public string Picture { get; set; }

        /// <summary>
        /// Their gender.
        /// </summary>
        public GenderType Gender { get; set; }

        public override string ToString()
        {
            return string.Format("Id: {0}. Name: {1}. UserName : {2}. Email: {3}.",
                                 string.IsNullOrEmpty(Id) ? "-no Id-" : Id,
                                 string.IsNullOrEmpty(Name) ? "-no name-" : Name,
                                 string.IsNullOrEmpty(UserName) ? "-no user name-" : UserName,
                                 string.IsNullOrEmpty(Email) ? "-no email-" : Email);
        }

        public string ToLongString()
        {
            return string.Format("{0} Locale: {1}. Picture: {2}. Gender {3}.",
                                 ToString(),
                                 string.IsNullOrEmpty(Locale) ? "-no locale-" : Locale,
                                 string.IsNullOrEmpty(Picture) ? "-no picture-" : Picture,
                                 Gender);
        }
    }
}