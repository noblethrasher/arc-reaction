using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prelude;
using System.Web;
using System.Drawing;

namespace ArcReaction
{
    public static class HtmlModule
    {
        public static T WrapIn<T>(this IEnumerable<HTMLElement> xs, T elem)
            where T : HTMLElement
        {
            elem.AddRange(xs);

            return elem;
        }

        public static HTMLElement Bundle(this IEnumerable<HTMLElement> xs)            
        {
            var elem = new Fragment();
            
            elem.AddRange(xs);

            return elem;
        }
    }
    
    internal static class HTML_Utils
    {
        public static string GetText<T>(T element)
            where T : HTMLElement, CanContainSimpleText
        {
            var sb = new StringBuilder();

            foreach (var child in element.children)
            {
                var contains_simple_text = child as CanContainSimpleText;

                if (contains_simple_text != null)
                    sb.AppendLine(contains_simple_text.Text);
            }

            return sb.ToString();
        }

        public static void SetText<T>(T element, string text)
            where T : HTMLElement, CanContainSimpleText
        {
            element.RemoveAllChildren();
            
            if(text != null)
                element.Add(new TextNode(text));
        }
    }

    interface CanContainSimpleText
    {
        string Text
        {
            get;            
        }
    }
    
    public abstract class HTMLElement : IEquatable<HTMLElement>, IHttpHandler, AppState
    {
        protected readonly string tag_name;
        protected readonly bool self_closing;

        List<HTMLAttribute> non_removable_attributes = new List<HTMLAttribute>();
        List<HTMLAttribute> removable_attributes = new List<HTMLAttribute>();

        protected internal List<HTMLElement> children = new List<HTMLElement>();

        protected internal HTMLElement parent = null;

        Guid unique_identifier = Guid.NewGuid();

        public HTMLElement(string tag_name) : this(tag_name, null, true) { }
        public HTMLElement(string tag_name, bool self_closing) : this(tag_name, null, self_closing) { }
        public HTMLElement(string tag_name, IEnumerable<HTMLElement> xs) : this(tag_name, xs, true) { }

        HTMLAttribute id;
        HTMLAttribute disabled;
        HTMLAttribute @class;

        public HTMLElement(string tag_name, IEnumerable<HTMLElement> xs, bool self_closing)
        {
            this.tag_name = tag_name;
            this.self_closing = self_closing;
            
            non_removable_attributes.Add(id = new DefaultHtmlAttribueImpl("id"));
            non_removable_attributes.Add(disabled = new DefaultHtmlAttribueImpl("disabled"));
            non_removable_attributes.Add(@class = new DefaultHtmlAttribueImpl("class"));

            if(xs != null)
                AddRange(xs);
        }

        public IEnumerable<HTMLAttribute> Attributes
        {
            get
            {
                for (var i = 0; i < non_removable_attributes.Count; i++)
                    if (!non_removable_attributes[i].Value.IsNullOrEmpty())
                        yield return non_removable_attributes[i];

                for (var i = 0; i < removable_attributes.Count; i++)
                    if (!removable_attributes[i].Value.IsNullOrEmpty())
                        yield return removable_attributes[i];
            }
        }

        public string ID
        {
            get
            {
                return id.Value;
            }
            set
            {
                id.Value = value;
            }
        }

        public string ClassName
        {

            get
            {
                return @class.Value;
            }
            set
            {
                @class.Value = value;
            }

        }

        public bool Disabled
        {
            get
            {
                return disabled.Value == "disabled";
            }
            set
            {
                disabled.Value = value ? "disabled" : null;
            }
        }

        public virtual IEnumerable<HTMLElement> Children
        {
            get
            {
                return children;
            }
        }

        public void Add(HTMLAttribute attribute)
        {
            foreach (var attr in non_removable_attributes)
                if (attr == attribute)
                {
                    attr.Value = attribute.Value;
                    return;
                }

            foreach (var attr in non_removable_attributes)
                if (attr == attribute)
                {
                    attr.Value = attribute.Value;
                    return;
                }

            removable_attributes.Add(attribute);
        }

        protected void AddNonRemovableAttribute(HTMLAttribute attr)
        {
            non_removable_attributes.Add(attr);
        }

        public void AddRange(IEnumerable<HTMLAttribute> attributes)
        {
            foreach (var attr in attributes)
                Add(attr);
        }

