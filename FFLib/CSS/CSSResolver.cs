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

namespace FFLib.CSS
{
    public class CSSResolver
    {
        #region classes
        public interface IDOMNode
        {
            string ElementName { get; }
            string Id { get; }
            string Class { get; }
            IDOMNode ParentNode { get; }
            string GetAttribute(string name);
        }

        public interface IDOMStyles
        {
            int[] AppliedStyles { get; set; }
            ICSSRule[] ContributedStyles { get; set; }
        }

        public class ContributedStyle
        {
            int RuleIndex;
            byte[] Segment;
        }

        public class RuleIndex:Dictionary<string,RuleIndex>{
            public List<ICSSRule > Rules = new List<ICSSRule>();
        }

        #endregion

        protected ICSSRule[] _rules;
        //protected RuleIndex _ruleIndex = new RuleIndex();
        //protected Dictionary<IDOMNode, IDOMNodeStyleSet> _DOMStyles;
        protected ICSSCoreResolver _resolver;
        

        public CSSResolver(ICSSCoreResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException("Core Resolver cannot be null");
            _resolver = resolver;
        }

        public virtual void Initialize(ICSSRule[] CssRules)
        {
            if (CssRules == null) throw new ArgumentNullException();
            CSSResolverHelper rh = new CSSResolverHelper();
            RuleIndex _rules = rh.IndexRules(CssRules);
            _resolver.Initialize(_rules);
        }

        public virtual ICSSRule[] GetDOMNodeRules(IDOMNode domNode)
        {
            ICSSRule[] nodeRules = _resolver.GetRules(domNode);
            nodeRules = _resolver.OrderBySpecificity(nodeRules);
            return nodeRules;
        }

        public virtual IDOMNodeStyleSet GetDOMNodeStyleSet(IDOMNode domNode)
        {
            if (domNode == null) return null;

            return _resolver.GetDOMNodeStyleSet(domNode);
        }
    }

    /// <summary>
    /// Resolves DOM node CSS declarations into styles conforming to CSS2.1 specifications for specificity.
    /// Styles must be evaluated from the root to the leaves.
    /// </summary>
    public class CSSCoreResolver:ICSSCoreResolver 
    {
        protected ICSSParser _parser;

        public CSSCoreResolver(ICSSParser parser)
            : base()
        {
            _parser = parser;
        }

        //public delegate IDOMNodeStyleSet ResolveNodeStylesDelegate(CSSResolver.IDOMNode domNode, ResolveNodeStylesDelegate resolverdelegate);

        string[] inheritableStyles = {"azimuth","border-colapse","caption-side","color","cursor","direction","elevation","empty-cells","font-family","font-size","font-style","font-variant","font-weight","font","letter-spacing","line-height","list-style-image" 
                                     ,"list-style-position","list-style-type","list-style","orphans","pitch-range","pitch","quotes","richness","speak-header","speak-numeral","speak","speech-rate","stress","text-align","text-indent","text-transform"
                                     ,"visibility","voice-family","volume","white-space","windows","word-spacing"};

        Dictionary<CSSResolver.IDOMNode, IDOMNodeStyleSet> styleCache = new Dictionary<CSSResolver.IDOMNode, IDOMNodeStyleSet>(10000);

        public Dictionary<CSSResolver.IDOMNode, IDOMNodeStyleSet> StyleCache { get { return this.styleCache; } }
        public CSSResolver.RuleIndex _ruleIndex;

        public virtual void Initialize(CSSResolver.RuleIndex rIndex)
        {
            if (rIndex == null) throw new ArgumentNullException("Rule Index cannot be null.");
            _ruleIndex = rIndex;
        }

        public virtual IDOMNodeStyleSet GetDOMNodeStyleSet(CSSResolver.IDOMNode domNode)
        {
            if (domNode == null) return null;
            if (styleCache.ContainsKey(domNode)) return styleCache[domNode];

            ICSSRule[] nodeRules = this.GetRules(domNode);

            return this.ResolveNodeStyles(domNode, nodeRules);
        }
        
        public Dictionary<string, string> GetInheritableStylesIndex()
        {
            Dictionary<string, string> index = new Dictionary<string,string>(inheritableStyles.Length,StringComparer.CurrentCultureIgnoreCase);
            for (int i = 0; i < inheritableStyles.Length; i++) index.Add(inheritableStyles[i], inheritableStyles[i]);

            return index;

        }

