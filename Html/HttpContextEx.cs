using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ArcReaction
{
    //Adds a few useful methods and properties to the HttpContext class
    
    public sealed class HttpContextEx
    {
        readonly HttpContextBase context;

        public HttpContextEx(HttpContextBase context)
        {
            this.context = context;
        }

        public HttpContextEx(HttpContext context)
        {
            this.context = new HttpContextWrapper(context);
        }

        public string this[string key]
        {
            get
            {
                return context.Request[key];
            }
        }

        public void AcceptWebSocketRequest(Func<System.Web.WebSockets.AspNetWebSocketContext, System.Threading.Tasks.Task> userFunc)
        {
            context.AcceptWebSocketRequest(userFunc);
        }

        public void AcceptWebSocketRequest(Func<System.Web.WebSockets.AspNetWebSocketContext, System.Threading.Tasks.Task> userFunc, System.Web.WebSockets.AspNetWebSocketOptions options)
        {
            context.AcceptWebSocketRequest(userFunc, options);
        }

        public void AddError(Exception errorInfo)
        {
            context.AddError(errorInfo);
        }

        public ISubscriptionToken AddOnRequestCompleted(Action<HttpContextBase> callback)
        {
            return context.AddOnRequestCompleted(callback);
        }

        public Exception[] AllErrors
        {
            get
            {
                return context.AllErrors;
            }
        }

        public bool AllowAsyncDuringSyncStages
        {
            get
            {
                return context.AllowAsyncDuringSyncStages;
            }
            set
            {
                context.AllowAsyncDuringSyncStages = value;
            }
        }

        public HttpApplicationStateBase Application
        {
            get
            {
                return context.Application;
            }
        }

        public HttpApplication ApplicationInstance
        {
            get
            {
                return context.ApplicationInstance;
            }
            set
            {
                context.ApplicationInstance = value;
            }
        }

        public System.Web.Configuration.AsyncPreloadModeFlags AsyncPreloadMode
        {
            get
            {
                return context.AsyncPreloadMode;
            }
            set
            {
                context.AsyncPreloadMode = value;
            }
        }

        public System.Web.Caching.Cache Cache
        {
            get
            {
                return context.Cache;
            }
        }

        public void ClearError()
        {
            context.ClearError();
        }

        public IHttpHandler CurrentHandler
        {
            get
            {
                return context.CurrentHandler;
            }
        }

        public RequestNotification CurrentNotification
        {
            get
            {
                return context.CurrentNotification;
            }
        }

        public ISubscriptionToken DisposeOnPipelineCompleted(IDisposable target)
        {
            return context.DisposeOnPipelineCompleted(target);
        }

        public override bool Equals(object obj)
        {
            return context.Equals(obj);
        }

        public Exception Error
        {
            get
            {
                return context.Error;
            }
        }

        public object GetGlobalResourceObject(string classKey, string resourceKey)
        {
            return context.GetGlobalResourceObject(classKey, resourceKey);
        }

        public object GetGlobalResourceObject(string classKey, string resourceKey, System.Globalization.CultureInfo culture)
        {
            return context.GetGlobalResourceObject(classKey, resourceKey, culture);
        }

        public override int GetHashCode()
        {
            return context.GetHashCode();
        }

        public object GetLocalResourceObject(string virtualPath, string resourceKey)
        {
            return context.GetLocalResourceObject(virtualPath, resourceKey);
        }

        public object GetLocalResourceObject(string virtualPath, string resourceKey, System.Globalization.CultureInfo culture)
        {
            return context.GetLocalResourceObject(virtualPath, resourceKey, culture);
        }

        public object GetSection(string sectionName)
        {
            return context.GetSection(sectionName);
        }

        public object GetService(Type serviceType)
        {
            return context.GetService(serviceType);
        }

        public IHttpHandler Handler
        {
            get
            {
                return context.Handler;
            }
            set
            {
                context.Handler = value;
            }
        }

        public bool IsCustomErrorEnabled
        {
            get
            {
                return context.IsCustomErrorEnabled;
            }
        }

        public bool IsDebuggingEnabled
        {
            get
            {
                return context.IsDebuggingEnabled;
            }
        }

        public bool IsPostNotification
        {
            get
            {
                return context.IsPostNotification;
            }
        }

        public bool IsWebSocketRequest
        {
            get
            {
                return context.IsWebSocketRequest;
            }
        }

        public bool IsWebSocketRequestUpgrading
        {
            get
            {
                return context.IsWebSocketRequestUpgrading;
            }
        }

        public System.Collections.IDictionary Items
        {
            get
            {
                return context.Items;
            }
        }

        public System.Web.Instrumentation.PageInstrumentationService PageInstrumentation
        {
            get
            {
                return context.PageInstrumentation;
            }
        }

        public IHttpHandler PreviousHandler
        {
            get
            {
                return context.PreviousHandler;
            }
        }

        public System.Web.Profile.ProfileBase Profile
        {
            get
            {
                return context.Profile;
            }
        }

        public void RemapHandler(IHttpHandler handler)
        {
            context.RemapHandler(handler);
        }

        public HttpRequestBase Request
        {
            get
            {
                return context.Request;
            }
        }

        public HttpResponseBase Response
        {
            get
            {
                return context.Response;
            }
        }

        public void RewritePath(string filePath, string pathInfo, string queryString)
        {
            context.RewritePath(filePath, pathInfo, queryString);
        }

        public void RewritePath(string filePath, string pathInfo, string queryString, bool setClientFilePath)
        {
            context.RewritePath(filePath, pathInfo, queryString, setClientFilePath);
        }

        public void RewritePath(string path)
        {
            context.RewritePath(path);
        }

        public void RewritePath(string path, bool rebaseClientPath)
        {
            context.RewritePath(path, rebaseClientPath);
        }

        public HttpServerUtilityBase Server
        {
            get
            {
                return context.Server;
            }
        }

        public HttpSessionStateBase Session
        {
            get
            {
                return context.Session;
            }
        }

        public void SetSessionStateBehavior(System.Web.SessionState.SessionStateBehavior sessionStateBehavior)
        {
            context.SetSessionStateBehavior(sessionStateBehavior);
        }

        public bool SkipAuthorization
        {
            get
            {
                return context.SkipAuthorization;
            }
            set
            {
                context.SkipAuthorization = value;
            }
        }

        public bool ThreadAbortOnTimeout
        {
            get
            {
                return context.ThreadAbortOnTimeout;
            }
            set
            {
                context.ThreadAbortOnTimeout = value;
            }
        }

        public DateTime Timestamp
        {
            get
            {
                return context.Timestamp;
            }
        }

        public override string ToString()
        {
            return context.ToString();
        }

        public TraceContext Trace
        {
            get
            {
                return context.Trace;
            }
        }

        public System.Security.Principal.IPrincipal User
        {
            get
            {
                return context.User;
            }
            set
            {
                context.User = value;
            }
        }

        public string WebSocketNegotiatedProtocol
        {
            get
            {
                return context.WebSocketNegotiatedProtocol;
            }
        }

        public IList<string> WebSocketRequestedProtocols
        {
            get
            {
                return context.WebSocketRequestedProtocols;
            }
        }

        public static implicit operator HttpContextEx(HttpContext context)
        {
            return new HttpContextEx(context);
        }

        public static implicit operator HttpContextEx(HttpContextBase context)
        {
            return new HttpContextEx(context);
        }
    }
}
