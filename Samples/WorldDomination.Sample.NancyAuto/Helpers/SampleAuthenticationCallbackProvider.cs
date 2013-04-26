using Nancy;
using Nancy.Authentication.WorldDomination;
using WorldDomination.Sample.NancyAuto.Modules;

namespace WorldDomination.Sample.NancyAuto.Helpers
{
    public class SampleAuthenticationCallbackProvider : IAuthenticationCallbackProvider
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

            return nancyModule.View["index", model];
        }
    }
}