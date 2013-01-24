namespace WorldDomination.Web.Authentication
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
    }
}