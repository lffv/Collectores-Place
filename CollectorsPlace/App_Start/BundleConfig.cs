using System.Web;
using System.Web.Optimization;

namespace CollectorsPlace
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/recs").Include(
                        "~/Scripts/recomendations.js"));

            bundles.Add(new ScriptBundle("~/bundles/searchs").Include(
                        "~/Scripts/search.js"));

            bundles.Add(new ScriptBundle("~/bundles/upload").Include(
                        "~/Scripts/jquery.uploadify.js"));

            bundles.Add(new ScriptBundle("~/bundles/tagit").Include(
                        "~/Scripts/tag-it.js",
                        "~/Scripts/tags.js"));

            /* Ajax calls */
            bundles.Add(new ScriptBundle("~/bundles/ajaxcalls").Include(
                        "~/Scripts/ajaxcalls.js"));

            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                        "~/Scripts/jquery.dataTables.js",
                        "~/Scripts/tabelas.js"));


            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/Content/site.css",
                        "~/Content/jquery.tagit.css",
                        "~/Content/tagit.ui-zendesk.css",
                        "~/Content/uploadify.css",
                        "~/Content/tabela/jquery-ui.css",
                        "~/Content/tabela/table_jui.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));
        }
    }
}