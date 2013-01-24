namespace WorldDomination.Web.Authentication
{
    /// <summary>
    /// Defines the contract that a Fake Authentication Provider must impliemnt.
    /// </summary>
    public interface IFakeAuthenticationProvider : IAuthenticationProvider
    {
        /// <summary>
        /// Error message if the redirect is suppose to fail.
        /// </summary>
        string RedirectToAuthenticateExceptionMessage { set; }

        /// <summary>
        /// Some fake user information that is retrieved.
        /// </summary>
        UserInformation UserInformation { set; }

        /// <summary>
        /// Fake error message if while trying to retrieve the Authenticate Client details, errored.
        /// </summary>
        string AuthenticateClientExceptionMessage { set; }
    }
}