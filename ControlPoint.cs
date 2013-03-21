using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ArcReaction
{
    public interface AppState
    {
        AppState Next(Message msg);
        IHttpHandler GetRepresentation(HttpContextEx context);
    }    
}
