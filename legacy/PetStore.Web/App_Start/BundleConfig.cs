using System.Collections.Generic;
using System.Text;

namespace PetStore
{
    // poor man's System.Web.Optimization replacement
    public class BundleConfig
    {
        public static Dictionary<string, string[]> Bundles = new Dictionary<string, string[]>();

        public static void RegisterBundles()
        {
            Bundles["~/bundles/jquery"] = new[] { "/Scripts/jquery-1.12.4.min.js" };
            Bundles["~/bundles/jqueryval"] = new[] { "/Scripts/jquery.validate.min.js", "/Scripts/jquery.validate.unobtrusive.min.js" };
            Bundles["~/bundles/bootstrap"] = new[] { "/Scripts/bootstrap.min.js" };
            Bundles["~/bundles/react"] = new[] {
                "/Scripts/vendor/react.development.js",
                "/Scripts/vendor/react-dom.development.js",
                "/Scripts/vendor/redux.min.js",
                "/Scripts/vendor/react-redux.min.js",
                "/Scripts/vendor/redux-saga.umd.min.js",
            };
            Bundles["~/bundles/babel"] = new[] { "/Scripts/vendor/babel.min.js" };
            Bundles["~/Content/css"] = new[] { "/Content/bootstrap.min.css", "/Content/Site.css" };
        }

        public static string Render(string name)
        {
            if (!Bundles.ContainsKey(name)) RegisterBundles();
            var sb = new StringBuilder();
            foreach (var f in Bundles[name])
            {
                if (f.EndsWith(".css"))
                    sb.Append("<link rel=\"stylesheet\" href=\"" + f + "?v=3\" />\n");
                else
                    sb.Append("<script src=\"" + f + "?v=3\"></script>\n");
            }
            return sb.ToString();
        }
    }
}
