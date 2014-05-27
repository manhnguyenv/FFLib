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

    public interface ICSSRule
    {
        ICSSSelector Selector { get;}
        UInt32 Specificity { get; }
        ICSSDeclaration[] Declarations { get;}
    }

    public interface ICSSSelectorSet
    {
        ICSSSelector[] Selectors { get;}
    }

    public interface ICSSSelector
    {
        string RawValue { get;}
        string[] Segments { get;}
        bool IsInline { get; }
        string ToString();
    }

    public interface ICSSDeclaration
    {
        string Name { get; }
        string Value { get; }
    }

    public interface ICSSStyle
    {
        string Name { get; }
        ICSSValue Value { get; }
    }

    public interface IDOMNodeStyleSet 
    {
        ICSSValue GetValue(string name);
        ICSSAbsoluteValue GetComputedValue(string name);
    }

    public interface ICSSValue 
    { 
        string ToString(); 
        decimal ToDecimal();
        bool IsInherited { get; }
        bool IsAbsolute { get; }
    }

    public interface ICSSAbsoluteValue : ICSSValue
    {
        decimal ToPixels();
        decimal ToPoints();
    }

    public interface ICSSRGBValue : ICSSAbsoluteValue 
    {
        System.Drawing.Color RGBValue();
    }

    public interface ICSSCoreResolver
    {
        void Initialize(CSSResolver.RuleIndex ruleIndex);
        IDOMNodeStyleSet GetDOMNodeStyleSet(CSSResolver.IDOMNode domNode);
        IDOMNodeStyleSet GetInheritableDOMNodeStyleSet(CSSResolver.IDOMNode domNode);
        ICSSRule[] GetRules(CSSResolver.IDOMNode node);
        ICSSRule[] OrderBySpecificity(ICSSRule[] rules);
        Dictionary<CSSResolver.IDOMNode, IDOMNodeStyleSet> StyleCache { get; }
        IDOMNodeStyleSet ResolveNodeStyles(CSSResolver.IDOMNode node, ICSSRule[] nodeRules);
        //CSSResolver.RuleIndex IndexRules(ICSSRule[] rules);
    }

    public interface IStyleParser
    {
        void Initialize();
        IDOMNodeStyleSet GetDOMNodeStyleSet(CSSResolver.IDOMNode node);
    }

    [System.Flags()]
    public enum CSSAttributes {Inherited = 1, Auto = 2, Transparent = 4 }
    public enum CSSAbsoluteUOM {pt, px, @in, cm}

    public class CSSRule : ICSSRule
    {
        public ICSSSelector Selector { get; protected set; }
        public UInt32 Specificity { get; protected set; }
        public virtual ICSSDeclaration[] Declarations { get; protected set; }

        public CSSRule(ICSSSelector Selector, ICSSDeclaration[] Declaratons) : base() 
        {
            this.Selector = Selector;
            this.Declarations = Declaratons;
            this.Specificity = this.CalcSpecificity(Selector);
        }

        /// <summary>
        /// Calculates Selector Specificity. Calculation is designed so that the when rules are numbered sequentially in the order they are defined 
        /// the rule index can be added to the specificity such as Rule.Specificity + RuleIndex = AbsoluteSpecificity.
        /// Index values must be less than 10,000. If you have more than 10K rules or index values that exceed 10,000 Specificity anomolies may occur.
        /// Algorythm also breaks down if a rule contains more than 100 selector segments.
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public virtual UInt32 CalcSpecificity(ICSSSelector selector)
        {
            if (selector == null) return 0;
            if (string.IsNullOrWhiteSpace(selector.RawValue)) return 0;

            try
            {
                UInt32 result = 100000;
                bool newSegment = true;
                bool firstClassSegment = true;
                bool firstIDSegment = true;
                for (int i = 0; i < selector.RawValue.Length; i++)
                {
                    if (newSegment && char.IsLetter(selector.RawValue[i])) result += 10000;
                    if (selector.RawValue[i] == '.') { if (firstClassSegment) { result += 1000000; firstClassSegment = false; } else result += 10000; }
                    if (selector.RawValue[i] == ':') { if (firstClassSegment) { result += 1000000; firstClassSegment = false; } else result += 10000; }
                    if (selector.RawValue[i] == '#') { if (firstIDSegment) { result += 2000000; firstIDSegment = false; } else result += 10000; }
                    if (selector.IsInline) { result = 3999999;}
                    if (char.IsWhiteSpace(selector.RawValue[i])) newSegment = true; else newSegment = false;
                }
                return result;
            }
            catch (OverflowException) { return UInt32.MaxValue; }
        }
    }

    public class CSSSelectorSet : ICSSSelectorSet
    {
        public ICSSSelector[] Selectors { get; protected set; }

        public CSSSelectorSet(ICSSSelector[] Selectors):base()
        {
            this.Selectors = Selectors;
        }
    }

    public class CSSSelector : ICSSSelector
    {
        public string RawValue { get; protected set; }
        public string[] Segments { get; protected set; }
        public bool IsInline { get; protected set; }

        public CSSSelector(string selectorString):base()
        {
            this.RawValue = selectorString;
            this.Segments = selectorString.Split(new char[]{' ','\t'});
        }

        public CSSSelector(bool isInline)
            : base()
        {
            this.RawValue = "!";
            this.Segments = new string[] {"!"};
            this.IsInline = isInline;
        }

        public override string ToString()
        {
            return this.RawValue;
        }
    }

    public class CSSDeclaration : ICSSDeclaration
    {
        protected string _computedValue = null;
        public delegate string CalcComputedValueDelegate(CSSDeclaration declaration);
        public CalcComputedValueDelegate CalcComputedValue;
        public string Name { get; protected set; }
        public string Value { get; protected set; }
        public string ComputedValue { get { if (_computedValue == null) _computedValue = CalcComputedValue(this); return _computedValue; } }
        
        public CSSDeclaration(string Name, string Value): base()
        {
            this.Name = Name;
            this.Value = Value;
        }

        public override string ToString()
        {
            return this.Name + "=" + this.Value;
        }
    }

    /// <summary>
    /// Container of all Style information for a given IDOMNode.
    /// </summary>
    public class DOMNodeStyleSet : IDOMNodeStyleSet
    {
        

        protected Dictionary<string, ICSSValue> _styles = new Dictionary<string, ICSSValue>();
        protected Dictionary<string, ICSSValue> _computedStyles = new Dictionary<string, ICSSValue>();

        public DOMNodeStyleSet(Dictionary<string, ICSSValue> styles, Dictionary<string, ICSSValue> computedStyles)
            : base()
        {
            _styles = styles;
            _computedStyles = computedStyles;
        }

        public bool InheritableStylesOnly { get; set; }

        /// <summary>
        /// returns a boolean value indicating if the IDOMNode for which this style set applies has a declared or inherited value for a given style property by name.
        /// </summary>
        /// <param name="name">Style property name</param>
        /// <returns></returns>
        public bool HasStyle(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            return this._styles.ContainsKey(name) && this._styles.ContainsKey(name) != null && !string.IsNullOrWhiteSpace(this._styles.ContainsKey(name).ToString());
        }

        /// <summary>
        /// returns the declared or inherited value for the given Style property name or null if the style property hasa not been declared or inherited.
        /// </summary>
        /// <param name="name">Style property name</param>
        /// <returns></returns>
        public ICSSValue GetValue(string name) 
        {
            if (string.IsNullOrWhiteSpace(name)) return new CSSValue((string)null,false);
            ICSSValue v;
            this._styles.TryGetValue(name, out v);
            if (v == null) return new CSSValue((string)null, false);
            return v;
        }

        /// <summary>
        /// Returns the absolute value of the given style property name or null if the absolute value cannot be determined.
        /// </summary>
        /// <param name="name">Style property name</param>
        /// <returns></returns>
        public ICSSAbsoluteValue GetComputedValue(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return new CSSAbsoluteValue(new CSSValue("0", false));
            ICSSValue v;
            this._styles.TryGetValue(name, out v);
            if (v != null && v.IsAbsolute) return new CSSAbsoluteValue(v);

            this._computedStyles.TryGetValue(name, out v);
            if (v == null) return null;
            return new CSSAbsoluteValue(v);
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder(1000);
            foreach (string key in _styles.Keys)
                s.AppendLine(key + " : " + _styles[key]);
            return s.ToString();

        }
    }

    public class CSSValue : ICSSValue
    {
        protected string _value;

        public CSSValue() : base() {
        
        }

        public CSSValue(string value, bool inherited): this()
        {
            this.IsInherited = inherited;
            _value = value;
            if (!string.IsNullOrWhiteSpace(_value)) this.IsAbsolute = Convert.IsAbsolute(_value); else this.IsAbsolute = false;
        }

        public CSSValue(ICSSDeclaration declaration): this(declaration.Value, false) { 
            if (declaration.Value.ToString().Trim().ToLower() == "inherit") this.IsInherited = true;
        }

        public CSSValue(ICSSValue value, bool inherited): this(value.ToString(),value.IsInherited){}

        public CSSValue(ICSSDeclaration declaration, bool inherited) : this(declaration.Value, inherited) { }

        public override string ToString()
        {
            return _value;
        }
        public decimal ToDecimal()
        {
            return Convert.ValueToDecimal(this._value);
        }
        public bool IsInherited { get; protected set; }
        public bool IsAbsolute { get; protected set; }
    }

    public class CSSAbsoluteValue : CSSValue,ICSSAbsoluteValue
    {
        public CSSAbsoluteValue(ICSSValue value)
            : base(value.ToString(),value.IsInherited)
        {
            if (string.IsNullOrWhiteSpace(value.ToString()) || !this.IsAbsolute) _value = "0";
            this.IsAbsolute = true;
        }

        public decimal ToPixels()
        {
            if (string.IsNullOrWhiteSpace(this._value)) return 0;
            return Convert.ConvertTo(this._value, Convert.AbsoluteUOM.px);
        }

        public decimal ToPoints()
        {
            if (string.IsNullOrWhiteSpace(this._value)) return 0;
            return Convert.ConvertTo(this._value, Convert.AbsoluteUOM.pt);
        }
    }
    public class CssValueElements{
    
        public CssValueElements(string value, string uom, string keyword):base()
        {
            this.Value = value;
            this.UOM = uom;
            this.Keyword = keyword;
        }
        public string Value{get; protected set;}
        public string UOM{get; protected set;}
        public string Keyword{get; protected set;}
    }
}
