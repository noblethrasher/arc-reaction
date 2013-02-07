using System;
using System.Web;
namespace ArcReaction
{
    public sealed class AdhocHttpHandler : IHttpHandler
    {
        readonly Action<HttpContext> processRequest;
        
        public AdhocHttpHandler(Action<HttpContext> processRequest)
        {
            this.processRequest = processRequest;
        }

        public AdhocHttpHandler(string str)
        {
            this.processRequest = c => c.Response.Write(str);
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            processRequest(context);
        }

        public static implicit operator AdhocHttpHandler(Action<HttpContext> processRequest)
        {
            return new AdhocHttpHandler(processRequest);
        }
    }
}