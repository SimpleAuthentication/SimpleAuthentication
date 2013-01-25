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
        public ProcessResult Process(NancyContext nancyContext, AuthenticateCallbackData model)
        {
            return new ProcessResult(ProcessResult.ActionType.RenderView)
            {
                View = "AuthenticateCallback.cshtml",
                ViewModel = model
            };
        }
    }
}