using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ArcReaction
{
    public sealed class RedirectToOther : ControlPoint, IHttpHandler
    {
        readonly string path;

        public RedirectToOther(string path)
        {
            this.path = path;
        }
        
        public ControlPoint Next(Message msg)
        {
            return this;
        }

        public System.Web.IHttpHandler GetHandler(HttpContextEx context)
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
