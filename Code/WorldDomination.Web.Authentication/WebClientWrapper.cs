using System.Net;
using CuttingEdge.Conditions;

namespace WorldDomination.Web.Authentication
{
    public class WebClientWrapper : IWebClientWrapper
    {
        #region Implementation of IWebClientWrapper

        public string DownloadString(string address)
        {
            Condition.Requires(address);

            var webclient = new WebClient();
            return webclient.DownloadString(address);
        }

        #endregion
    }
}