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
        }
    }

    public class Test : IAuthenticationCallbackProvider
    {
        public ProcessResult Process(AuthenticateCallbackData model)
        {
            return new ProcessResult(ProcessResult.ActionEnum.RenderView)
            {
                View = "AuthenticateCallback.cshtml",
                ViewModel = model
            };
        }
    }
}