using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFLib.Data.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class PrimaryKeyAttribute : System.Attribute 
    {
        public PrimaryKeyAttribute(){}
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NotPersistedAttribute : System.Attribute
    {
        public NotPersistedAttribute() { }
    }

    [AttributeUsage(AttributeTargets.Class )]
    public class DBTableNameAttribute : System.Attribute
    {
        string _tableName;
        public DBTableNameAttribute(string TableName)
        {
            _tableName = TableName;
        }

        public string TableName { get { return _tableName; } }
    }
}
