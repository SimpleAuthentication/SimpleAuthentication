namespace WorldDomination.Web.Authentication
{
    public interface IFakeAuthenticationProvider : IAuthenticationProvider
    {
        string RedirectToAuthenticateExceptionMessage { set; }
        UserInformation UserInformation { set; }
        string AuthenticateClientExceptionMessage { set; }
    }
}