using Prelude;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

namespace ArcReaction
{
    public abstract class JSON : IHttpHandler
    {
        protected readonly JSONPropertyList properties;

        public JSON()
        {
            this.properties = new JSONPropertyList(this);
        }

        protected sealed class JSONPropertyList : IEnumerable<JSONProperty>
        {
            readonly List<JSONProperty> xs = new List<JSONProperty>();
            readonly JSON owner;

            public void Add(JSONProperty property)
            {
                xs.Add(property);
            }

            public void Add(string name, object value, bool show_null)
            {
                xs.Add(new DefaultJSONProperty(name, JSONProperty.GetValueString(value), show_null));
            }

            public void Add(string name, object value)
            {
                xs.Add(new DefaultJSONProperty(name, JSONProperty.GetValueString(value), true));
            }

            public void Add(string name, JSON value, bool show_null)
            {
                xs.Add(JSONProperty.Create(name, value, show_null));
            }

            public void Add(string name, JSON value)
            {
                xs.Add(JSONProperty.Create(name, value, true));
            }

            public void Add(string name, IEnumerable<JSON> values, bool show_null)
            {
                xs.Add(JSONProperty.Create(name, new JSONCollection(values), show_null));
            }

            public void Add(string name, IEnumerable<JSON> values)
            {
                xs.Add(JSONProperty.Create(name, new JSONCollection(values)));
            }

            public JSONPropertyList(JSON owner)
            {
                this.owner = owner;
            }

            public IEnumerator<JSONProperty> GetEnumerator()
            {
                return xs.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public abstract class JSONProperty
        {
            protected readonly string name, value;
            internal readonly bool show_null;

            public string Name { get { return name; } }
            public string Value { get { return value; } }

            public JSONProperty(string name, string value, bool show_null)
            {
                this.name = MakeSafeJavaScriptIdentifier(name);
                this.value = value;
                this.show_null = show_null;
            }

            static string MakeSafeString(string s)
            {
                return '"' + s.Replace("\"", "\\\"") + '"';
            }

            static bool ValidInitialCharacter(char c)
            {
                var unicode_category = char.GetUnicodeCategory(c);

                return  unicode_category == UnicodeCategory.UppercaseLetter ||
                        unicode_category == UnicodeCategory.LowercaseLetter ||
                        unicode_category == UnicodeCategory.TitlecaseLetter ||
                        unicode_category == UnicodeCategory.ModifierLetter ||
                        unicode_category == UnicodeCategory.LetterNumber ||
                        unicode_category == UnicodeCategory.OtherLetter;
            }

            static bool ValidSubsequentCharacter(char c)
            {
                var unicode_category = char.GetUnicodeCategory(c);

                return  ValidInitialCharacter(c) ||
                        unicode_category == UnicodeCategory.NonSpacingMark ||
                        unicode_category == UnicodeCategory.SpacingCombiningMark ||
                        unicode_category == UnicodeCategory.DecimalDigitNumber ||
                        unicode_category == UnicodeCategory.ConnectorPunctuation;
            }

            static string MakeSafeJavaScriptIdentifier(string name)
            {
                if (!ValidInitialCharacter(name[0]))
                    return MakeSafeString(name);
                                                
                for (var i = 1; i < name.Length; i++)
                    if(!ValidSubsequentCharacter(name[i]))
                        return MakeSafeString(name);

                return name;                            
            }
            
            internal static string GetValueString(object value)
            {                
                if(value == null || (IsNull(value)))
                    return "null";                
                
                var type = value.GetType();

                if (type.IsArray)
                {
                    var result = new List<string>();

                    dynamic xs = value;                    

                    foreach (var x in xs)
                        result.Add(GetValueString(x));

                    return '[' + string.Join(",", result) + ']';
                }

                var s = value.ToString();

                if (IsSimpleNumberType(type))
                    return s;

                if (type == typeof(DateTime))
                    return "new Date(\"" + s + "\")";

                if (type == typeof(Guid))
                    return '"' + s + '"';

                if (type == typeof(bool))
                    return s.ToLower();

                if(type.IsSubclassOf(typeof(JSON)))
                    return value.ToString();

                return MakeSafeString(s);
            }

            static bool IsNull(object value)                
            {
                Type type = null;

                return object.ReferenceEquals(value, null) || ((type = value.GetType()).IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && !(((dynamic)value).HasValue));
            }

            static bool IsSimpleNumberType(Type type)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    return IsSimpleNumberType(type.GetGenericArguments()[0]);
                else
                    return type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort) || type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong) ||
                            type == typeof(decimal) || type == typeof(float) || type == typeof(double);
            }


