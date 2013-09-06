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
    public class IntegrationTests
    {
        [Test]
        public void GetNodeRules()
        {
            string[] cssFiles = { "data\\ui.css", "data\\customui.css", "data\\document.css", "data\\customdoc.css" };

            CSSParser parser = new CSSParser();

            ICSSRule[] rules = parser.ParseCSSFiles(cssFiles);
            Console.WriteLine(rules.Length + " Rules parsed.");
            CSSCoreResolver cr = new CSSCoreResolver(parser);
            CSSResolverHelper rh = new CSSResolverHelper();

            CSSResolver.IDOMNode node = this.DOMBranch1_leaf();
                CSSResolver.RuleIndex ri = rh.IndexRules(rules);
                cr.Initialize(ri);
                rules = cr.GetRules(node);

            rules = cr.OrderBySpecificity(rules);
            Console.WriteLine(rules.Length.ToString() + " rules match node: " + node.ElementName + " class='" + node.Class + "'");
        }

        [Test]
        public void GetNodeStyles()
        {
            string[] cssFiles = { "data\\ui.css", "data\\customui.css", "data\\document.css", "data\\customdoc.css" };

            CSSParser parser = new CSSParser();

            List<ICSSRule> ruleslist = new List<ICSSRule>();
            ruleslist.AddRange(parser.ParseCSSString(FFLib.CSS.Util.BrowserBaseCSS));
            ruleslist.AddRange(parser.ParseCSSFiles(cssFiles));
            ICSSRule[] rules = ruleslist.ToArray();
            Console.WriteLine(rules.Length + " Rules parsed.");
            ICSSCoreResolver cr = new CSSCoreResolver(parser);
            CSSResolver resolver = new CSSResolver(cr);
            resolver.Initialize(rules);

            CSSResolver.IDOMNode node = this.ParseHTML(System.IO.File.ReadAllText("data\\complexHTML1.htm"));

            
            XNode roottestnode = ((XNode)node).ChildNodes[3].ChildNodes[3].ChildNodes[3].ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[1];

            Console.WriteLine("Test Root:" + roottestnode);
            Console.WriteLine("child nodes:" + roottestnode.ChildNodes.Count);
            XNode nodeT1 = roottestnode.ChildNodes[5];
            XNode nodeT2 = roottestnode.ChildNodes[7];

            Console.WriteLine(rules.Length.ToString() + " rules match node: " + node.ElementName + " class='" + node.Class + "'");
            //IDOMNodeStyleSet ds = resolver.GetDOMNodeStyleSet(nodeT1);
            IDOMNodeStyleSet ds;
            foreach (var cn in roottestnode.ChildNodes)
            {
                if (cn.ElementName.StartsWith("#")) continue;
                ds = resolver.GetDOMNodeStyleSet(cn);

                Console.WriteLine(cn.ElementName);
                Console.WriteLine("color:" + ds.GetValue("color") + (ds.GetValue("color").IsInherited ? " : inherited" : ""));
                Console.WriteLine("font-size:" + ds.GetValue("font-size") + (ds.GetValue("font-size").IsInherited ? " : inherited" : ""));
                Console.WriteLine("font-size:" + ds.GetComputedValue("font-size") + (ds.GetValue("font-size").IsInherited ? " : inherited" : ""));
                Console.WriteLine("font-weight:" + ds.GetComputedValue("font-weight") + (ds.GetComputedValue("font-weight") != null && ds.GetComputedValue("font-weight").IsInherited ? " : inherited" : ""));
                Console.WriteLine("font-family:" + ds.GetValue("font-family"));
                Console.WriteLine("margin:" + ds.GetValue("margin"));
                Console.WriteLine("margin-top:" + ds.GetValue("margin-top"));
                Console.WriteLine("margin-top:" + ds.GetComputedValue("margin-top"));
                Console.WriteLine("margin-right:" + ds.GetValue("margin-right"));
                Console.WriteLine("margin-bottom:" + ds.GetValue("margin-bottom"));
                Console.WriteLine("margin-left:" + ds.GetValue("margin-left"));
                Console.WriteLine("padding:" + ds.GetValue("padding"));
                Console.WriteLine("text-align:" + ds.GetComputedValue("text-align") + (ds.GetValue("text-align").IsInherited ? " : inherited" : ""));
                Console.WriteLine("background-color:" + ds.GetValue("background-color"));

                
            }
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

        public class XNode : CSSResolver.IDOMNode
        {

            public HtmlAgilityPack.HtmlNode HtmlNode;
            public XNodeList _childNodes;
            public delegate XNode OwnerDelegate();
            protected internal OwnerDelegate GetParent;

            public XNode(HtmlAgilityPack.HtmlNode htmlNode)
            {
                HtmlNode = htmlNode;
                ChildNodes = new XNodeList(this);
            }

            public XNodeList ChildNodes { get { return _childNodes; } set { _childNodes = value; _childNodes._owner = this; } }

            public string getAttribute(string name)
            {
                if (HtmlNode == null) return null;
                return HtmlNode.GetAttributeValue(name, null);
            }

            public CSSResolver.IDOMNode ParentNode
            {
                get { if (this.GetParent == null) return null; else return this.GetParent(); }
            }

            public string Id { get { return HtmlNode.Id; } }
            public string ElementName { get { return HtmlNode.Name; } }
            public string Class { get { return HtmlNode.GetAttributeValue("class",""); } }

            public string GetAttribute(string name) { return null; }
        }

        public class XNodeList : IList<XNode>
        {
            protected List<XNode> _innerList = new List<XNode>();
            protected internal XNode _owner;

            public XNodeList(XNode Owner) { _owner = Owner; }

            public XNode Owner() { return _owner; }

            public int IndexOf(XNode item)
            {
                return _innerList.IndexOf(item);
            }

            public void Insert(int index, XNode item)
            {
                _innerList.Insert(index, item);
                item.GetParent = new XNode.OwnerDelegate(this.Owner);

            }

            public void RemoveAt(int index)
            {
                _innerList[index].GetParent = null;
                _innerList.RemoveAt(index);
            }

            public XNode this[int index]
            {
                get
                {
                    return _innerList[index];
                }
                set
                {
                    _innerList[index] = value;
                }
            }

            public void Add(XNode item)
            {
                _innerList.Add(item);
                item.GetParent = new XNode.OwnerDelegate(this.Owner);

            }

            public void AddRange(XNode[] items)
            {
                _innerList.AddRange(items);
                foreach (XNode item in items)
                {
                    item.GetParent = new XNode.OwnerDelegate(this.Owner);
                }

            }

            public void Clear()
            {
                _innerList.Clear();
            }

            public bool Contains(XNode item)
            {
                return _innerList.Contains(item);
            }

            public void CopyTo(XNode[] array, int arrayIndex)
            {
                _innerList.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return _innerList.Count; }
            }

            public bool IsReadOnly
            {
                get { return IsReadOnly; }
            }

            public bool Remove(XNode item)
            {
                _innerList[_innerList.IndexOf(item)].GetParent = null;
                return _innerList.Remove(item);
            }

            public IEnumerator<XNode> GetEnumerator()
            {
                return _innerList.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return _innerList.GetEnumerator();
            }
        }
        #endregion

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
            node.ElementName = "div";
            node.ParentNode = new DomNode();

            node = (DomNode)node.ParentNode;
            node.ElementName = "div";
            node.Class = "leftside";
            node.Id = "#doc-view";
            node.ParentNode = new DomNode();

            node = (DomNode)node.ParentNode;
            node.ElementName = "body";
            node.ParentNode = new DomNode();

            node = (DomNode)node.ParentNode;
            node.ElementName = "html";
            node.ParentNode = new DomNode();

            return leaf;
        }

        public virtual XNode ParseHTML(string htmlContent)
        {

            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);
            HtmlAgilityPack.HtmlNode TocRoot = default(HtmlAgilityPack.HtmlNode);

            TocRoot = htmlDoc.DocumentNode;

            return ParseHTMLNode(TocRoot);

        }

        public virtual XNode ParseHTMLNode(HtmlAgilityPack.HtmlNode el)
        {
            if (el == null)
                return null;
            XNode docNode = new XNode(el);
            if (docNode == null)
                return null;
            if (el.HasChildNodes)
            {
                foreach (HtmlAgilityPack.HtmlNode childNode in el.ChildNodes)
                {
                    XNode rc = ParseHTMLNode(childNode);
                    if (rc != null) docNode.ChildNodes.Add(rc);
                }
            }
            return docNode;
        }      
    }
}
