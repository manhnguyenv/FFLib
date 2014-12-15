/*******************************************************
 * Project: FFLib V1.0
 * Title: String.cs
 * Author: Phillip Bird of Fast Forward,LLC
 * Copyright © 2012 Fast Forward, LLC. 
 * Dual licensed under the MIT or GPL Version 2 licenses.
 * Use of any component of FFLib requires acceptance and adhearance 
 * to the terms of either the MIT License or the GNU General Public License (GPL) Version 2 exclusively.
 * Notification of license selection is not required and will be infered based on applicability.
 * Contributions to FFLib requires a contributor grant on file with Fast Forward, LLC.
********************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FFLib.Extensions
{
    public static class StringExtensions_General
    {
        public static string SubstringEx(this string self, int StartIndex, int Length)
        {
            if (Length > self.Length - StartIndex) Length = self.Length - StartIndex;
            return self.Substring(StartIndex, Length);
        }

        public static string[] Cut(this string self, int StartIndex, int Length)
        {
            if (StartIndex + Length > self.Length) Length = self.Length - StartIndex;

            string s = self.Substring(StartIndex, Length);
            string s1 = StartIndex > 0 ? self.Substring(0, StartIndex) : string.Empty;
            string s2 = StartIndex + Length < self.Length ? self.Substring(StartIndex + Length) : string.Empty;
            return new string[] {s1 + s2, s};
        }
    }

    public static partial class StringHelper
    {
        public static string IfNull(this string self, string Default){
            if (self == null) return Default;
            else return self;
        }
    }
}
