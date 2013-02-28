using System.Web;
using System.Web.Mvc;

namespace WorldDomination.Web.Authentication.Mvc
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
    }
}