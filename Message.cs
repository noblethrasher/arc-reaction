using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ArcReaction
{
    public sealed class Message
    {
        internal string message;
        public readonly HttpContextEx Context;

        static StringComparison comparison = StringComparison.OrdinalIgnoreCase;

        static Message()
        {
            var sensitivity = System.Web.Configuration.WebConfigurationManager.AppSettings["CaseInsensitivePathSegment"];

            if (sensitivity != null && (sensitivity.Equals("no", StringComparison.OrdinalIgnoreCase) || sensitivity.Equals("false", StringComparison.OrdinalIgnoreCase)))
                comparison = StringComparison.Ordinal;
        }

        public Message(string message, HttpContextEx context)
        {
            this.message = message;
            this.Context = context;
        }

        public string this[string s]
        {
            get
            {
                return Context.Request[s];
            }
        }

        internal Message(HttpContextEx context)
        {
            this.Context = context;
        }

        public static implicit operator string(Message msg)
        {
            return msg.ToString();
        }

        public static bool operator ==(Message msg, string s)
        {
            return msg.message.Equals(s, comparison);
        }

        public static bool operator !=(Message msg, string s)
        {
            return !msg.message.Equals(s, comparison);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return comparison == StringComparison.OrdinalIgnoreCase ? message.ToLower() : message;
        }
    }    
}