        public virtual ICSSRule[] GetRules(CSSResolver.IDOMNode node)
        {
            if (node == null) return new ICSSRule[] { };
            if (_ruleIndex == null || _ruleIndex.Count == 0) return new ICSSRule[] { };
            SortedList<string, string> sl = new SortedList<string, string>(StringComparer.CurrentCultureIgnoreCase);
            if (!string.IsNullOrWhiteSpace(node.Id)) sl.Add("#" + node.Id, null);
            //Element names are not case sensitive, convert to lowercase
            if (!string.IsNullOrWhiteSpace(node.ElementName)) sl.Add(node.ElementName.ToLower(), null);
            if (!string.IsNullOrWhiteSpace(node.Class))
                foreach (string c in node.Class.Split(new char[]{' ','\t'})) sl.Add("." + c, null);
            if (sl.Count == 0) return new ICSSRule[] { };

            string[] sortedElements = sl.Keys.ToArray();
            List<ICSSRule> canidateRules = new List<ICSSRule>();
            canidateRules.AddRange(this.GetInlineRules(node));
            canidateRules.AddRange(this.GetAttributeRules(node));
            canidateRules.AddRange(this.SearchIndex(_ruleIndex, sortedElements));

            ICSSRule[] matches = this.FilterRulesByNodeContext(node, canidateRules.ToArray());

            return matches;

            //foreach (ICSSRule rule in 
        }

        public ICSSRule[] GetInlineRules(CSSResolver.IDOMNode node){
            ICSSRule[] nullResult = new ICSSRule[] { };
            string inlineStyleText = node.GetAttribute("style");
            if (string.IsNullOrWhiteSpace(inlineStyleText)) return nullResult;
            string inlineStyles = "#inline { " + inlineStyleText + " } ";
            ICSSRule[] inlineRules = _parser.ParseCSSString(inlineStyles);
            if (inlineRules == null || inlineRules.Length == 0) return nullResult;

            Dictionary<string,ICSSDeclaration> sl = new Dictionary<string,ICSSDeclaration>(StringComparer.CurrentCultureIgnoreCase);
            foreach (ICSSDeclaration d in inlineRules[0].Declarations) if (sl.ContainsKey(d.Name)) sl[d.Name] = d; else sl.Add(d.Name, d);

            return new ICSSRule[] {new CSSRule(new CSSSelector(true), sl.Values.ToArray())};
        }

        public ICSSRule[] GetAttributeRules(CSSResolver.IDOMNode node)
        {
            ICSSRule[] nullResult = new ICSSRule[] { };
            List<string> attr_delarations = new List<string>();
            string attr_align = node.GetAttribute("align");
            string attr_valign = node.GetAttribute("valign");
            string attr_width = node.GetAttribute("width");
            string attr_height = node.GetAttribute("height");

            if (!string.IsNullOrWhiteSpace(attr_align)) attr_delarations.Add("text-align:" + attr_align + ";");
            if (!string.IsNullOrWhiteSpace(attr_valign)) attr_delarations.Add("vertical-align:" + attr_valign + ";");
            if (!string.IsNullOrWhiteSpace(attr_width))
            {
                if (!attr_width.EndsWith("%")) attr_width += "px";
                attr_delarations.Add("width:" + attr_width + ";");
            }
            if (!string.IsNullOrWhiteSpace(attr_height))
            {
                if (!attr_height.EndsWith("%")) attr_height += "px";
                attr_delarations.Add("height:" + attr_height + ";");
            }
            if (attr_delarations.Count == 0) return new ICSSRule[] { };

            string attrStyles = node.ElementName + " { " + string.Join("\n",attr_delarations.ToArray()) + " } ";
            ICSSRule[] attrRules = _parser.ParseCSSString(attrStyles);
            if (attrRules == null || attrRules.Length == 0) return nullResult;
            //Dictionary<string, ICSSDeclaration> sl = new Dictionary<string, ICSSDeclaration>(StringComparer.CurrentCultureIgnoreCase);
            //foreach (ICSSDeclaration d in attrRules[0].Declarations) if (sl.ContainsKey(d.Name)) sl[d.Name] = d; else sl.Add(d.Name, d);

            return attrRules;
        }

