﻿using System.Web.Mvc;

namespace WorldDomination.Sample.MvcAuto.App_Start
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}