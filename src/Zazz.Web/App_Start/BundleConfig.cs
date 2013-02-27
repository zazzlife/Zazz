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
            js.IncludeDirectory("~/Scripts/", "*.js", false);

            bundles.Add(css);
            bundles.Add(js);
        }
    }
}