        public virtual ICSSRule[] SearchIndex(CSSResolver.RuleIndex index, string[] sortedElements)
        {
            //FuncResult<ICSSRule[]> result = new FuncResult<ICSSRule[]>();
            List<ICSSRule> ruleMatches = new List<ICSSRule>();
            //FuncResult<ICSSRule[]> subResult;

            for(int i=0; i < sortedElements.Length; i++)
            {
                if (!index.ContainsKey(sortedElements[i])) continue;
                ruleMatches.AddRange(index[sortedElements[i]].Rules);
                string[] oe = new string[sortedElements.Length -1];
                Array.Copy(sortedElements,1,oe,0,sortedElements.Length -1);
                ruleMatches.AddRange(this.SearchIndex(index[sortedElements[i]],oe));
            }
            return ruleMatches.ToArray();
        }

        public virtual ICSSRule[] FilterRulesByNodeContext(CSSResolver.IDOMNode node, ICSSRule[] rules)
        {
            if (rules == null | rules.Length == 0) return rules;
            List<ICSSRule> rulesInScope = new List<ICSSRule>();

            foreach (ICSSRule r in rules)
            {
                if (r.Selector.Segments.Length == 0) continue;
                if (r.Selector.Segments.Length == 1) { rulesInScope.Add(r); continue; }
                rulesInScope.AddRange(this.IsRuleInContext(node.ParentNode, r, r.Selector.Segments.Length -1));
            }

            return rulesInScope.ToArray();
        }

        public virtual ICSSRule[] IsRuleInContext(CSSResolver.IDOMNode node, ICSSRule rule, int segmentDepth)
        {
            if (segmentDepth == 0) return new ICSSRule[]{rule};
            if (node == null) return new ICSSRule[]{};

            Dictionary<string, string> l = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            if (!string.IsNullOrWhiteSpace(node.Id)) l.Add("#" + node.Id, null);
            //Element names are not case sensitive, convert to lowercase
            if (!string.IsNullOrWhiteSpace(node.ElementName)) l.Add(node.ElementName.ToLower(), null);
            if (node.Class != null)
                foreach (string c in node.Class.Split(' ')) l.Add("." + c, null);
            string[] segelements = CSSResolverHelper.SplitSegment(rule.Selector.Segments[segmentDepth - 1]);

            bool match = true;
            for (int i = 0; i < segelements.Length; i++)
            {
                if (!l.ContainsKey(segelements[i])) {match = false; break;}
            }

            if (match) segmentDepth--;

            return this.IsRuleInContext(node.ParentNode, rule, segmentDepth);
        }

        public virtual ICSSRule[] OrderBySpecificity(ICSSRule[] rules)
        {
            SortedList<UInt32, ICSSRule> orderedRules = new SortedList<UInt32, ICSSRule>(rules.Length * 2);
            for (int i = 0; i < rules.Length; i++) orderedRules.Add(rules[i].Specificity + (UInt32)i, rules[i]);
            return orderedRules.Values.ToArray();
        }

        public virtual IDOMNodeStyleSet ResolveNodeStyles(CSSResolver.IDOMNode node, ICSSRule[] nodeRules)
        {
            if (node == null) return null;
            nodeRules = this.OrderBySpecificity(nodeRules);
            Dictionary<string,string> _inheritableStyles = this.GetInheritableStylesIndex();
            Dictionary<string, ICSSValue> styles = new Dictionary<string, ICSSValue>(StringComparer.CurrentCultureIgnoreCase);
            Dictionary<string, ICSSValue> computedstyles = new Dictionary<string, ICSSValue>(StringComparer.CurrentCultureIgnoreCase);

            this.GetStylesFromDeclarations(nodeRules, styles, _inheritableStyles);

            this.ResolveDeclaredStyles(node, styles, computedstyles);

            this.ResolveInheritedStyles(node, styles, computedstyles, _inheritableStyles);

            DOMNodeStyleSet ds = new DOMNodeStyleSet(styles, computedstyles);

            if (styleCache.ContainsKey(node)) styleCache[node] = ds; else styleCache.Add(node, ds);

            return ds;
        }

        /// <summary>
        /// nodeRules are processed into styles and declared styles that are inheritable are removed from the inheritableStyles index
        /// </summary>
        /// <param name="nodeRules"></param>
        /// <param name="styles"></param>
        /// <param name="inheritableStylesList"></param>
        public void GetStylesFromDeclarations(ICSSRule[] nodeRules, Dictionary<string, ICSSValue> styles, Dictionary<string, string> inheritableStylesList)
        {
            for (int i = 0; i < nodeRules.Length; i++)
                for (int t = 0; t < nodeRules[i].Declarations.Length; t++)
                {
                    if (!styles.ContainsKey(nodeRules[i].Declarations[t].Name))
                        styles.Add(nodeRules[i].Declarations[t].Name, new CSSValue(nodeRules[i].Declarations[t]));
                    else
                        styles[nodeRules[i].Declarations[t].Name] = new CSSValue(nodeRules[i].Declarations[t]);

                    if (inheritableStylesList.ContainsKey(nodeRules[i].Declarations[t].Name)) inheritableStylesList.Remove(nodeRules[i].Declarations[t].Name);
                }
        }

