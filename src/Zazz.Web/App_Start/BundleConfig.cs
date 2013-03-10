using System.Web;
using System.Web.Optimization;

namespace Zazz.Web
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            var css = new StyleBundle("~/Content/css");
            css.IncludeDirectory("~/Content/", "*.css", false);

            var js = new ScriptBundle("~/js");
            js.IncludeDirectory("~/Scripts/", "*.js", true);

            var jqueryUiCss = new StyleBundle("~/Content/themes/base/css",
                                              "http://code.jquery.com/ui/1.10.1/themes/base/jquery-ui.css")
                .Include(
                    "~/Content/themes/base/jquery.ui.core.css",
                    "~/Content/themes/base/jquery.ui.autocomplete.css",
                    "~/Content/themes/base/jquery.ui.selectable.css",
                    "~/Content/themes/base/jquery.ui.slider.css",
                    "~/Content/themes/base/jquery.ui.datepicker.css",
                    "~/Content/themes/base/jquery.ui.theme.css");

            bundles.Add(css);
            bundles.Add(jqueryUiCss);
            bundles.Add(js);
        }
    }
}