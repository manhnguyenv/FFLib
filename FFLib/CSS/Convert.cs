/*******************************************************
 * Project: FFLib V1.0
 * Title: ORMInterfaces.cs
 * Author: Phillip Bird of Fast Forward,LLC
 * Copyright © 2012 Fast Forward, LLC. 
 * Dual licensed under the MIT or GPL Version 2 licenses.
 * Use of any component of FFLib requires acceptance and adhearance 
 * to the terms of either the MIT License or the GNU General Public License (GPL) Version 2 exclusively.
 * Notification of license selection is not required and will be infered based on applicability.
 * Contributions to FFLib requires a contributor grant on file with Fast Forward, LLC.
********************************************************/
//disable Missing XML documentation warning
#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FFLib.CSS
{
    public class Convert
    {
        public enum UOM : int
        {
            pt, px, @in, cm, mm, em, pct //order and value must remain in sync with UOMIndex below.
        };
        public enum AbsoluteUOM : int { pt, px, @in, cm, mm }
        protected static int absThreshold = 5;//this is the index of the first relative UOM in the UOMIndex. i.e. all values less than this are absolute.
        protected static Dictionary<string, int> UOMIndex = new Dictionary<string, int>() { { "pt", 0 }, { "px", 1 }, { "in", 2 }, { "cm", 3 }, { "mm", 4 }, { "em", 5 }, { "pct", 6 }, { "%", 6 } }; //update absThreashold if this list is altered.
        protected static decimal[,] convertmatrix = {{1,1.333333333333333M,0.0138888888888889M,0.0352777777777778M,0.352777777777778M,0,0}
                                                    ,{0.75M,1,0.0104166666666667M,0.0264583333333333M,0.264583333333333M,0,0}
                                                    ,{72,54,1,2.54M,25.4M,1,100}
                                                    ,{28.34645669291339M,37.79527559055119M,0.3937007874015748M,1,10,0,0}
                                                    ,{2.834645669291339M,3.779527559055119M,0.03937007874015748M,0.1M,1,0,0}
                                                    ,{1,1,1,1,1,1,100}
                                                    ,{.01M,.01M,0.01M,0.01M,0.01M,0.01M,1}};
        protected static Regex cssUOMExpression = new Regex(@"^\s*(?:(?<value>-?[0-9\.]+)\s*(?<uom>(?:[a-z]+|%))?\s*)|(?<keyword>[\#a-z][a-z0-9\-_]*)\s*", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);
        protected static Regex cssRGBExpression1 = new Regex(@"^\s*#(?<r>[0-9A-F]{2})(?<g>[0-9A-F]{2})(?<b>[0-9A-F]{2}\s*$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);
        protected static Regex cssRGBExpression2 = new Regex(@"^\s*#(?<r>[0-9A-F]{1})(?<g>[0-9A-F]{1})(?<b>[0-9A-F]{1})\s*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);
        protected static Regex cssRGBExpression3 = new Regex(@"^\s*rgb\(\s*(?<r>[0-9]{1,3}),\s*(?<g>[0-9]{1,3}),\s*(?<b>[0-9]{1,3})\s*\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);
        protected static Regex cssRGBExpression4 = new Regex(@"^\s*(?<keyword>[a-z][a-z0-9\-_]*)\s*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);

        public static bool IsAbsolute(string cssValueText)
        {
            if (string.IsNullOrEmpty(cssValueText)) return false;
            if (cssValueText.Trim().ToLower() == "inherit") return false;
            Match match = cssUOMExpression.Match(cssValueText);

            if (!match.Success) return false;
            int idx;
            idx = Convert.IsUOMAbosulte(match.Groups["uom"].Value) ? 0 : 99;
            if (idx == 0 && !string.IsNullOrWhiteSpace(match.Groups["keyword"].Value) && match.Groups["keyword"].Value.Trim().ToLower() != "inherit") return true;
            if (idx < absThreshold) return true;
            return false;
        }
        protected static bool IsUOMAbosulte(string uom)
        {
            int idx = 0;
            UOMIndex.TryGetValue(uom, out idx);
            if (idx < absThreshold) return true;
            return false;
        }

        public static CssValueElements GetCssValueElements(string cssValue)
        {
            if (string.IsNullOrWhiteSpace(cssValue)) return new CssValueElements(null, null, null);
            Match match = cssUOMExpression.Match(cssValue);

            return new CssValueElements(match.Groups["value"].Value, match.Groups["uom"].Value, match.Groups["keyword"].Value);
        }

        public static UOM? UOMFromString(string uom)
        {
            if (string.IsNullOrWhiteSpace(uom)) return UOM.pt; //if UOM is null or blank then assume Points, the value is probably 0 or a keyword
            if (!UOMIndex.ContainsKey(uom)) return null;
            return (UOM)UOMIndex[uom];
        }

        public static decimal ConvertTo(string cssValueText, AbsoluteUOM outputUOM)
        {
            if (!Convert.IsAbsolute(cssValueText)) throw new ArgumentOutOfRangeException("Input Values must be in an Absolute UOM");
            CssValueElements cssValue = Convert.GetCssValueElements(cssValueText);
            UOM? inputUOM = Convert.UOMFromString(cssValue.UOM);

            if (!inputUOM.HasValue) throw new ArgumentOutOfRangeException("CSS Value UOM is unrecognized.");
            if (!string.IsNullOrWhiteSpace(cssValue.Keyword)) return 0;

            decimal decValue = 0;
            decimal.TryParse(cssValue.Value, out decValue);
            return convertmatrix[(int)inputUOM, (int)outputUOM] * decValue;
        }

        public static decimal CalcAbsoluteValue(string relativeValue, string baseValue)
        {
            if (string.IsNullOrWhiteSpace(relativeValue) || string.IsNullOrWhiteSpace(baseValue)) return 0;
            if (Convert.IsAbsolute(relativeValue)) throw new ArgumentOutOfRangeException("Input Value must be in a Relative UOM");
            if (!Convert.IsAbsolute(baseValue)) throw new ArgumentOutOfRangeException("baseValue must be in absolute units");

            if (relativeValue == "inherit") relativeValue = "100%";
            CssValueElements inValue = Convert.GetCssValueElements(relativeValue);
            CssValueElements bValue = Convert.GetCssValueElements(baseValue);

            if (string.IsNullOrWhiteSpace(inValue.Value)) throw new ArgumentOutOfRangeException("relativeValue: " + relativeValue + " not recognized as a number");
            if (string.IsNullOrWhiteSpace(bValue.Value)) throw new ArgumentOutOfRangeException("baseValue: " + baseValue + " not recognized as a number");

            UOM? inUOM = Convert.UOMFromString(inValue.UOM);
            UOM? baseUOM = Convert.UOMFromString(bValue.UOM);
            if (!inUOM.HasValue) throw new ArgumentOutOfRangeException("relativeValue UOM is unrecognized.");
            if (!baseUOM.HasValue) throw new ArgumentOutOfRangeException("baseValue UOM is unrecognized.");

            decimal bdecValue = 0;
            decimal.TryParse(bValue.Value, out bdecValue);
            decimal idecValue = 0;
            decimal.TryParse(inValue.Value, out idecValue);

            return idecValue * convertmatrix[(int)inUOM.Value, (int)baseUOM.Value] * bdecValue; 
        }


        public static decimal ValueToDecimal(string cssValueText)
        {
            CssValueElements inValue = Convert.GetCssValueElements(cssValueText);

            decimal cssValue = 0;
            decimal.TryParse(inValue.Value, out cssValue);

            return cssValue;
        }

        public static System.Drawing.Color ValueToRGB(string cssValueText)
        {
            Int32 R = 0;
            Int32 G = 0;
            Int32 B = 0;
            int rgbBase = 16;
            int shortCode = 0;

            //Debug.WriteLine(cssValueText);
            Match match = cssRGBExpression4.Match(cssValueText);

            string keyword = match.Groups["keyword"].Value;
            if (keyword.Trim().ToLower() == "transparent") return System.Drawing.Color.FromArgb(0, 0, 0, 0); //Aspose does not honor System.Drawing.Color.Transparent;
            if (!string.IsNullOrWhiteSpace(keyword) && CssColors.ContainsKey(keyword.Trim())) return Convert.ValueToRGB(CssColors[keyword]);

            if (!match.Success) match = cssRGBExpression1.Match(cssValueText);
            if (!match.Success) { match = cssRGBExpression2.Match(cssValueText); shortCode = 1; }
            if (!match.Success) { match = cssRGBExpression3.Match(cssValueText); rgbBase = 10; shortCode = 0; }


            R = string.IsNullOrEmpty(match.Groups["r"].Value) ? 0 : System.Convert.ToInt32(match.Groups["r"].Value, rgbBase);
            G = string.IsNullOrEmpty(match.Groups["g"].Value) ? 0 : System.Convert.ToInt32(match.Groups["g"].Value, rgbBase);
            B = string.IsNullOrEmpty(match.Groups["b"].Value) ? 0 : System.Convert.ToInt32(match.Groups["b"].Value, rgbBase);

            R += shortCode * R * 16;
            G += shortCode * G * 16;
            B += shortCode * B * 16;

            if (string.IsNullOrEmpty(keyword) && !match.Success) return System.Drawing.Color.Transparent;
            return System.Drawing.Color.FromArgb(R, G, B);
        }

        public static Dictionary<string, string> CssColors = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase){
            {"AliceBlue","#F0F8FF"},
            {"AntiqueWhite","#FAEBD7"},
            {"Aqua","#00FFFF"},
            {"Aquamarine","#7FFFD4"},
            {"Azure","#F0FFFF"},
            {"Beige","#F5F5DC"},
            {"Bisque","#FFE4C4"},
            {"Black","#000000"},
            {"BlanchedAlmond","#FFEBCD"},
            {"Blue","#0000FF"},
            {"BlueViolet","#8A2BE2"},
            {"Brown","#A52A2A"},
            {"BurlyWood","#DEB887"},
            {"CadetBlue","#5F9EA0"},
            {"Chartreuse","#7FFF00"},
            {"Chocolate","#D2691E"},
            {"Coral","#FF7F50"},
            {"CornflowerBlue","#6495ED"},
            {"Cornsilk","#FFF8DC"},
            {"Crimson","#DC143C"},
            {"Cyan","#00FFFF"},
            {"DarkBlue","#00008B"},
            {"DarkCyan","#008B8B"},
            {"DarkGoldenRod","#B8860B"},
            {"DarkGray","#A9A9A9"},
            {"DarkGreen","#006400"},
            {"DarkKhaki","#BDB76B"},
            {"DarkMagenta","#8B008B"},
            {"DarkOliveGreen","#556B2F"},
            {"Darkorange","#FF8C00"},
            {"DarkOrchid","#9932CC"},
            {"DarkRed","#8B0000"},
            {"DarkSalmon","#E9967A"},
            {"DarkSeaGreen","#8FBC8F"},
            {"DarkSlateBlue","#483D8B"},
            {"DarkSlateGray","#2F4F4F"},
            {"DarkTurquoise","#00CED1"},
            {"DarkViolet","#9400D3"},
            {"DeepPink","#FF1493"},
            {"DeepSkyBlue","#00BFFF"},
            {"DimGray","#696969"},
            {"DimGrey","#696969"},
            {"DodgerBlue","#1E90FF"},
            {"FireBrick","#B22222"},
            {"FloralWhite","#FFFAF0"},
            {"ForestGreen","#228B22"},
            {"Fuchsia","#FF00FF"},
            {"Gainsboro","#DCDCDC"},
            {"GhostWhite","#F8F8FF"},
            {"Gold","#FFD700"},
            {"GoldenRod","#DAA520"},
            {"Gray","#808080"},
            {"Green","#008000"},
            {"GreenYellow","#ADFF2F"},
            {"HoneyDew","#F0FFF0"},
            {"HotPink","#FF69B4"},
            {"IndianRed","#CD5C5C"},
            {"Indigo","#4B0082"},
            {"Ivory","#FFFFF0"},
            {"Khaki","#F0E68C"},
            {"Lavender","#E6E6FA"},
            {"LavenderBlush","#FFF0F5"},
            {"LawnGreen","#7CFC00"},
            {"LemonChiffon","#FFFACD"},
            {"LightBlue","#ADD8E6"},
            {"LightCoral","#F08080"},
            {"LightCyan","#E0FFFF"},
            {"LightGoldenRodYellow","#FAFAD2"},
            {"LightGray","#D3D3D3"},
            {"LightGreen","#90EE90"},
            {"LightPink","#FFB6C1"},
            {"LightSalmon","#FFA07A"},
            {"LightSeaGreen","#20B2AA"},
            {"LightSkyBlue","#87CEFA"},
            {"LightSlateGray","#778899"},
            {"LightSteelBlue","#B0C4DE"},
            {"LightYellow","#FFFFE0"},
            {"Lime","#00FF00"},
            {"LimeGreen","#32CD32"},
            {"Linen","#FAF0E6"},
            {"Magenta","#FF00FF"},
            {"Maroon","#800000"},
            {"MediumAquaMarine","#66CDAA"},
            {"MediumBlue","#0000CD"},
            {"MediumOrchid","#BA55D3"},
            {"MediumPurple","#9370DB"},
            {"MediumSeaGreen","#3CB371"},
            {"MediumSlateBlue","#7B68EE"},
            {"MediumSpringGreen","#00FA9A"},
            {"MediumTurquoise","#48D1CC"},
            {"MediumVioletRed","#C71585"},
            {"MidnightBlue","#191970"},
            {"MintCream","#F5FFFA"},
            {"MistyRose","#FFE4E1"},
            {"Moccasin","#FFE4B5"},
            {"NavajoWhite","#FFDEAD"},
            {"Navy","#000080"},
            {"OldLace","#FDF5E6"},
            {"Olive","#808000"},
            {"OliveDrab","#6B8E23"},
            {"Orange","#FFA500"},
            {"OrangeRed","#FF4500"},
            {"Orchid","#DA70D6"},
            {"PaleGoldenRod","#EEE8AA"},
            {"PaleGreen","#98FB98"},
            {"PaleTurquoise","#AFEEEE"},
            {"PaleVioletRed","#DB7093"},
            {"PapayaWhip","#FFEFD5"},
            {"PeachPuff","#FFDAB9"},
            {"Peru","#CD853F"},
            {"Pink","#FFC0CB"},
            {"Plum","#DDA0DD"},
            {"PowderBlue","#B0E0E6"},
            {"Purple","#800080"},
            {"Red","#FF0000"},
            {"RosyBrown","#BC8F8F"},
            {"RoyalBlue","#4169E1"},
            {"SaddleBrown","#8B4513"},
            {"Salmon","#FA8072"},
            {"SandyBrown","#F4A460"},
            {"SeaGreen","#2E8B57"},
            {"SeaShell","#FFF5EE"},
            {"Sienna","#A0522D"},
            {"Silver","#C0C0C0"},
            {"SkyBlue","#87CEEB"},
            {"SlateBlue","#6A5ACD"},
            {"SlateGray","#708090"},
            {"Snow","#FFFAFA"},
            {"SpringGreen","#00FF7F"},
            {"SteelBlue","#4682B4"},
            {"Tan","#D2B48C"},
            {"Teal","#008080"},
            {"Thistle","#D8BFD8"},
            {"Tomato","#FF6347"},
            {"Turquoise","#40E0D0"},
            {"Violet","#EE82EE"},
            {"Wheat","#F5DEB3"},
            {"White","#FFFFFF"},
            {"WhiteSmoke","#F5F5F5"},
            {"Yellow","#FFFF00"},
            {"YellowGreen","#9ACD32"}};

    }
}
