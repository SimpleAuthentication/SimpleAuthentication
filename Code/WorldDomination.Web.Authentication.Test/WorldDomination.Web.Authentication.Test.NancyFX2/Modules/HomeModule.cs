using System;
using Nancy;
using Nancy.Authentication.WorldDomination;

namespace WorldDomination.Web.Authentication.Test.NancyFX2.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get["/"] = parameters => View["login"];
            Get["/logged-in"] = parameters => { return "Logged in!!!"; };
        }
    }

    public class Test : IAuthenticationCallbackProvider
    {
        public ProcessResult Process(AuthenticateCallbackData model)
        {
            throw new NotImplementedException();
        }
    }
}