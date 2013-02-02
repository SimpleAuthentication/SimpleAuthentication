using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldDomination.Web.Authentication.ExtraProviders
{
    public class WindowsLiveProvider : IAuthenticationProvider
    {
        public string Name { get; private set; }
        public Uri CallBackUri { get; private set; }
        public IAuthenticationServiceSettings DefaultAuthenticationServiceSettings { get; private set; }

        public Uri RedirectToAuthenticate(IAuthenticationServiceSettings authenticationServiceSettings)
        {
            throw new NotImplementedException();
        }

        public IAuthenticatedClient AuthenticateClient(NameValueCollection parameters, string existingState)
        {
            throw new NotImplementedException();
        }
    }
}
