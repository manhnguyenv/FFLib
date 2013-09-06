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
    public class CSSResolverHelperTests
    {

        [Test]
        public void IndexRules()
        {
            CSSResolverHelper rh = new CSSResolverHelper();
            ICSSRule[] rules = RuleList1();

            CSSResolver.RuleIndex ri = rh.IndexRules(rules);

            Assert.AreEqual(3, ri.Count);
            Assert.AreEqual(2, ri["div"].Rules.Count);
            Assert.AreEqual(1, ri["p"].Rules.Count);
            Assert.AreEqual(1, ri[".alert"].Rules.Count());
            Assert.AreEqual(2, ri[".alert"].Count());
            Assert.AreEqual(0, ri[".alert"]["p"].Count());
            Assert.AreEqual(1, ri[".alert"]["p"].Rules.Count());
        }

        [Test]
        public void GetRules()
        {
            CSSCoreResolver cr = new CSSCoreResolver(new CSSParser());
            CSSResolverHelper rh = new CSSResolverHelper();
            ICSSRule[] rules = RuleList1();
            CSSResolver.IDOMNode node = this.DOMBranch1_leaf();
            CSSResolver.RuleIndex ri = rh.IndexRules(rules);
            cr.Initialize(ri);
            rules = cr.GetRules(node);

            Assert.AreEqual(3, rules.Length);
            Assert.AreEqual(true, rules[0].Selector.RawValue == ".alert");
            Assert.AreEqual(true, rules[1].Selector.RawValue == "div p.alert");
            Assert.AreEqual(true, rules[2].Selector.RawValue == "p");
        }

        [Test]
        public void OrderBySpecificity()
        {
            CSSCoreResolver rh = new CSSCoreResolver(new CSSParser());
            ICSSRule[] rules = RuleList1();
            int l = rules.Length;

            rules = rh.OrderBySpecificity(rules);

            Assert.AreEqual(l, rules.Length);
            Assert.AreEqual(true, rules[0].Selector.RawValue == "div");
            Assert.AreEqual(true, rules[3].Selector.RawValue == ".alert");
            Assert.AreEqual(true, rules[5].Selector.RawValue == "div p.alert");
            Assert.AreEqual(true, rules[7].Selector.RawValue == "#header.leftside span.alert");
        }

        #region Classes
        protected class DomNode : CSSResolver.IDOMNode
        {
            public string ElementName
            {
                get;
                set;
            }

            public string Id
            {
                get;
                set;
            }

            public string Class
            {
                get;
                set;
            }

            public CSSResolver.IDOMNode ParentNode
            {
                get;
                set;
            }

            public string GetAttribute(string name) { return null; }

        }

        #endregion

        #region Test_Data

        /// <summary>
        /// returns the leaf node of a single branch of a DOM Tree.
        /// </summary>
        /// <returns></returns>
        protected CSSResolver.IDOMNode DOMBranch1_leaf()
        {
            DomNode node = new DomNode();
            CSSResolver.IDOMNode leaf = node;

            node.ElementName = "p";
            node.Class = "alert";
            node.ParentNode = new DomNode();

            node = (DomNode)node.ParentNode;
            node.ElementName = "form";
            node.ParentNode = new DomNode();

            node = (DomNode)node.ParentNode;
            node.ElementName = "div";
            node.Class = "leftside";
            node.Id = "header";
            node.ParentNode = new DomNode();

            node = (DomNode)node.ParentNode;
            node.ElementName = "body";
            node.ParentNode = new DomNode();

            node = (DomNode)node.ParentNode;
            node.ElementName = "html";
            node.ParentNode = new DomNode();

            return leaf;
        }

        protected ICSSRule[] RuleList1()
        {
            List<ICSSRule> list = new List<ICSSRule>();
            list.Add(new CSSRule(new CSSSelector("div"), new CSSDeclaration[] { new CSSDeclaration("position", "static"), new CSSDeclaration("text-align", "center"), new CSSDeclaration("color", "#fff") }));
            list.Add(new CSSRule(new CSSSelector("div"), new CSSDeclaration[] { new CSSDeclaration("color", "#ffc"), new CSSDeclaration("margin-top", "auto"), new CSSDeclaration("padding-top", "0") }));
            list.Add(new CSSRule(new CSSSelector("p"), new CSSDeclaration[] { new CSSDeclaration("color", "#11c"), new CSSDeclaration("margin-top", "2pt"), new CSSDeclaration("padding-left", "10pt") }));
            list.Add(new CSSRule(new CSSSelector("div p.alert"), new CSSDeclaration[] { new CSSDeclaration("color", "#f11"), new CSSDeclaration("font-weight", "bold") }));
            list.Add(new CSSRule(new CSSSelector(".alert"), new CSSDeclaration[] { new CSSDeclaration("color", "#fd1"), new CSSDeclaration("font-weight", "normal") }));
            list.Add(new CSSRule(new CSSSelector("span.alert"), new CSSDeclaration[] { new CSSDeclaration("background-color", "#dd0")}));
            list.Add(new CSSRule(new CSSSelector("table .leftside span.alert"), new CSSDeclaration[] { new CSSDeclaration("background-color", "#dd0") }));
            list.Add(new CSSRule(new CSSSelector("#header.leftside span.alert"), new CSSDeclaration[] { new CSSDeclaration("background-color", "#dd0") }));
            return list.ToArray();
        }
        #endregion
    }
}