        /// <summary>
        /// resolves styles by looking up parent values when declared as 'inherit' and eveluating computed value when not absolute.
        /// results are added to the styles and computedStyles indexed lists.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="styles"></param>
        /// <param name="computedstyles"></param>
        public void ResolveDeclaredStyles(CSSResolver.IDOMNode node, Dictionary<string, ICSSValue> styles, Dictionary<string, ICSSValue> computedstyles)
        {
            Dictionary<string, ICSSValue> resolvedStyles = new Dictionary<string, ICSSValue>(StringComparer.CurrentCultureIgnoreCase);
            foreach(var kv in styles)
            {
                ICSSValue p = null;
                ICSSValue v = kv.Value;
                if (v == null || string.IsNullOrWhiteSpace(v.ToString())) continue;
                if (v.IsInherited)
                {
                    v = this.GetParentValue(kv.Key, node.ParentNode);
                    if (v == null || string.IsNullOrWhiteSpace(v.ToString())) continue;
                    if (resolvedStyles.ContainsKey(kv.Key)) resolvedStyles[kv.Key] = v;
                    else resolvedStyles.Add(kv.Key,v);
                }
                if (!v.IsAbsolute)
                {
                    string pname = kv.Key;
                    CssValueElements e = Convert.GetCssValueElements(v.ToString());
                    if (e.UOM.ToLower() == "em") pname = "font-size";
                    if (v.IsInherited) p = this.GetParentComputedValue(kv.Key, node.ParentNode);
                    if (p == null || string.IsNullOrWhiteSpace(p.ToString()))
                        p = this.GetParentAbsoluteValue(pname, node);
                    computedstyles.Add(kv.Key, this.CalcAbsoluteValue(v, p));
                }
            }
            foreach (var kv in resolvedStyles) if (styles.ContainsKey(kv.Key)) styles[kv.Key] = kv.Value; else styles.Add(kv.Key, kv.Value);
        }

        /// <summary>
        /// resolves the inherited and computed value of styles that where not declared for the node but are always inheritable by css specification.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="styles"></param>
        /// <param name="computedstyles"></param>
        /// <param name="_inheritableStyles"></param>
        public void ResolveInheritedStyles(CSSResolver.IDOMNode node, Dictionary<string, ICSSValue> styles, Dictionary<string, ICSSValue> computedstyles, Dictionary<string, string> _inheritableStyles)
        {
            foreach (string i in _inheritableStyles.Values)
            {
                ICSSValue p = null;
                ICSSValue v = this.GetParentValue(i, node.ParentNode);
                if (v == null || string.IsNullOrWhiteSpace(v.ToString())) continue;
                //v = new CSSValue(this.GetParentValue(i, node.ParentNode).ToString(), true);
                styles.Add(i, v);
                if (!v.IsAbsolute)
                {
                    string pname = i;
                    CssValueElements e = Convert.GetCssValueElements(i);
                    if (e.UOM.ToLower() == "em") pname = "font-size";
                    p = this.GetParentComputedValue(pname, node);
                    computedstyles.Add(i, this.CalcAbsoluteValue(v, p));
                }
            }
        }

        

        public ICSSValue GetParentValue(string styleName, CSSResolver.IDOMNode parentNode)
        {
            if (string.IsNullOrWhiteSpace(styleName) || parentNode == null) return null;

            IDOMNodeStyleSet s = null;
            this.styleCache.TryGetValue(parentNode,out s);

            if (s == null) s = this.GetDOMNodeStyleSet(parentNode); //GetParentValue(styleName,parentNode.ParentNode );
            if (s == null) return this.GetParentValue(styleName, parentNode.ParentNode); 

            ICSSValue v = s.GetValue(styleName);
            if (v != null) return v; 
            return this.GetParentValue(styleName, parentNode.ParentNode);
        }