        public void Add(HTMLElement element)
        {
            lock (children)
            {
                if (element.parent == this)
                    return;

                children.Add(element);
                element.parent = this;
            }
        }

        public void AddRange(IEnumerable<HTMLElement> elements)
        {
            children.AddRange(from e in elements where e.parent != this select e);
            
            foreach (var element in elements)
                element.parent = this;
        }

        public bool RemoveChild(HTMLElement element)
        {
            lock (children)
            {
                int? n = null;

                foreach (var child in children)
                    if (child.unique_identifier == element.unique_identifier)
                    {
                        n = n ?? 0;
                        break;
                    }
                    else
                    {
                        n = n ?? 0;
                        n++;
                    }

                if (n.HasValue && n < children.Count)
                {
                    children.RemoveAt(n.Value);
                    return true;
                }

                return false;
            }
        }

        public void RemoveAllChildren()
        {
            lock (children)
            {
                foreach (var child in children)
                    child.parent = null;

                children.Clear();
            }
        }

        public T WrapIn<T>(T other)
            where T : HTMLElement
        {
            other.Add(this);
            return other;
        }

        public Fragment BundleWith(params HTMLElement[] elements)
        {
            var fragment = new Fragment();
            fragment.Add(this);

            fragment.AddRange(elements);

            return fragment;
        }

        public bool Equals(HTMLElement other)
        {
            return other != null && other.unique_identifier == this.unique_identifier;
        }

        protected internal virtual string ToString(int n)
        {
            var sb = new StringBuilder();

            sb.Append('\t'.RepeatAndAppend(n, "<" + tag_name));

            if (Attributes.Any())
                sb.Append(" " + string.Join(" ", Attributes));

            var has_children = children.Any();

            if (self_closing && !has_children)
                sb.Append("/>");
            else
            {
                sb.Append(">");

                if (has_children)
                {
                    sb.AppendLine();

                    foreach (var child in children)
                        sb.AppendLine(child.ToString(n + 1));
                }

                sb.Append('\t'.RepeatAndAppend(n, "</" + tag_name + ">"));
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            return ToString(1);
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            context.Response.Write(this.ToString());
        }

        public AppState Next(Message msg)
        {
            return null;
        }

        public IHttpHandler GetRepresentation(HttpContextEx context)
        {
            return this;
        }
    }

    public abstract class HTMLAttribute : IEquatable<HTMLAttribute>
    {
        readonly string name;
        string value;

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        public HTMLAttribute(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        public override string ToString()
        {
            return name + "=\"" + System.Net.WebUtility.HtmlEncode(value) +"\"";
        }

        public bool Equals(HTMLAttribute other)
        {
            return other != null && this.name.Equals(other.name, StringComparison.OrdinalIgnoreCase);
        }

        public static bool operator ==(HTMLAttribute x, HTMLAttribute y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(HTMLAttribute x, HTMLAttribute y)
        {
            return !x.Equals(y);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as HTMLAttribute);
        }
    }

    sealed class DefaultHtmlAttribueImpl : HTMLAttribute
    {
        public DefaultHtmlAttribueImpl(string name, string value) : base(name, value) { }
        public DefaultHtmlAttribueImpl(string name) : this(name, null) { }   
    }

    public sealed class P : HTMLElement, CanContainSimpleText
    {
        public P(string text) : base("p")
        {
            Text = text;
        }
        
        public string Text
        {
            get
            {
                return HTML_Utils.GetText(this);
            }
            set
            {
                HTML_Utils.SetText(this, value);
            }
        }
    }

    public sealed class A : HTMLElement, CanContainSimpleText
    {
        readonly HTMLAttribute href;

        public A(string text, string href)
            : base("a")
        {
            AddNonRemovableAttribute(this.href = new DefaultHtmlAttribueImpl("href", href));

            if (text != null)
                Text = text;
        }

        public A(string text, AppState p)
            : this(text, "/x/" + new OneTimeUseControlPoint(p).Key)
        {

        }

        public A(string text, Func<HttpContextEx, IHttpHandler> f) : this(text, "/x/" + new OneTimeUseControlPoint(f).Key)
        {

        }
        
        public string Href
        {
            get
            {
                return href.Value;
            }
            set
            {
                href.Value = value;
            }
        }

        public string Text
        {
            get
            {
                return HTML_Utils.GetText(this);
            }
            set
            {
                HTML_Utils.SetText(this, value);
            }
        }
    }

