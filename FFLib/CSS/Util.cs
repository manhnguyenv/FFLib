using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FFLib.CSS
{
    public class Util
    {
        public static Regex fontRegex = new Regex(
        @"^(?<style>normal|italic|oblique|inherit)?\s*(?<variant>normal|small-caps|inherit)?\s*(?<weight>normal|bold|bolder|lighter|100|200|300|400|500|600|700|800|900)?\s*(?<size>[0-9\.]+(?:pt|px|em|in|cm|pc|%)|small|medium|large|x-large|larger|smaller|inherit)\s*(?<height>/[0-9\.]+(?:pt|px|em|in|cm|pc|%))?\s*(?<font>.+)"
        , RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static Regex borderRegex = new Regex(@"^(?<width>[0-9\.]+(?:pt|px|em|in|cm|pc|%)|thin|medium|thick)?\s*(?<style>none|hidden|dotted|dashed|solid|double|groove|ridge|inset|outset|inherit)?\s*(?<color>.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static Regex listRegex = new Regex(@"^(?<type>disc|circle|square|decimal|decimal-leading-zero|lower-roman|upper-roman|lower-greek|lower-latin|upper-latin|armenian|georgian|lower-alpha|upper-alpha|none)?\s*(?<image>none|inherit|url\(.+?\))?\s*(?<position>inside|outside|inherit)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static Regex BackgrdRegex = new Regex(@"^(?<color>\w+|#[0-9a-fA-F]+)\s+(url\((?<url>.+?)\))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static string BrowserBaseCSS { get { return FFLib.Properties.Resources.Base_CSS; } }
        public delegate CSSDeclaration[] SHExpanderDelegate(string name, string value);

        protected static Dictionary<string, SHExpanderDelegate> ShortHandProperties = new Dictionary<string, SHExpanderDelegate>(StringComparer.CurrentCultureIgnoreCase) { { "margin", SHE_Margin }, { "padding", SHE_Padding }, { "font", SHE_Font }, { "border", SHE_Border }, { "border-left", SHE_Border_Left }, { "border-right", SHE_Border_Right }, { "border-top", SHE_Border_Top }, { "border-bottom", SHE_Border_Bottom }, { "list-style", SHE_List }, {"background", SHE_Background}, {"border-color", SHE_BorderColor} };
        
        public static bool IsShorthandProperty(string name){ return ShortHandProperties.ContainsKey(name);}

        public static CSSDeclaration[] ExpandShortHandProperties(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            if (string.IsNullOrWhiteSpace(value)) return null;

            SHExpanderDelegate func = ShortHandProperties[name];
            if (func != null) return func(name, value);

            return null;
        }

        public static CSSDeclaration[] SHE_Margin(string name, string value)
        {
            string[] values = value.Split(new char[] { ' ', '\t', '\n', '\r' });
            switch (values.Length)
            {
                case 0: return null;
                case 1:
                    {
                        CSSDeclaration[] d = new CSSDeclaration[4];
                        d[0] = new CSSDeclaration("margin-top", values[0]);
                        d[1] = new CSSDeclaration("margin-right", values[0]);
                        d[2] = new CSSDeclaration("margin-bottom", values[0]);
                        d[3] = new CSSDeclaration("margin-left", values[0]);
                        return d;
                    }
                case 2:
                    {
                        CSSDeclaration[] d = new CSSDeclaration[4];
                        d[0] = new CSSDeclaration("margin-top", values[0]);
                        d[1] = new CSSDeclaration("margin-right", values[1]);
                        d[2] = new CSSDeclaration("margin-bottom", values[0]);
                        d[3] = new CSSDeclaration("margin-left", values[1]);
                        return d;
                    }
                case 3:
                    {
                        CSSDeclaration[] d = new CSSDeclaration[4];
                        d[0] = new CSSDeclaration("margin-top", values[0]);
                        d[1] = new CSSDeclaration("margin-right", values[1]);
                        d[2] = new CSSDeclaration("margin-bottom", values[2]);
                        d[3] = new CSSDeclaration("margin-left", values[1]);
                        return d;
                    }
                case 4:
                    {
                        CSSDeclaration[] d = new CSSDeclaration[4];
                        d[0] = new CSSDeclaration("margin-top", values[0]);
                        d[1] = new CSSDeclaration("margin-right", values[1]);
                        d[2] = new CSSDeclaration("margin-bottom", values[2]);
                        d[3] = new CSSDeclaration("margin-left", values[3]);
                        return d;
                    }
            }
            return new CSSDeclaration[]{new CSSDeclaration(name, value)};
        }
        public static CSSDeclaration[] SHE_Padding(string name, string value)
        {
            string[] values = value.Split(new char[] { ' ', '\t', '\n', '\r' });
            switch (values.Length)
            {
                case 0: return null;
                case 1:
                    {
                        CSSDeclaration[] d = new CSSDeclaration[4];
                        d[0] = new CSSDeclaration("padding-top", values[0]);
                        d[1] = new CSSDeclaration("padding-right", values[0]);
                        d[2] = new CSSDeclaration("padding-bottom", values[0]);
                        d[3] = new CSSDeclaration("padding-left", values[0]);
                        return d;
                    }
                case 2:
                    {
                        CSSDeclaration[] d = new CSSDeclaration[4];
                        d[0] = new CSSDeclaration("padding-top", values[0]);
                        d[1] = new CSSDeclaration("padding-right", values[1]);
                        d[2] = new CSSDeclaration("padding-bottom", values[0]);
                        d[3] = new CSSDeclaration("padding-left", values[1]);
                        return d;
                    }
                case 3:
                    {
                        CSSDeclaration[] d = new CSSDeclaration[4];
                        d[0] = new CSSDeclaration("padding-top", values[0]);
                        d[1] = new CSSDeclaration("padding-right", values[1]);
                        d[2] = new CSSDeclaration("padding-bottom", values[2]);
                        d[3] = new CSSDeclaration("padding-left", values[1]);
                        return d;
                    }
                case 4:
                    {
                        CSSDeclaration[] d = new CSSDeclaration[4];
                        d[0] = new CSSDeclaration("padding-top", values[0]);
                        d[1] = new CSSDeclaration("padding-right", values[1]);
                        d[2] = new CSSDeclaration("padding-bottom", values[2]);
                        d[3] = new CSSDeclaration("padding-left", values[3]);
                        return d;
                    }
            }
            return new CSSDeclaration[] { new CSSDeclaration(name, value) };
        }
        public static CSSDeclaration[] SHE_Font(string name, string value)
        {
            Match m = fontRegex.Match(value);
            if (m == null || !m.Success) return new CSSDeclaration[] { new CSSDeclaration(name, value) };
            List<CSSDeclaration> d = new List<CSSDeclaration>();
            if (!string.IsNullOrWhiteSpace(m.Groups["style"].Value)) d.Add(new CSSDeclaration("font-style", m.Groups["style"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["variant"].Value)) d.Add(new CSSDeclaration("font-variant", m.Groups["variant"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["weight"].Value)) d.Add(new CSSDeclaration("font-weight", m.Groups["weight"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["size"].Value)) d.Add(new CSSDeclaration("font-size", m.Groups["size"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["height"].Value)) d.Add(new CSSDeclaration("line-height", m.Groups["style"].Value.Substring(1)));
            if (!string.IsNullOrWhiteSpace(m.Groups["font"].Value)) d.Add(new CSSDeclaration("font-family", m.Groups["font"].Value));

            return d.ToArray();
        }
        public static CSSDeclaration[] SHE_Border(string name, string value)
        {
            Match m = borderRegex.Match(value);
            if (m == null || !m.Success) return new CSSDeclaration[] { new CSSDeclaration(name, value) };
            List<CSSDeclaration> d = new List<CSSDeclaration>();
            //top
            if (!string.IsNullOrWhiteSpace(m.Groups["width"].Value)) d.Add(new CSSDeclaration("border-top-width", m.Groups["width"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["style"].Value)) d.Add(new CSSDeclaration("border-top-style", m.Groups["style"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["color"].Value)) d.Add(new CSSDeclaration("border-top-color", m.Groups["color"].Value));
            //left
            if (!string.IsNullOrWhiteSpace(m.Groups["width"].Value)) d.Add(new CSSDeclaration("border-left-width", m.Groups["width"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["style"].Value)) d.Add(new CSSDeclaration("border-left-style", m.Groups["style"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["color"].Value)) d.Add(new CSSDeclaration("border-left-color", m.Groups["color"].Value));
            //right
            if (!string.IsNullOrWhiteSpace(m.Groups["width"].Value)) d.Add(new CSSDeclaration("border-right-width", m.Groups["width"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["style"].Value)) d.Add(new CSSDeclaration("border-right-style", m.Groups["style"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["color"].Value)) d.Add(new CSSDeclaration("border-right-color", m.Groups["color"].Value));
            //bottom
            if (!string.IsNullOrWhiteSpace(m.Groups["width"].Value)) d.Add(new CSSDeclaration("border-bottom-width", m.Groups["width"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["style"].Value)) d.Add(new CSSDeclaration("border-bottom-style", m.Groups["style"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["color"].Value)) d.Add(new CSSDeclaration("border-bottom-color", m.Groups["color"].Value));

            return d.ToArray();
        }
        public static CSSDeclaration[] SHE_Border_Left(string name, string value)
        {
            Match m = borderRegex.Match(value);
            if (m == null || !m.Success) return new CSSDeclaration[] { new CSSDeclaration(name, value) };
            List<CSSDeclaration> d = new List<CSSDeclaration>();
            //top
            if (!string.IsNullOrWhiteSpace(m.Groups["width"].Value)) d.Add(new CSSDeclaration("border-left-width", m.Groups["width"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["style"].Value)) d.Add(new CSSDeclaration("border-left-style", m.Groups["style"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["color"].Value)) d.Add(new CSSDeclaration("border-left-color", m.Groups["color"].Value));

            return d.ToArray();
        }
        public static CSSDeclaration[] SHE_Border_Right(string name, string value)
        {
            Match m = borderRegex.Match(value);
            if (m == null || !m.Success) return new CSSDeclaration[] { new CSSDeclaration(name, value) };
            List<CSSDeclaration> d = new List<CSSDeclaration>();
            //top
            if (!string.IsNullOrWhiteSpace(m.Groups["width"].Value)) d.Add(new CSSDeclaration("border-right-width", m.Groups["width"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["style"].Value)) d.Add(new CSSDeclaration("border-right-style", m.Groups["style"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["color"].Value)) d.Add(new CSSDeclaration("border-right-color", m.Groups["color"].Value));

            return d.ToArray();
        }
        public static CSSDeclaration[] SHE_Border_Top(string name, string value)
        {
            Match m = borderRegex.Match(value);
            if (m == null || !m.Success) return new CSSDeclaration[] { new CSSDeclaration(name, value) };
            List<CSSDeclaration> d = new List<CSSDeclaration>();
            //top
            if (!string.IsNullOrWhiteSpace(m.Groups["width"].Value)) d.Add(new CSSDeclaration("border-top-width", m.Groups["width"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["style"].Value)) d.Add(new CSSDeclaration("border-top-style", m.Groups["style"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["color"].Value)) d.Add(new CSSDeclaration("border-top-color", m.Groups["color"].Value));

            return d.ToArray();
        }
        public static CSSDeclaration[] SHE_Border_Bottom(string name, string value)
        {
            Match m = borderRegex.Match(value);
            if (m == null || !m.Success) return new CSSDeclaration[] { new CSSDeclaration(name, value) };
            List<CSSDeclaration> d = new List<CSSDeclaration>();
            //top
            if (!string.IsNullOrWhiteSpace(m.Groups["width"].Value)) d.Add(new CSSDeclaration("border-bottom-width", m.Groups["width"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["style"].Value)) d.Add(new CSSDeclaration("border-bottom-style", m.Groups["style"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["color"].Value)) d.Add(new CSSDeclaration("border-bottom-color", m.Groups["color"].Value));

            return d.ToArray();
        }
        public static CSSDeclaration[] SHE_List(string name, string value)
        {
            Match m = listRegex.Match(value);
            if (m == null || !m.Success) return new CSSDeclaration[] { new CSSDeclaration(name, value) };
            List<CSSDeclaration> d = new List<CSSDeclaration>();
            //top
            if (!string.IsNullOrWhiteSpace(m.Groups["type"].Value)) d.Add(new CSSDeclaration("list-style-type", m.Groups["type"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["image"].Value)) d.Add(new CSSDeclaration("list-style-image", m.Groups["image"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["position"].Value)) d.Add(new CSSDeclaration("list-style-position", m.Groups["position"].Value));

            return d.ToArray();
        }
        public static CSSDeclaration[] SHE_Background(string name, string value)
        {
            Match m = BackgrdRegex.Match(value);
            if (m == null || !m.Success) return new CSSDeclaration[] { new CSSDeclaration(name, value) };
            List<CSSDeclaration> d = new List<CSSDeclaration>();
            //top
            if (!string.IsNullOrWhiteSpace(m.Groups["color"].Value)) d.Add(new CSSDeclaration("background-color", m.Groups["color"].Value));
            if (!string.IsNullOrWhiteSpace(m.Groups["url"].Value)) d.Add(new CSSDeclaration("background-image", m.Groups["url"].Value));

            return d.ToArray();
        }
        public static CSSDeclaration[] SHE_BorderColor(string name, string value)
        {
            Match m = borderRegex.Match(value);
            if (m == null || !m.Success) return new CSSDeclaration[] { new CSSDeclaration(name, value) };
            List<CSSDeclaration> d = new List<CSSDeclaration>();
            //top
            if (!string.IsNullOrWhiteSpace(value)) d.Add(new CSSDeclaration("border-top-color", value));
            //left
            if (!string.IsNullOrWhiteSpace(value)) d.Add(new CSSDeclaration("border-left-color", value));
            //right
            if (!string.IsNullOrWhiteSpace(value)) d.Add(new CSSDeclaration("border-right-color", value));
            //bottom
            if (!string.IsNullOrWhiteSpace(value)) d.Add(new CSSDeclaration("border-bottom-color", value));

            return d.ToArray();
        }

        protected static Dictionary<string, string> KeywordMap = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase) { { "inherit", "100%" }, { "auto", "1pt" } };
        protected static Dictionary<string,Dictionary<string,string>> StyleKeywordMap = new Dictionary<string,Dictionary<string,string>>(StringComparer.CurrentCultureIgnoreCase){
            {"line-height",new Dictionary<string,string>(StringComparer.CurrentCultureIgnoreCase) {{"normal","120%"},{"inherit","100%"}}},
            {"margin",new Dictionary<string,string>(StringComparer.CurrentCultureIgnoreCase) {{"auto","1px"}}}};

        public static string ResolveKeywordValue(string styleName, string v)
        {
            if (KeywordMap.ContainsKey(v)) return KeywordMap[v];
            if (!StyleKeywordMap.ContainsKey(styleName)) return v;
            if (!StyleKeywordMap[styleName].ContainsKey(v)) return v;
            return StyleKeywordMap[styleName][v];
        }
    }
}
