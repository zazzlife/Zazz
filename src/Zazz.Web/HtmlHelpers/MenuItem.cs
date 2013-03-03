using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Zazz.Web.HtmlHelpers
{
    public static class MenuItem
    {
        public static MvcHtmlString MenuLink(
            this HtmlHelper htmlHelper,
            string linkText,
            string actionLink,
            string controllerName,
            params string[] actions)
        {
            var currentController = htmlHelper.ViewContext.RouteData.GetRequiredString("controller");
            var currentAction = htmlHelper.ViewContext.RouteData.GetRequiredString("action");

            var li = new TagBuilder("li");

            if (controllerName.Equals(currentController, StringComparison.InvariantCultureIgnoreCase))
            {
                var isMatch =
                    actions.Any(action => currentAction.Equals(action, StringComparison.InvariantCultureIgnoreCase));

                if (isMatch)
                    li.AddCssClass("active");
            }

            li.InnerHtml = htmlHelper.ActionLink(linkText, actionLink, controllerName).ToHtmlString();
            return MvcHtmlString.Create(li.ToString());
        }
    }
}