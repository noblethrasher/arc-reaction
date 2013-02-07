using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ArcReaction
{
    public interface ControlPoint
    {
        ControlPoint Next(Message msg);
        IHttpHandler GetHandler(HttpContextEx context);
    }    
}
