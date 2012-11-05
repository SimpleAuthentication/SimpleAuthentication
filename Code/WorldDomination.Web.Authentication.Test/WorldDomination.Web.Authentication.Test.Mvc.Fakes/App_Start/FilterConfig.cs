using System.Web.Mvc;

namespace WorldDomination.Web.Authentication.Test.Mvc.Fakes.App_Start
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}