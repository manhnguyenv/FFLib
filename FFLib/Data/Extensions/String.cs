using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FFLib.Extensions
{
    public static class  StringExtensions
    {
        //public static string Join(this string self, string delimiter, int[] intArray){
        //    StringBuilder result = new StringBuilder();
        //    if (intArray.Length == 0 ) return string.Empty;
        //    result.Append(intArray[0].ToString());
        //    for (int i = 1; i < intArray.Length; i++)
        //        result.Append(delimiter + intArray[i].ToString());

        //    return result.ToString();
        //}

        public static string SqlQuote(this string self)
        {
            return "'"+self.Replace("'", "''")+"'";
        }

        public static string SqlEscape(this string self, bool QuoteResult)
        {
            Regex escapeRegEx = new Regex(@"([\[\]_%])");

            if (QuoteResult) return "'" + escapeRegEx.Replace(self.Replace("'", "''"), @"$1") + "'";
            return escapeRegEx.Replace(self.Replace("'", "''"), @"$1");
        }

        public static string RegexEscape(this string self)
        {
            if (self == null || string.IsNullOrEmpty(self)) return self;
            Regex escapeRegEx = new Regex(@"(?=[\\\^\$\(\)\[\{\|\.\*\+\?])");
            return escapeRegEx.Replace(self, "\\");
        }

        public static string Join(char delimiter, int[] list)
        {
            if (list == null || list.Length == 0) return string.Empty;
            System.Text.StringBuilder result = new StringBuilder();
            result.Append(list[0].ToString());
            for (int i = 1; i < list.Length; i++)
                result.Append(delimiter + list[i].ToString());
            return result.ToString();
        }
    }
}
