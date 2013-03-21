using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ArcReaction
{
    public abstract class TerminalPage : AppState
    {
        public AppState Next(Message msg)
        {
            return null;
        }

        public abstract IHttpHandler GetRepresentation(HttpContextEx context);
    }
}