            public static JSONProperty Create(string name, object o, bool showNull)
            {
                if (o is JSON[])
                    return new JSONObjectProperty(name, new JSONCollection(o as JSON[]), showNull);
                
                return new DefaultJSONProperty(name, GetValueString(o), showNull);
            }

            public static JSONProperty Create(string name, object o)
            {
                return Create(name, o, true);
            }

            public static JSONProperty Create(string name, JSON o, bool showNull)
            {
                return o != null ? (JSONProperty) new JSONObjectProperty(name, o, showNull) : new DefaultJSONProperty(name, GetValueString(null), showNull);
            }

            public static JSONProperty Create(string name, JSON o)
            {
                return Create(name, o, true);
            }

            public abstract string ToString(int n);
            
            public override string ToString()
            {
                return ToString(0);
            }
        }

        public sealed class DefaultJSONProperty : JSONProperty
        {
            public DefaultJSONProperty(string name, string value, bool show_null) : base(name, value, show_null) { }

            public override string ToString(int n)
            {
                return '\t'.RepeatAndAppend(n, name + " : " + value);
            }
        }

        public sealed class JSONObjectProperty : JSONProperty
        {
            JSON json;
            
            public JSONObjectProperty(string name, JSON obj, bool show_null)
                : base(name, obj.ToString(), show_null)
            {
                this.json = obj;
            }

            public override string ToString(int n)
            {
                var sb = new StringBuilder();

                sb.Append('\t'.RepeatAndAppend(n, Name + ":"));
                sb.Append(json.ToString(n));

                return sb.ToString();
            }
        }

        public virtual string ToString(int n)
        {
            var sb = new StringBuilder();
            sb.Append('\t'.RepeatAndAppend(n, "{\r\n"));

            if (properties.Any())
            {
                var filtered = (from p in properties where p.Value != "null" || p.show_null select p).GetEnumerator();

                filtered.MoveNext();

                sb.Append(filtered.Current.ToString(n + 1));

                while (filtered.MoveNext())
                {
                    sb.Append(",");
                    sb.Append("\r\n");
                    sb.Append(filtered.Current.ToString(n + 1));
                }
            }

            sb.Append("\r\n" + '\t'.RepeatAndAppend(n, "}"));

            return sb.ToString();
        
        }

        public override string ToString()
        {
            return ToString(0);
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.AddHeader("Content-Type", "application/json");
            context.Response.AddHeader("Content-Disposition", "inline");
            context.Response.Write(this.ToString());
        }

        public void AddScalar(string name, object value)
        {
            this.properties.Add(name, value);
        }

        public void AddArray(string name, System.Collections.IEnumerable values)
        {
            object[] array = values as object[];

            if (array == null)
            {
                var xs = new List<object>();

                foreach (var value in values)
                    xs.Add(value);

                array = xs.ToArray();
            }
            
            this.properties.Add(name, array);
        }
    }

    public sealed class JSONCollection : JSON
    {
        JSON[] json_objs;        
        
        public JSONCollection(IEnumerable<JSON> xs) : this(xs != null ? xs.ToArray() : new JSON[0]) { }

        public JSONCollection(params JSON[] json)
        {
            json_objs = json;           
        }

        public JSON this[int n]
        {
            get
            {
                return json_objs[n];
            }
        }

        public override string ToString()
        {
            return ToString(0);
        }

        public override string ToString(int n)
        {
            if (json_objs.Length == 0)
                return "new Array()";
            
            var sb = new StringBuilder();

            if(n > 0)
                sb.AppendLine();

            sb.AppendLine('\t'.RepeatAndAppend(n, "["));

            sb.Append(json_objs[0].ToString(n + 1));

            for (var i = 1; i < json_objs.Length; i++)
            {
                sb.Append(',');
                sb.Append("\r\n");
                sb.Append(json_objs[i].ToString(n + 1));
            }
            sb.Append("\r\n" + '\t'.RepeatAndAppend(n, "]"));

            return sb.ToString();
        }
    }
}
