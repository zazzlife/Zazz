using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Zazz.Core.Interfaces;
using Zazz.Infrastructure.Helpers;

namespace Zazz.Web.Helpers
{
    public static class HtmlHelpers
    {
        private static readonly IStringHelper StringHelper = new StringHelper();

        public static MvcHtmlString WrapTagsInAnchorTags(this HtmlHelper htmlHelper, string text)
        {
            var tags = StringHelper.ExtractTags(text);
            const string BASE_ADDRESS = "/home/tags?select=";
            
            var tagBuilder = new TagBuilder("a");
            tagBuilder.AddCssClass("tag");
            
            foreach (var tag in tags)
            {
                var link = BASE_ADDRESS + tag.Replace("#", "");
                
                tagBuilder.MergeAttribute("href", link);
                tagBuilder.SetInnerText(tag);

                text = text.Replace(tag, tagBuilder.ToString());
            }

            var t = MvcHtmlString.Create(text);
            return t;
        }

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

        public static MvcHtmlString RadioButtonForEnum<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, TProperty>> expression
    )
        {
            var metaData = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            if (!metaData.ModelType.IsEnum)
            {
                throw new ArgumentException("This helper is intended to be used with enum types");
            }

            var names = Enum.GetNames(metaData.ModelType);
            var sb = new StringBuilder();

            var fields = metaData.ModelType.GetFields(
                BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public
            );

            foreach (var name in names)
            {
                var id = string.Format(
                    "{0}_{1}_{2}",
                    htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix,
                    metaData.PropertyName,
                    name
                );
                var radio = htmlHelper.RadioButtonFor(expression, name, new { id }).ToHtmlString();
                var field = fields.Single(f => f.Name == name);
                var label = name;
                var display = field
                    .GetCustomAttributes(typeof(DisplayAttribute), false)
                    .OfType<DisplayAttribute>()
                    .FirstOrDefault();
                if (display != null)
                {
                    label = display.GetName();
                }

                sb.AppendFormat(
                    "<label class=\"radio\" for=\"{0}\">{1}  {2}</label>",
                    id,
                    radio,
                    HttpUtility.HtmlEncode(label)
                );
            }
            return MvcHtmlString.Create(sb.ToString());
        } 
    }
}