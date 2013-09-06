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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFLib.CSS;
using NUnit;
using NUnit.Framework;

namespace FFLibUnitTests
{
    [TestFixture]
    public class CSSParserTest
    {
        [Test]
        public void ParseFiles()
        {
            string[] cssFiles = { "data\\ui.css", "data\\customui.css", "data\\document.css", "data\\customdoc.css" };

            CSSParser parser = new CSSParser();

            List<ICSSRule> rules = new List<ICSSRule>(10000);

            rules.AddRange( parser.ParseCSSString(FFLib.CSS.Util.BrowserBaseCSS));
            rules.AddRange( parser.ParseCSSFiles(cssFiles));

            Console.WriteLine("# of Parsed rules: " + rules.Count.ToString());

            Console.WriteLine(rules[12].Declarations[0].ToString());
            Console.WriteLine(rules[100].Declarations[0].ToString());
            Console.WriteLine(rules[120].Declarations[0].ToString());
            Console.WriteLine(rules[220].Declarations[0].ToString());
            Assert.Greater(rules.Count, 1);
        }
        [Test]
        public void ParseInlineStyles()
        {
            string cssString = "#inline { color:blue; font-size:16px; background:transparent;}";

            CSSParser parser = new CSSParser();

            List<ICSSRule> rules = new List<ICSSRule>(10000);

            rules.AddRange(parser.ParseCSSString(cssString));

            Console.WriteLine("# of Parsed rules: " + rules.Count.ToString());
            Console.WriteLine(rules[0].Selector.Segments[0].ToString());
            //Console.WriteLine(rules[12].Declarations[0].ToString());
            //Console.WriteLine(rules[100].Declarations[0].ToString());
            //Console.WriteLine(rules[120].Declarations[0].ToString());
            //Console.WriteLine(rules[220].Declarations[0].ToString());
            Assert.Greater(rules.Count, 0);
        }
    }
}