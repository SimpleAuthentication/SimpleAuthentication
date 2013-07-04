using System.Web;
using System.Web.Mvc;

namespace SimpleAuthentication.Mvc
{
    /// <summary>
    ///     Defines the contract for the callback from an Authentication Provider.
    /// </summary>
    public interface IAuthenticationCallbackProvider
    {
        /// <summary>
        ///     Custom processing during the callback from an Authentication Provider.
        /// </summary>
        /// <param name="context">The current HttpContext passed from the controller.</param>
        /// <param name="model">Some data related to the callback, such as an error or some authenticated user data.</param>
        /// <returns>An action result - a redirect or content or whatever :)</returns>
        ActionResult Process(HttpContextBase context, AuthenticateCallbackData model);

        //TODO: As a major version update we will implement error handling in
        ///// <summary>
        /////     Custom error handling from the callback of a Authentication Provider
        ///// </summary>
        ///// <param name="context"></param>
        ///// <param name="Model"></param>
        ///// <returns></returns>
        //ActionResult Process(HttpContextBase context, AuthenticateCallbackError model);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        ActionResult OnRedirectToAuthenticationProviderError(HttpContextBase context, string errorMessage);
    }
}