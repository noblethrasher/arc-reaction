using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ArcReaction
{
    public sealed class ImageText : ControlPoint, IHttpHandler
    {
        static readonly Regex hexPattern = new Regex("([0-9]|[A-F]){6}|([0-9]|[A-F]){3}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex rgbPattern = new Regex("(0|1)[0-9]{2}|2([0-4][0-9]|5[0-5]),(0|1)[0-9]{2}|2([0-4][0-9]|5[0-5]),(0|1)[0-9]{2}|2([0-4][0-9]|5[0-5])", RegexOptions.Compiled);

        static Dictionary<string, FontFamily> fontFamilies;

        static readonly string[] knownColors = new[]
            {
                "transparent",
                "aliceblue",
                "antiquewhite",
                "aqua",
                "aquamarine",
                "azure",
                "beige",
                "bisque",
                "black",
                "blanchedalmond",
                "blue",
                "blueviolet",
                "brown",
                "burlywood",
                "cadetblue",
                "chartreuse",
                "chocolate",
                "coral",
                "cornflowerblue",
                "cornsilk",
                "crimson",
                "cyan",
                "darkblue",
                "darkcyan",
                "darkgoldenrod",
                "darkgray",
                "darkgreen",
                "darkkhaki",
                "darkmagenta",
                "darkolivegreen",
                "darkorange",
                "darkorchid",
                "darkred",
                "darksalmon",
                "darkseagreen",
                "darkslateblue",
                "darkslategray",
                "darkturquoise",
                "darkviolet",
                "deeppink",
                "deepskyblue",
                "dimgray",
                "dodgerblue",
                "firebrick",
                "floralwhite",
                "forestgreen",
                "fuchsia",
                "gainsboro",
                "ghostwhite",
                "gold",
                "goldenrod",
                "gray",
                "green",
                "greenyellow",
                "honeydew",
                "hotpink",
                "indianred",
                "indigo",
                "ivory",
                "khaki",
                "lavender",
                "lavenderblush",
                "lawngreen",
                "lemonchiffon",
                "lightblue",
                "lightcoral",
                "lightcyan",
                "lightgoldenrodyellow",
                "lightgray",
                "lightgreen",
                "lightpink",
                "lightsalmon",
                "lightseagreen",
                "lightskyblue",
                "lightslategray",
                "lightsteelblue",
                "lightyellow",
                "lime",
                "limegreen",
                "linen",
                "magenta",
                "maroon",
                "mediumaquamarine",
                "mediumblue",
                "mediumorchid",
                "mediumpurple",
                "mediumseagreen",
                "mediumslateblue",
                "mediumspringgreen",
                "mediumturquoise",
                "mediumvioletred",
                "midnightblue",
                "mintcream",
                "mistyrose",
                "moccasin",
                "navajowhite",
                "navy",
                "oldlace",
                "olive",
                "olivedrab",
                "orange",
                "orangered",
                "orchid",
                "palegoldenrod",
                "palegreen",
                "paleturquoise",
                "palevioletred",
                "papayawhip",
                "peachpuff",
                "peru",
                "pink",
                "plum",
                "powderblue",
                "purple",
                "red",
                "rosybrown",
                "royalblue",
                "saddlebrown",
                "salmon",
                "sandybrown",
                "seagreen",
                "seashell",
                "sienna",
                "silver",
                "skyblue",
                "slateblue",
                "slategray",
                "snow",
                "springgreen",
                "steelblue",
                "tan",
                "teal",
                "thistle",
                "tomato",
                "turquoise",
                "violet",
                "wheat",
                "white",
                "whitesmoke",
                "yellow",
                "yellowgreen"
            };

        static ImageText()
        {
            fontFamilies = new Dictionary<string, FontFamily>();
            
            foreach (var font in FontFamily.Families)
            {
                var name = font.Name.ToUpper();

                if (!fontFamilies.ContainsKey(name))
                    fontFamilies.Add(name, font);
            }
        }
        
        public ControlPoint Next(Message msg)
        {
            return null;
        }

        public System.Web.IHttpHandler GetHandler(HttpContextEx context)
        {
            return this;
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            string text;

            if ((text = context.Request.QueryString["e"]) != null)
                text = Encoding.UTF32.GetString(Convert.FromBase64String(text.Replace('-', '=')));
            else
                text = context.Request.QueryString["t"] ?? "";            
            
            var color = ParseColor(context.Request.QueryString["c"], Color.Black);

            FontFamily fontFamily;
            sbyte size;


            var styles = context.Request.QueryString["v"] ?? "REGULAR";


            FontStyle fontStyle = 0;

            foreach (var style in styles.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries))
            {
                if (style.Equals("regular", StringComparison.OrdinalIgnoreCase))
                    fontStyle |= FontStyle.Regular;

                if (style.Equals("bold", StringComparison.OrdinalIgnoreCase))
                    fontStyle |= FontStyle.Bold;

                if (style.Equals("italic", StringComparison.OrdinalIgnoreCase))
                    fontStyle |= FontStyle.Italic;

                if (style.Equals("strikeout", StringComparison.OrdinalIgnoreCase))
                    fontStyle |= FontStyle.Strikeout;

                if (style.Equals("underline", StringComparison.OrdinalIgnoreCase))
                    fontStyle |= FontStyle.Underline;
            }

            if (!fontFamilies.TryGetValue((context.Request.QueryString["f"] ?? "").ToUpper(), out fontFamily))
                fontFamily = FontFamily.GenericSerif;

            if (!sbyte.TryParse(context.Request.QueryString["z"] ?? "", out size))
                size = 14;
            
            var backColor = ParseColor(context.Request.QueryString["b"], Color.Transparent);

            var font = new Font(fontFamily, size, fontStyle);

            SizeF sizeF = new SizeF();

            using (var gfx = Graphics.FromImage(new Bitmap(1, 1)))
                sizeF = gfx.MeasureString(text, font);

            var bmp = new Bitmap((int)Math.Ceiling(sizeF.Width), (int)Math.Ceiling(sizeF.Height));            

            using (var gfx = Graphics.FromImage(bmp))
            {
                gfx.Clear(backColor);
                gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                gfx.DrawString(text, font, new SolidBrush(color), new PointF(0, 0));
            }

            bmp.Save(context.Response.OutputStream, ImageFormat.Png);
            context.Response.ContentType = "image/png";
            context.Response.Cache.SetCacheability(HttpCacheability.Public);            
        }        

        static Color ParseColor(string s, Color? defaultColor = null)
        {
            s = new string((s ?? "").Take(6).ToArray());

            foreach (var c in knownColors)
                if (c.Equals(s, StringComparison.OrdinalIgnoreCase))
                    return Color.FromName(c);

            var match = hexPattern.Match(s);

            if (match.Success)
            {
                var val = match.Value;

                if (val.Length == 6)
                    return Color.FromArgb(255, Color.FromArgb(int.Parse(val, System.Globalization.NumberStyles.HexNumber)));

                else //match.Value.Length should be 3
                {
                    int r = 0, g = 0, b = 0;
                    
                    r = Convert.ToInt32(new string(new char[] { val[0], val[0] }), 16);
                    g = Convert.ToInt32(new string(new char[] { val[1], val[1] }), 16);
                    b = Convert.ToInt32(new string(new char[] { val[2], val[2] }), 16);

                    return Color.FromArgb(r, g, b);
                }                
            }

            return defaultColor ?? Color.White;
        }
    }
}
