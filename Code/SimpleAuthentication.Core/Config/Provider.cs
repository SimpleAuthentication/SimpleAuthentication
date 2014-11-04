namespace SimpleAuthentication.Core.Config
{
    public class Provider
    {
        public string Name { get; set; }
        public string Secret { get; set; }
        public string Key { get; set; }
        public string Scopes { get; set; }

        public override string ToString()
        {
            return string.Format("Provider: {0}", 
                string.IsNullOrWhiteSpace(Name)
                ? "--no Name--"
                : Name);
        }
    }
}