    public sealed class Img : HTMLElement
    {
        public readonly HTMLAttribute src;
        
        public Img(string src)
            : base("img", true)
        {
            AddNonRemovableAttribute(this.src = new DefaultHtmlAttribueImpl("src", src));
        }

        public string Source
        {
            get
            {
                return src.Value;
            }
            set
            {
                src.Value = value;
            }
        }
    }

    public sealed class TextNode : HTMLElement, CanContainSimpleText
    {
        readonly string text;

        public TextNode(string text) : base(null)
        {
            this.text = text;
        }
        
        protected internal override string ToString(int n)
        {
            return '\t'.RepeatAndAppend(n, System.Net.WebUtility.HtmlEncode(text));
        }

        public string Text
        {
            get { return text; }
        }
    }

    public sealed class UL : HTMLElement
    {
        public UL() : base("ul") { }
        public UL(IEnumerable<LI> xs) : base("ul", xs) { }
        public UL(IEnumerable<object> xs) : this(xs.Select(x => new LI(x.ToString()))) { }
    }

    public sealed class OL : HTMLElement
    {
        public OL() : base("ol") { }
        public OL(IEnumerable<LI> xs) : base("ol", xs) { }
    }

    public sealed class LI : HTMLElement, CanContainSimpleText
    {
        public LI() : base("li") { }

        public LI(IEnumerable<HTMLElement> xs) : base("li", xs) { }
        public LI(string text)
            : this()
        {
            Text = text;
        }

        public LI(object obj) : this(obj.ToString()) { }
         
        public LI(params HTMLElement[] xs) : this(xs as IEnumerable<HTMLElement>) { }

        public string Text
        {
            get
            {
                return HTML_Utils.GetText(this);
            }
            set
            {
                HTML_Utils.SetText(this, value);
            }
        }
    }

    public sealed class Fragment : HTMLElement
    {
        public Fragment() : base(null) { }
        
        protected internal override string ToString(int n)
        {
            var sb = new StringBuilder();

            foreach (var child in children)
                sb.Append(child.ToString(n));

            return sb.ToString();
        }
    }

    public sealed class Table : HTMLElement
    {
        HTMLAttribute cellSpacing, cellPadding, width, height;

        
        public Table(IEnumerable<TR> trs) : base("table", false)
        {
            cellPadding = new DefaultHtmlAttribueImpl("cellpadding");
            cellSpacing = new DefaultHtmlAttribueImpl("cellspacing");
            width = new DefaultHtmlAttribueImpl("width");
            height = new DefaultHtmlAttribueImpl("height");            
        }

        public Table(params TR[] trs) : this(trs as IEnumerable<TR>)
        {

        }
       
        static int? GetAttrValue(HTMLAttribute attr)
        {
            var val = attr.Value;

            if (!val.IsNullOrEmpty())
                return int.Parse(val);
            else
                return null;
        }

        public int? CellSpacing
        {
            get
            {
                return GetAttrValue(cellSpacing);
            }
            set
            {   
                cellSpacing.Value = value.ToString();
            }
        }

        public int? CellPadding
        {
            get
            {
                return GetAttrValue(cellPadding);
            }
            set
            {
                cellPadding.Value = value.ToString();
            }
        }

        public int? Width
        {
            get
            {
                return GetAttrValue(width);
            }
            set
            {
                width.Value = value.ToString();
            }
        }

        public int? Height
        {
            get
            {
                return GetAttrValue(height);
            }
            set
            {
                height.Value = value.ToString();
            }
        }
       
        protected internal override string ToString(int n)
        {
            var head = from child in children where child is Thead select child;
            var body = from child in children where child is TBody select child;
            var foot = from child in children where child is TFoot select child;

            var rest = children.Except(head.Union(body).Union(foot));

            var sb = new StringBuilder();

            sb.Append('\t'.RepeatAndAppend(n, "<table"));

            if (Attributes.Any())
                sb.Append(" " + string.Join(" ", Attributes));

            sb.Append(">\r\n");

            foreach (var e in head)
                sb.AppendLine(e.ToString(n + 1));

            foreach (var e in body)
                sb.AppendLine(e.ToString(n + 1));

            foreach (var e in rest)
                sb.AppendLine(e.ToString(n + 1));

            foreach (var e in foot)
                sb.AppendLine(e.ToString(n + 1));

            sb.AppendLine('\t'.RepeatAndAppend(n, "</table>"));

            return sb.ToString();
        }
    }

