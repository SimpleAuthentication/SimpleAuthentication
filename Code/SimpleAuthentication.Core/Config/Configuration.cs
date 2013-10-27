using System.Collections.Generic;

namespace SimpleAuthentication.Core.Config
{
    public class Configuration
    {
        public string RedirectRoute { get; set; }
        public string CallBackRoute { get; set; }
        public IList<Provider> Providers { get; set; }
    }
}