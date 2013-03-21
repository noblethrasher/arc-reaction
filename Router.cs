using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ArcReaction
{
    public abstract class Router : IHttpModule
    {
        HttpApplication app;
        
        public void Dispose() { }
        
        public void Init(HttpApplication app)
        {
            this.app = app;

            this.app.PostResolveRequestCache += Route;

            this.app.Error += HandleError;

            this.app.BeginRequest += delegate { ModelState.ClearErrors(); };
        }

        protected virtual void HandleError(object sender, EventArgs e)
        {
            var ex = app.Server.GetLastError() as WebException;

            if (ex != null)
            {
                var handler = ex.GetHandler(new HttpContextWrapper(app.Context));

                app.Server.Transfer(handler, true);
            }
        }

        protected abstract AppState GetRoot(HttpContextEx context);

        protected virtual RouteVerdict GetRouteVerdict(HttpContextEx context)
        {
            return new DefaultRouteVerdict(context);
        }

        protected virtual AppState TranslateNull(HttpContextEx context)
        {
            return new ResourceNotFound();
        }

        protected virtual void Route(object sender, EventArgs e)
        {
            var context = new HttpContextEx(app.Context);
            var verdict = GetRouteVerdict(context);

            if (verdict.IsRoutable)
            {
                var controlPoint = GetRoot(context);
                var message = new Message(context);

                try
                {
                    var segments = verdict.Segments;
                    
                    if (segments.Length > 0)
                    {
                        int index = 0;

                        if (segments[0] == "x" && segments.Length > 1)
                        {
                            controlPoint = new ClosureControlPoint(segments[1]);
                            index = 1;
                        }
                        
                        for (var i = index; i < segments.Length; i++)
                        {
                            message.message = segments[i];
                            controlPoint = controlPoint.Next(message) ?? TranslateNull(context);
                        }

                    }                    
                    
                    context.RemapHandler(controlPoint.GetRepresentation(context));
                }
                catch (WebException ex)
                {
                    var handler = ex.GetHandler(context);

                    if (handler != null)
                        context.RemapHandler(handler);
                    else
                        throw;
                }
            }
        }

        protected abstract class RouteVerdict
        {
            HttpContextEx context;
            public readonly string[] Segments;
            public bool IsRoutable;

            readonly static char[] separators = new[] { '/' };

            public RouteVerdict(HttpContextEx context)
            {
                this.context = context;

                var raw = context.Request.Path;

                if (raw == "/")
                    Segments = new string[0];
                else
                    Segments = raw.Substring(1).Split(separators, StringSplitOptions.RemoveEmptyEntries);                
            }

            public static bool operator true(RouteVerdict verdict)
            {
                return verdict.IsRoutable;
            }

            public static bool operator false(RouteVerdict verdict)
            {
                return !verdict.IsRoutable;
            }
        }

        sealed class DefaultRouteVerdict : RouteVerdict
        {
            public DefaultRouteVerdict(HttpContextEx context)
                : base(context)
            {
                string lst = null;

                if (Segments.Length > 0 && (lst = Segments[Segments.Length - 1]).Contains('.'))
                {
                    var xs = lst.Split('.');

                    var ext = xs[xs.Length - 1];

                    if (MimeMapping.GetMimeMapping('.' + ext) != "application/octet-stream")
                    {
                        IsRoutable = false;
                        return;
                    }
                }

                IsRoutable = true;
            }
        }
    }    
}
