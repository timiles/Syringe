﻿using System.Web.Mvc;

namespace Syringe.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
			filters.Add(new LogExceptionsAttribute());
		}
    }
}
