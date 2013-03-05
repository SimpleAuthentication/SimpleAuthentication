namespace WorldDomination.Web.Authentication.Csrf
{
    public interface IAntiForgery
    {
        string CreateToken(string extraData = null);

        TokenData ValidateToken(string token);
    }
}