        public ICSSValue GetParentAbsoluteValue(string styleName, CSSResolver.IDOMNode parentNode)
        {
            if (string.IsNullOrWhiteSpace(styleName) || parentNode == null) return null;

            IDOMNodeStyleSet s = null;
            this.styleCache.TryGetValue(parentNode, out s);

            if (s == null) return GetParentAbsoluteValue(styleName, parentNode.ParentNode);

            ICSSValue v = s.GetValue(styleName);
            if (v != null && !string.IsNullOrWhiteSpace(v.ToString()))
            {
                if (v.ToDecimal() > 0 && v.IsAbsolute) return v;
                if (v.ToDecimal() == 0 && v.IsAbsolute)
                {
                    if (v.ToString().Trim().ToLower() != "inherit")
                    {
                        v = new CSSValue(Util.ResolveKeywordValue(styleName, v.ToString()), v.IsInherited);
                        if (v.IsAbsolute) return v;
                    }
                }
            }
            return this.GetParentAbsoluteValue(styleName, parentNode.ParentNode);
        }

        public ICSSValue GetParentComputedValue(string styleName, CSSResolver.IDOMNode parentNode)
        {
            if (string.IsNullOrWhiteSpace(styleName) || parentNode == null) return null;

            IDOMNodeStyleSet s = null;
            this.styleCache.TryGetValue(parentNode, out s);

            if (s == null) return this.GetParentComputedValue(styleName, parentNode.ParentNode);

            ICSSValue v = s.GetComputedValue(styleName);
            if (v != null && v.IsAbsolute)
            {
                if (v.ToString().Trim().ToLower() != "inherit" && v.ToDecimal() == 0)
                    v = new CSSValue(Util.ResolveKeywordValue(styleName, v.ToString()), v.IsInherited);
                if (v.IsAbsolute) return v;
            }
            return this.GetParentComputedValue(styleName, parentNode.ParentNode);
        }

        public ICSSValue CalcAbsoluteValue(ICSSValue v, ICSSValue p)
        {
            if (v == null || p == null) return new CSSValue("0", v.IsInherited);
            CssValueElements vparts = Convert.GetCssValueElements(v.ToString());
            CssValueElements pparts = Convert.GetCssValueElements(p.ToString());

            decimal absValue = Convert.CalcAbsoluteValue(v.ToString(), p.ToString());

            return new CSSValue(absValue.ToString() + pparts.UOM,v.IsInherited);
        }

        
    }

    public class CSSResolverHelper
    {
        public virtual CSSResolver.RuleIndex IndexRules(ICSSRule[] rules)
        {
            CSSResolver.RuleIndex index = new CSSResolver.RuleIndex();
            if (rules == null || rules.Length == 0) return index;
            foreach (ICSSRule rule in rules)
            {
                string lastSegment = rule.Selector.Segments[rule.Selector.Segments.Length - 1];
                string[] segelements = CSSResolverHelper.SplitSegment(lastSegment);
                SortedList<string, string> sorted = new SortedList<string, string>(StringComparer.CurrentCultureIgnoreCase);
                foreach (string s in segelements) sorted.Add(s, null);
                //segelements = sorted.Keys.ToArray();
                CSSResolver.RuleIndex idx = index;
                CSSResolver.RuleIndex rdx = index;
                foreach (string st in sorted.Keys.ToArray())
                {
                    rdx = idx;
                    if (!idx.ContainsKey(st)) { CSSResolver.RuleIndex idx2 = new CSSResolver.RuleIndex(); idx.Add(st, idx2); idx = idx2; }
                    else idx = idx[st];
                }
                idx.Rules.Add(rule);
            }
            return index;
        }

        public static string[] SplitSegment(string segment)
        {
            StringBuilder buffer = new StringBuilder(segment[0]);
            List<string> result = new List<string>();

            if (string.IsNullOrWhiteSpace(segment)) return result.ToArray();
            segment = segment.Trim();
            char c;
            for (int i = 0; i < segment.Length; i++)
            {
                c = segment[i];
                if (char.IsWhiteSpace(c) || c == '#' || c == '.' || c == ':')
                {
                    if (buffer.Length > 0)
                    {
                        result.Add(buffer.ToString());
                        buffer.Clear();
                    }
                }
                if (!char.IsWhiteSpace(c)) buffer.Append(c);
            }
            result.Add(buffer.ToString());
            //Element names are not case sensitive, convert all element names ot lowercase. Do not alter Id's or Classes
            for (int i = 0; i < result.Count; i++) if (char.IsLetter(result[i][0])) result[i] = result[i].ToLower();
            return result.ToArray();
        }

    }
}
