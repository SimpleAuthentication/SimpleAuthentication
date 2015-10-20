namespace SimpleAuthentication.Core.Providers.Google
{
    public class Name
    {
        public string FamilyName { get; set; }
        public string GivenName { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", GivenName, FamilyName).Trim();
        }
    }
}
