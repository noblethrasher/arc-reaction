using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ArcReaction
{
    public abstract class WebException : Exception
    {
        public abstract IHttpHandler GetHandler(HttpContextEx context);
    }

    public abstract class Error400 : AppState
    {
        protected readonly int status;

        public Error400(int status)
        {
            this.status = status;
        }

        public AppState Next(Message msg)
        {
            return this;
        }

        public abstract IHttpHandler GetRepresentation(HttpContextEx context);        
    }

    public sealed class ResourceNotFound : Error400, IHttpHandler
    {
        public ResourceNotFound()
            : base(404)
        {

        }
        
        public override IHttpHandler GetRepresentation(HttpContextEx context)
        {
            return this;
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Write("not found");
            context.Response.StatusCode = status;
        }
    }
}