    public sealed class Thead : HTMLElement
    {
        public Thead() : base("thead", false) { }
    }

    public sealed class TFoot : HTMLElement
    {
        public TFoot() : base("tfoot", false) { }
    }

    public sealed class TBody : HTMLElement
    {
        public TBody() : base("tbody", false) { }
    }

    public sealed class TR : HTMLElement
    {
        public TR(IEnumerable<TD> tds) : base("tr", tds, true) { }
    }   

    public abstract class TableData : HTMLElement, CanContainSimpleText
    {
        public TableData(string tag_name) : base(tag_name) { }
        
        public string Text
        {
            get
            {
                return HTML_Utils.GetText(this);
            }
            
            set
            {
                HTML_Utils.SetText(this, value);
            }
        }
    }
    
    public sealed class TD : TableData, CanContainSimpleText
    {
        public TD() : base("td") { }

        public TD(object obj)
            : this()
        {
            Text = obj.ToString();
        }       
    }

    public sealed class TH : TableData
    {
        public TH() : base("th") { }
    }
    
    public abstract class FormElement : HTMLElement
    {
        HTMLAttribute name;

        public FormElement(string tag_name, string name)
            : base(tag_name)
        {
            AddNonRemovableAttribute(this.name = new DefaultHtmlAttribueImpl("name", name));
        }        

        public string Name
        {
            get
            {
                return name.Value;
            }
            set
            {
                name.Value = value;
            }
        }
    }

    public abstract class InputElement : FormElement
    {
        HTMLAttribute type;
        HTMLAttribute value;
        
        public InputElement(string name, string value, string type)
            : base("input", name)
        {
            AddNonRemovableAttribute(this.type = new DefaultHtmlAttribueImpl("type", type));
            AddNonRemovableAttribute(this.value = new DefaultHtmlAttribueImpl("value", value));
        }

        public string Type
        {
            get
            {
                return type.Value;
            }
        }

        public string Value
        {
            get
            {
                return value.Value;
            }
            set
            {
                this.value.Value = value;
            }
        }
    }

    public sealed class TextInput : InputElement
    {
        public TextInput(string name, string value) : base(name, value, "text") { }
        public TextInput(string name) : this(name, null) { }
    }

    public abstract class Checkable : InputElement
    {
        HTMLAttribute @checked;
        
        public Checkable(string name, string value, bool is_checked, string type) : base(name, value, type) 
        {
            AddNonRemovableAttribute(@checked = new DefaultHtmlAttribueImpl("checked", is_checked ? "checked" : null));
        }

        public Checkable(string name, string value, string type)
            : this(name, value, false, type)
        {

        }

        public bool Checked
        {
            get
            {
                return @checked.Value != null;
            }
            set
            {
                @checked.Value = value ? "checked" : null;
            }
        }
    }

    public sealed class Checkbox : Checkable
    {
        public Checkbox(string name, string value, bool is_checked) : base(name, value, is_checked, "checkbox") { }
    }

    public sealed class Radio : Checkable
    {
        public Radio(string name, string value, bool is_checked) : base(name, value, is_checked, "radio") { }
    }

    public sealed class Submit : InputElement
    {
        public Submit(string name, string value) : base(name, value, "submit") { }
        public Submit() : this(null, null) { }
    }

    public sealed class Select : FormElement
    {
        HTMLAttribute allow_multiple;
        
        public Select(string name, bool allow_multiple)
            : base("select", name)
        {
            this.allow_multiple = new DefaultHtmlAttribueImpl("multiple", allow_multiple ? "multiple" : null);
        }

        public Select(string name, IEnumerable<Option> options)
            : this(name, false)
        {
            AddRange(options);
        }

        public Select(string name, params Option[] options) : this(name, options as IEnumerable<Option>)
        {
            
        }

        public bool AllowMultiple
        {
            get
            {
                return allow_multiple.Value != null;
            }
            set
            {
                allow_multiple.Value = value ? "multiple" : null;
            }
        }
    }

    public sealed class Option : FormElement
    {
        HTMLAttribute value;
        
        public Option(string name, string value)
            : base("option", name)
        {
            this.value = new DefaultHtmlAttribueImpl("value", value);
        }

        public string Value
        {
            get
            {
                return value.Value;
            }
            set
            {
                this.value.Value = value;
            }
        }
    }

