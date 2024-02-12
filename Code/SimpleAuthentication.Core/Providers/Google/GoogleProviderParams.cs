using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleAuthentication.Core.Providers.Google
{
    public class GoogleProviderParams : ProviderParams
    {
        public string PromptType { get; set; }
    }
}
