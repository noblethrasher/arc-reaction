using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ArcReaction
{
    public abstract class TerminalPage : ControlPoint
    {
        public ControlPoint Next(Message msg)
        {
            return null;
        }

        public abstract IHttpHandler GetHandler(HttpContextEx context);
    }
}
