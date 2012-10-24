using System.Web;
using System.Web.Mvc;

namespace WorldDomination.Web.IntegrationTest
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}