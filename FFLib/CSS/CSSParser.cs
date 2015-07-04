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

namespace FFLib.CSS
{
    public interface ICSSParser
    {
        ICSSRule[] ParseCSSFiles(string[] CSSFiles);
        ICSSRule[] ParseCSSString(string CSSString);
    }

    public class CSSParser : ICSSParser
    {
        public CSSParser()
        {

        }

        public virtual ICSSRule[] ParseCSSFiles(string[] CSSFiles)
        {
            List<ICSSRule> cssRules = new List<ICSSRule>(10000);
            BoneSoft.CSS.CSSParser parser = new BoneSoft.CSS.CSSParser();

            foreach (string file in CSSFiles)
            {
                parser.ParseFile(file);
                BoneSoft.CSS.CSSDocument css = parser.CSSDocument;

                if (css.RuleSets.Count == 0) return new ICSSRule[] { };



                for (int i = 0; i < css.RuleSets.Count; i++)
                {
                    foreach (BoneSoft.CSS.Selector s in css.RuleSets[i].Selectors)
                    {
                        cssRules.Add(new Rule(css.RuleSets[i], s.ToString()));
                    }
                }
            }

            return cssRules.ToArray();
        }
 
        public virtual ICSSRule[] ParseCSSString(string CSSString)
        {
            List<ICSSRule> cssRules = new List<ICSSRule>(10000);
            BoneSoft.CSS.CSSParser parser = new BoneSoft.CSS.CSSParser();

            parser.ParseText(CSSString);
            BoneSoft.CSS.CSSDocument css = parser.CSSDocument;

            if (css.RuleSets.Count == 0) return new ICSSRule[] { };



            for (int i = 0; i < css.RuleSets.Count; i++)
            {
                foreach (BoneSoft.CSS.Selector s in css.RuleSets[i].Selectors)
                {
                    cssRules.Add(new Rule(css.RuleSets[i], s.ToString()));
                }
            }

            return cssRules.ToArray();
        }
    }

    public class Rule : CSSRule
    {
        BoneSoft.CSS.RuleSet _rule;
       
        public Rule(BoneSoft.CSS.RuleSet rule, string selector):base(new CSSSelector(selector),null){
            _rule = rule;
            this.Selector = new CSSSelector(selector);
        }

        public override ICSSDeclaration[] Declarations
        {
            get
            {
                
                    if (base.Declarations == null)
                    {
                        List<CSSDeclaration> d = new List<CSSDeclaration>(_rule.Declarations.Count * 2 + 10);
                        for (int i = 0; i < _rule.Declarations.Count; i++)
                        {
                            try
                            {
                            if (Util.IsShorthandProperty(_rule.Declarations[i].Name))
                                d.AddRange(Util.ExpandShortHandProperties(_rule.Declarations[i].Name, _rule.Declarations[i].Expression.ToString()));
                            else
                                d.Add(new CSSDeclaration(_rule.Declarations[i].Name, _rule.Declarations[i].Expression.ToString()));
                            }
                            catch { throw new ApplicationException("Error Parsing CSS Declaration:" + _rule.Declarations[i].Name); }
                        }
                        base.Declarations = d.ToArray();
                    }
                    return base.Declarations;
            }
            protected set
            {
                base.Declarations = value;
            }
        }

        
    }


}
