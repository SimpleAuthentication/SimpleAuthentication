namespace Nancy.Authentication.WorldDomination
{
    public interface IAuthenticationCallbackProvider
    {
        ProcessResult Process(NancyContext nancyContext, AuthenticateCallbackData model);
    }
}