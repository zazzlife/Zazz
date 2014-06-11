using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Zazz.Infrastructure.Helpers;

namespace Zazz.Web.Helpers
{
    public static class HtmlHelpers
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

        public static MvcHtmlString RadioButtonForEnum<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, TProperty>> expression)
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

        public static MvcHtmlString RadioButtonForEnum2<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, TProperty>> expression)
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
                    "{1}<label class=\"radio\" for=\"{0}\">{2}</label>",
                    id,
                    radio,
                    HttpUtility.HtmlEncode(label)
                );
            }
            return MvcHtmlString.Create(sb.ToString());
        }
 
        public static string GetEnumDisplay<TEnum>(this TEnum en)
        {
            var fieldInfo = en.GetType().GetField(en.ToString());
            var displayAttribute = fieldInfo.GetCustomAttributes(typeof (DisplayAttribute), false)
                                            .OfType<DisplayAttribute>()
                                            .FirstOrDefault();

            return displayAttribute == null ? en.ToString() : displayAttribute.GetName();
        }

        public static SelectList EnumToSelectList<TEnum>(this TEnum enumobj)
        {
            var values = from TEnum e in Enum.GetValues(typeof (TEnum))
                         select new { Id = e, Name = GetEnumDisplay(e) };

            return new SelectList(values, "Id", "Name", enumobj);
        }

        public static IHtmlString DisplayColumnNameFor<TModel, TClass, TProperty>
            (this HtmlHelper<TModel> helper, IEnumerable<TClass> model,
             Expression<Func<TClass, TProperty>> expression)
        {
            var name = ExpressionHelper.GetExpressionText(expression);
            name = helper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(
                () => Activator.CreateInstance<TClass>(), typeof (TClass), name);

            return new MvcHtmlString(metadata.DisplayName ?? name);
        }

        public static MvcHtmlString RelativeTime(this HtmlHelper helper, DateTime time, object htmlAttributes = null)
        {
            var tag = new TagBuilder("time");
            tag.Attributes.Add("data-livestamp", time.ToString("s") + "z");
            tag.SetInnerText(time.ToRelativeTime());

            if (htmlAttributes != null)
                tag.MergeAttributes(new RouteValueDictionary(htmlAttributes));

            return MvcHtmlString.Create(tag.ToString());
        }
    }
}