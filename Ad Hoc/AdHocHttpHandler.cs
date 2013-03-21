using System;
using System.Web;
namespace ArcReaction
{
    public sealed class AdhocHttpHandler : IHttpHandler
    {
        readonly Action<HttpContext> processRequest;
        readonly int status = 200;
        
        private AdhocHttpHandler(int status)
        {
            this.status = status;
        }

        public AdhocHttpHandler(Action<HttpContext> processRequest, int status) : this(status)
        {
            this.processRequest = processRequest;
        }

        public AdhocHttpHandler(Action<HttpContext> processRequest) : this(processRequest, 200) { }

        public AdhocHttpHandler(string str, int status) : this(status)
        {
            this.processRequest = c => c.Response.Write(str);

        }

        public AdhocHttpHandler(string str) : this(str, 200) { }

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