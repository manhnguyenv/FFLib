using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFLib.Data
{
    public class SqlParameter
    {

        public SqlParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; private set; }
        public object Value { get; set; }
    }

    public class SqlParameter<T> : SqlParameter
    {
        public SqlParameter(string name, T value) : base(name, value) { }
    }
}
