using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ArcReaction
{
    public sealed class ClosureControlPoint : AppState
    {
        readonly string key;
        
        public ClosureControlPoint(string key)
        {
            this.key = key;
        }
        
        public AppState Next(Message msg)
        {
            //here we ignore the msg parameter because we want guaranteed case sensitivity
            
            return Session.Retrieve(key);
        }

        public System.Web.IHttpHandler GetRepresentation(HttpContextEx context)
        {
            return (AdhocHttpHandler) (c =>
            {
                context.Response.Write("Not Found");
                context.Response.StatusCode = 404;
            });
        }
    }
}
