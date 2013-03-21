using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Prelude;

namespace ArcReaction
{
    public abstract class Session : AppState
    {
        static Dictionary<string, Func<string, Session>> sessionFactories = new Dictionary<string, Func<string, Session>>();
        static readonly byte key_length = 3;

        protected static bool AddSessionFactory(string key, Func<string, Session> factory)
        {
            if (sessionFactories.Count == Math.Floor(Math.Pow(62, key_length)))
                throw new Exception("");
            
            if (!sessionFactories.ContainsKey(key))
            {
                sessionFactories.Add(key, factory);
                return true;
            }

            return false;
        }

        protected static string AddSessionFactory(Func<string, Session> factory)
        {
            string session_key = null;

            lock (sessionFactories)
            {
                do
                    session_key = GenerateKey(key_length);
                while (sessionFactories.ContainsKey(session_key));

                sessionFactories.Add(session_key, factory);
            }

            return session_key;
        }

        static Func<char>[] random_chars = new[] { (Func<char>)GetRandomLowerCaseLetter, GetRandomUpperCaseLetter, GetRandomDigit };

        public abstract AppState Next(Message msg);
        public abstract IHttpHandler GetRepresentation(HttpContextEx context);

        static Random random = new Random();

        protected static string GenerateKey(byte length)
        {
            var bytes = new char[length];
            
            for (var i = 0; i < length; i++)
                bytes[i] = random_chars[random.Next(0, 2)]();

            return new string(bytes);
        }

        static char GetRandomLowerCaseLetter()
        {
            return (char) random.Next('A', 'Z');
        }

        static char GetRandomUpperCaseLetter()
        {
            return (char) random.Next('a', 'z');
        }

        static char GetRandomDigit()
        {
            return (char) random.Next('0', '9');
        }

        public static Session Retrieve(string key)
        {
            Func<string, Session> factory = null;

            if (key.GreaterThan(key_length))
            {
                var factory_key = key.Substring(0, key_length);
                var session_key = key.Substring(key_length, key.Length - factory_key.Length);

                if(sessionFactories.TryGetValue(factory_key, out factory))
                    return factory(session_key);
            }

            return null;
        }

        public abstract string Key { get; }
    }

    public sealed class OneTimeUseControlPoint : Session
    {
        static readonly string factory_key;
        
        static OneTimeUseControlPoint()
        {
            factory_key = AddSessionFactory(GetSession);
        }

        static Session GetSession(string key)
        {
            var session = HttpContext.Current.Cache.Get(key) as OneTimeUseControlPoint;

            if (session != null)
                HttpContext.Current.Cache.Remove(key);

            return session;
        }

        Func<HttpContextEx, IHttpHandler> get_handler;
        Func<Message, AppState> get_controlPoint;
        readonly string key;
       
        public OneTimeUseControlPoint(IHttpHandler handler) : this(c => handler, null) { }
        
        public OneTimeUseControlPoint(IHttpHandler handler, AppState next) : this(c => handler, m => next) { }
        public OneTimeUseControlPoint(AppState control) : this(control.GetRepresentation, control.Next) { }

        public OneTimeUseControlPoint(Func<HttpContextEx, IHttpHandler> handler_factory) : this(handler_factory, null) { }

        public OneTimeUseControlPoint(Func<HttpContextEx, IHttpHandler> handler_factory, Func<Message, AppState> next)
        {
            get_handler = handler_factory;
            get_controlPoint = next;

            var cache = HttpContext.Current.Cache;

            init:

            var key = GenerateKey(10);

            if (cache[key] != null)
                goto init;
            else
                cache.Insert(key, this, null, DateTime.Now.AddMinutes(5), System.Web.Caching.Cache.NoSlidingExpiration);

            this.key = key;   
        }

        public override AppState Next(Message msg)
        {
            return get_controlPoint != null ? get_controlPoint(msg) : null;
        }

        public override IHttpHandler GetRepresentation(HttpContextEx context)
        {
            return get_handler(context);
        }

        public override string Key
        {
            get { return factory_key + key; }
        }
    }
}
