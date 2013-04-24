using Nancy;
using Nancy.Authentication.WorldDomination;

namespace WorldDomination.Sample.NancyAuto.Modules
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