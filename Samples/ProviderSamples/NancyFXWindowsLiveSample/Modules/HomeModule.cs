using Nancy;
using Nancy.Authentication.WorldDomination;
using NancyFXWindowsLiveSample.Model;

namespace NancyFXWindowsLiveSample.Modules
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
        public dynamic Process(NancyModule nancyModule, AuthenticateCallbackData model)
        {
            return nancyModule.Negotiate.WithView("AuthenticateCallback").WithModel(model);
        }

        public dynamic OnRedirectToAuthenticationProviderError(NancyModule nancyModule, string errorMessage)
        {
            var model = new IndexViewModel
                        {
                            ErrorMessage = errorMessage
                        };

            return nancyModule.Negotiate.WithView("login").WithModel(model);
        }
    }
}