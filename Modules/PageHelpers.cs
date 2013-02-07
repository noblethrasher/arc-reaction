using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

namespace ArcReaction
{
    public static class PageHelpers
    {
        public static IHttpHandler GetCompiledPageInstance(this string path, HttpContext context)
        {
            return System.Web.UI.PageParser.GetCompiledPageInstance(path, context.Server.MapPath(path), context);
        }

        public static IHttpHandler GetCompiledPageInstance(this string path)
        {
            var context = HttpContext.Current;

            return PageParser.GetCompiledPageInstance(path, context.Server.MapPath(path), context);
        }

        public static T GetCompiledPageInstance<T>(this string path)
        {
            return (T)path.GetCompiledPageInstance();
        }

        public static T GetCompiledPageInstance<T>(this string path, HttpContext context)
        {
            return (T)path.GetCompiledPageInstance(context);
        }
    }
}
