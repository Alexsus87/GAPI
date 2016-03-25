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
						"~/Content/plugins/jquery.themepunch.tools.min.js",
						"~/Content/plugins/jquery.themepunch.revolution.min.js",
						"~/Content/plugins/isotope.pkgd.min.js",
						"~/Content/plugins/jquery.magnific-popup.min.js",
						"~/Content/plugins/jquery.waypoints.min.js",
						"~/Content/plugins/jquery.parallax-1.1.3.js",
						"~/Content/plugins/jquery.validate.js",
						"~/Content/plugins/morphext.min.js",
						"~/Content/plugins/jquery.vide.js",
						"~/Content/plugins/owl.carousel.js",
						"~/Content/plugins/jquery.browser.js",
						"~/Content/plugins/SmoothScroll.js",
						"~/Scripts/template.js",
						"~/Scripts/custom.js"));

			bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
						"~/Scripts/jquery.validate*"));

			// Use the development version of Modernizr to develop with and learn from. Then, when you're
			// ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
			bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
						"~/Scripts/modernizr-*"));

			bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
				"~/Scripts/bootstrap.js",
				"~/Scripts/bootstrap-datepicker.js",
				"~/Scripts/respond.js"));
			//  "~/Scripts/npm.js"));

			bundles.Add(new StyleBundle("~/Content/css").Include(
					  "~/fonts/font-awesome/css/font-awesome.css",
					  "~/fonts/fontello/css/fontello.css",
					  "~/Content/bootstrap.css",
					  "~/Content/style.css",
					  "~/Content/bootstrap-datepicker.css",
					  "~/Content/site.css",
					  "~/Content/animations.css",
					  "~/Content/jquery-ui.css",
					  "~/Content/jquery.datetimepicker.css",
					  "~/Content/bootstrap-theme.css"));
		}
	}
}
