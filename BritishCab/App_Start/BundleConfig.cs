using System.Web;
using System.Web.Optimization;

namespace BritishCab
{
	public class BundleConfig
	{
		// For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
						"~/Scripts/jquery-{version}.js",
						"~/Scripts/jquery-ui-{version}.js",
						"~/Scripts/jquery.datetimepicker.js",
						"~/Content/plugins/jquery.validate.js",
						"~/Content/plugins/owl.carousel.js",
						"~/Content/plugins/jquery.browser.js",
						"~/Content/plugins/SmoothScroll.js",
						"~/Scripts/template.js",
						"~/Scripts/custom.js",
						"~/Content/plugins/rs-plugin/js/jquery.themepunch.tools.min.js",
						"~/Content/plugins/rs-plugin/js/jquery.themepunch.revolution.min.js"));

			bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
						"~/Scripts/jquery.validate*"));

			// Use the development version of Modernizr to develop with and learn from. Then, when you're
			// ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
			bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
						"~/Scripts/modernizr-*"));

			bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
				"~/Scripts/bootstrap.js",
				"~/Scripts/respond.js"));
			//  "~/Scripts/npm.js"));

			bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/themes/bootstrap.css",
					  "~/fonts/fontello/css/fontello.css",
                      "~/fonts/font-awesome/css/font-awesome.css",
					  "~/Content/plugins/rs-plugin/css/settings.css",
                      "~/Content/themes/animations.css",
                      "~/Content/themes/style.css",
                      "~/Content/themes/jquery-ui.css",
                      "~/Content/themes/jquery.datetimepicker.css",
                      "~/Content/plugins/rs-plugin/css/setting.css",
                      "~/Content/themes/bootstrap-theme.css",
                      "~/Content/themes/light_blue.css",
                      "~/Content/themes/site.css",
                      "~/Content/themes/custom.css"));
		}
	}
}
