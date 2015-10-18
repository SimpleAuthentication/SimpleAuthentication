namespace SimpleAuthentication.Core
{
    /// <summary>
    /// Defines the contract that a Fake Authentication Provider must impliemnt. It provides some optional setters which can be used for testing or just quick development without the requirement to integrate with the real internet.
    /// </summary>
    public interface IFakeAuthenticationProvider : IAuthenticationProvider
    {
        /// <summary>
        /// (Optional) Error message if the redirect is suppose to fail. Set this if you wish to test handling an exception during the RedirectToAuthentication phase.
        /// </summary>
        /// <remarks>If this is not set, then no exception will be thrown during the RedirectToAuthenticate phase.</remarks>
        string RedirectToAuthenticateExceptionMessage { set; }

        /// <summary>
        /// (Optional) Some fake user information that is retrieved.
        /// </summary>
        /// <remarks>If this is not set, then some hardcoded, default UserInformation data will be used.</remarks>
        UserInformation UserInformation { set; }

        /// <summary>
        /// (Optional) Fake error message if while trying to retrieve the Authenticate Client details, errored. Set this if you wish to test handling an exception during the AuthenticateClient phase.
        /// </summary>
        /// <remarks>If this is not set, then no exception will be thrown during the AuthenticateClient phase.</remarks>
        string AuthenticateClientExceptionMessage { set; }
    }
}