using System.Web;
using System.Web.Mvc;
using Zazz.Web.Filters;

namespace Zazz.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new CustomHandleErrorAttribute());
        }
    }
}