using System;
using System.Collections.Specialized;
using Nancy;
using WorldDomination.Web.Authentication.Test.NancyFX2.Model;

namespace WorldDomination.Web.Authentication.Test.NancyFX2.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get["/"] = parameters => View["login"];
            Get["/logged-in"] = parameters =>
            {
                return "Logged in!!!";
            };
        }
    }
}