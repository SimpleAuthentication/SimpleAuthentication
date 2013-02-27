using Nancy;
using Nancy.Authentication.WorldDomination;
using WorldDomination.Web.Authentication.Samples.NancyFX2.Model;

namespace WorldDomination.Web.Authentication.Samples.NancyFX2.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get["/"] = parameters => View["login", new IndexViewModel()];
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