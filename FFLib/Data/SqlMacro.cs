using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFLib.Extensions;

namespace FFLib.Data
{

    public class SqlMacro
    {
        public SqlMacro() { }
        public SqlMacro(string TokenPrefix)
        {
            _tokenprefix = TokenPrefix;
        }

        public enum MacroTypes {
            Literal = 0,
            Identifier = 1,
            Keyword = 3
        }

        private string _tokenprefix = @"##_";

        public virtual MacroTypes MacroType { get; set; }

        public virtual string Name { get; set; }
        /// <summary>
        /// This is the literal value that will be searched for and substituted with the macro's value
        /// </summary>
        public virtual string Token { get { return  _tokenprefix + Name; } }

        public virtual object Value { get; set; }

        public virtual bool IsNumeric() { return false; }

        public override string ToString()
        {
            return this.Value.ToString();
        }
    }

    public class SqlMacro<T> : SqlMacro
    {
        public SqlMacro():base() { }
        public SqlMacro(string TokenPrefix) : base(TokenPrefix) { }

        public new virtual T Value { get; set; }

        public override bool IsNumeric() {
            if (Value is int || Value is long || Value is short || Value is bool) return true;
            if (Value is decimal || Value is double || Value is Single) return true;
            if (Value is uint || Value is ulong || Value is ushort ) return true;
            return false; 
        }

        public override string ToString() 
        {
            if (Value is bool) { return Convert.ToBoolean(Value) ? "1" : "0"; }

            if (this.MacroType == MacroTypes.Identifier) return "[" + string.Join("].[", Value.ToString().Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries)) + "]";
            if (this.MacroType == MacroTypes.Literal)
                if (typeof(T).IsArray)
                {
                    List<string> results = new List<string>();
                    Array items = Value as Array;
                    foreach (T i in items) { results.Add(this.IsNumeric() ? i.ToString() : i.ToString().SqlQuote()); }
                    return string.Join(",", results.ToArray());
                }
                else
                    return (this.IsNumeric()) ? Value.ToString() : Value.ToString().SqlQuote();
            
            if (this.MacroType == MacroTypes.Keyword) return Value.ToString();
            return Value.ToString();
            
        }
    }
}
