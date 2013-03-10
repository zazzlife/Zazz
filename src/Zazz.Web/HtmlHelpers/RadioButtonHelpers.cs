using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Zazz.Web.HtmlHelpers
{
    public static class RadioButtonHelpers
    {
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