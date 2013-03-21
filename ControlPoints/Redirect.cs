using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ArcReaction
{
    public sealed class RedirectToOther : AppState, IHttpHandler
    {
        readonly string path;

        public RedirectToOther(string path)
        {
            this.path = path;
        }
        
        public AppState Next(Message msg)
        {
            return this;
        }

        public System.Web.IHttpHandler GetRepresentation(HttpContextEx context)
        {
            return this;
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.AddHeader("Location", path);
            context.Response.StatusCode = 303;
        }
    }
}
