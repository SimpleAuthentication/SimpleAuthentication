namespace SimpleAuthentication.Sample.NancyAuto.Helpers
{
    using Models;
    using Nancy;
    using Nancy.SimpleAuthentication;

    public class SampleAuthenticationCallbackProvider : IAuthenticationCallbackProvider
    {
        public dynamic Process(NancyModule nancyModule, AuthenticateCallbackData model)
        {
            return nancyModule.View["AuthenticateCallback", model];
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