    public class AntiForgery : InputElement
    {
        public AntiForgery()
            : base("ANTIFORGERY", "", "hidden")
        {
            
        }
    }

    public sealed class Form : HTMLElement
    {
        readonly HTMLAttribute action;
        readonly HTMLAttribute method;
        readonly HTMLAttribute autocomplete;

        public Form(string action, string method, IEnumerable<HTMLElement> children)
            : base("form")
        {
            this.action = new DefaultHtmlAttribueImpl("action", action);
            this.method = new DefaultHtmlAttribueImpl("method", method);
            this.autocomplete = new DefaultHtmlAttribueImpl("autocomplete", null);

            AddNonRemovableAttribute(this.action);
            AddNonRemovableAttribute(this.method);

            AddRange(children);
        }

        public Form(Func<HttpContextEx, IHttpHandler> f, params HTMLElement[] children)
            : this("/x/" + new OneTimeUseControlPoint(f).Key, "post", children) { }

        public string Action
        {
            get
            {
                return action.Value;
            }
            set
            {
                action.Value = value;
            }
        }

        public bool AutoComplete
        {
            get
            {
                return autocomplete.Value != null;
            }
            set
            {
                autocomplete.Value = value ? "autocomplete" : null;
            }
        }

        public string Method
        {
            get
            {
                return method.Value;
            }
            set
            {
                method.Value = value;
            }
        }
    }

    public abstract class StyleAttribute : HTMLAttribute, IEnumerable<StyleRule>
    {
        List<StyleRule> rules = new List<StyleRule>();

        public StyleAttribute()
            : base("style", null)
        {

        }

        public override string ToString()
        {
            return Name + ":" + string.Join(";", rules);
        }

        public void Add(StyleRule rule)
        {
            for (var i = 0; i < rules.Count; i++)
                if (rules[i] == rule)
                {
                    rules[i] = rule;
                    return;
                }

            rules.Add(rule);
        }

        public void Remove(string name)
        {
            for (var i = 0; i < rules.Count; i++)
            {
                if (rules[i].name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    rules.RemoveAt(i);
                    return;
                }
            }
        }

        public IEnumerator<StyleRule> GetEnumerator()
        {
            return rules.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public abstract class StyleRule : IEquatable<StyleRule>
    {
        internal readonly string name;

        public StyleRule(string name)
        {
            this.name = name;
        }

        protected abstract string GetRuleValue();

        public override string ToString()
        {
            return name + ":" + GetRuleValue();
        }

        public bool Equals(StyleRule other)
        {
            return other != null && other.name.Equals(this.name, StringComparison.OrdinalIgnoreCase);
        }

        static bool Equals(StyleRule x, StyleRule y)
        {
            if (object.ReferenceEquals(x, null))
                return object.ReferenceEquals(y, null);

            if (object.ReferenceEquals(y, null))
                return object.ReferenceEquals(x, null);

            return x.Equals(y);
        }

        public static bool operator ==(StyleRule x, StyleRule y)
        {
            return Equals(x, y);
        }

        public static bool operator !=(StyleRule x, StyleRule y)
        {
            return !Equals(x, y);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as StyleRule);
        }
    }
    
    public sealed class Forecolor : StyleRule
    {
        Func<string> get_value;

        public Forecolor(byte alpha, byte red, byte green, byte blue) : base("color")
        {
            get_value = () =>
            {
                var color = "#" + red.ToString("X") + green.ToString("X") + blue.ToString("X");

                if (alpha < 255)
                {
                    var opacity = alpha /255f;
                    
                    color += (";opacity:" + opacity + ";" + string.Format("filter:alpha(opacity = {0})", opacity * 10));
                }

                return color;
            };
        }

        public Forecolor(string name)
            : base("color")
        {
            get_value = () => name;            
        }

        protected override string GetRuleValue()
        {
            return get_value();
        }
    }

    public sealed class Border : StyleRule
    {
        byte thickness;
        string type, color;
        
        public Border(byte thickness, string type, int color)
            :this(thickness, type, "#" + ((color >> 16) & 255).ToString("X") + ((color >> 8) & 255).ToString("X") + (color & 255).ToString("X"))
        {
            
        }

        public Border(byte thickness, string type, string color) : base("border")
        {
            this.thickness = thickness;
            this.color = color;
            this.type = type;
        }

        protected override string GetRuleValue()
        {
            return thickness + "px " + type + " " + color;
        }
    }
}
