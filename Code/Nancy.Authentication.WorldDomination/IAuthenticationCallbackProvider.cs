namespace Nancy.Authentication.WorldDomination
{
    public interface IAuthenticationCallbackProvider
    {
        ProcessResult Process(AuthenticateCallbackData model);
    }
}