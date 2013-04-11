using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Helpers;
using WorldDomination.Web.Authentication.Csrf;

using SystemWebAntiForgery = System.Web.Helpers.AntiForgery;
using AntiForgery = WorldDomination.Web.Authentication.Csrf.AntiForgery;

namespace WorldDomination.Web.Authentication.Mvc
{
    // Implements IAntiForgery using ASP.Net's built-in AntiForgery stuff
    // In order to remain compatible with the existing AntiForgery cookies, we store extra data in the _sent_ token, not the kept token.
    public class AspNetAntiForgery : AntiForgery
    {
        public override string DefaultCookieName
        {
            get { return AntiForgeryConfig.CookieName; }
        }

        protected override void GenerateTokens(string existingToKeepToken, out string toSend, out string toKeep)
        {
            SystemWebAntiForgery.GetTokens(existingToKeepToken, out toKeep, out toSend);
            toKeep = toKeep ?? existingToKeepToken; // toKeep == null if the existing token is A-OK
        }

        protected override void EncodeExtraData(string extraData, ref string toKeep, ref string toSend)
        {
            toSend = GenerateExtraData(extraData, toSend);
        }

        protected override void SelectTokens(string keptToken, string recievedToken, out string pureToken, out string tokenWithExtraData)
        {
            pureToken = keptToken;
            tokenWithExtraData = recievedToken;
        }

        protected override void CheckToken(string pureToken, string state)
        {
            SystemWebAntiForgery.Validate(pureToken, state);
        }
    }
}