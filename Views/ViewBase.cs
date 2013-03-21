using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace ArcReaction
{
    public sealed class View : IHttpHandler
    {
        readonly string path;
        bool is_resuable;

        public View(string path) : this(path, true) { }
        
        public View(string path, bool resuable)
        {
            this.path = path;
            this.is_resuable = resuable;
        }

        public bool IsReusable
        {
            get { return is_resuable; }
        }

        public void ProcessRequest(HttpContext context)
        {
            ("views/" + path + ".aspx").GetCompiledPageInstance().ProcessRequest(context);
        }
    }

    public abstract class ViewPage<T> : System.Web.UI.Page
    {
        public T Model { get; set; }
    }
}
