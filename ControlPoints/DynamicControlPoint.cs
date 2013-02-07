using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ArcReaction
{
    public sealed class ClosureControlPoint : ControlPoint
    {
        readonly string key;
        
        public ClosureControlPoint(string key)
        {
            this.key = key;
        }
        
        public ControlPoint Next(Message msg)
        {
            //here we ignore the msg parameter because we want guaranteed case sensitivity
            
            return Session.Retrieve(key);
        }

        public System.Web.IHttpHandler GetHandler(HttpContextEx context)
        {
            return (AdhocHttpHandler) (c =>
            {
                context.Response.Write("Not Found");
                context.Response.StatusCode = 404;
            });
        }
    }
}
