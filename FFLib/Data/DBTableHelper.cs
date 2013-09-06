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
using System.Reflection;

namespace FFLib.Data
{
    public class DBTableHelper
    {
        public class TableDef
        {
            public TableDef(string tableName, string primaryKey, MemberInfo pk_memberInfo)
            {
                _tableName = tableName;
                _pk = primaryKey;
                _pk_memberinfo = pk_memberInfo;
            }

            string _tableName;
            string _pk;
            MemberInfo _pk_memberinfo;

            public string TableName { get { return _tableName; } }
            public string PK { get { return _pk; } }
            public MemberInfo PK_MemberInfo { get { return _pk_memberinfo; } }

            
        }

        public static TableDef GetTableDef<T>(string tableName){
            string _tableName = null;
            string _pk = null;
            MemberInfo _pk_memberinfo = null;
            bool attr_found = false;

            MemberInfo[] miList = typeof(T).GetMember("*", MemberTypes.Field | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Public);
            foreach (MemberInfo mi in miList)
            {
                foreach (Attribute attr in mi.GetCustomAttributes(typeof(Attributes.PrimaryKeyAttribute), false))
                {
                    if (attr is Attributes.PrimaryKeyAttribute) 
                    {
                        _pk_memberinfo = mi; _pk = mi.Name; attr_found = true; 
                        foreach (Attribute attr2 in mi.GetCustomAttributes(typeof(FFLib.Attributes.MapsToAttribute), false))
                            if (attr2 is FFLib.Attributes.MapsToAttribute) { _pk = ((FFLib.Attributes.MapsToAttribute)attr2).PropertyName; break; }
                        break; 
                    }
                }
                if (attr_found) break;
            }
            attr_found = false;

            if (!string.IsNullOrEmpty(tableName)) { _tableName = tableName;}
            else
                foreach (Attribute attr in typeof(T).GetCustomAttributes(typeof(Attributes.DBTableNameAttribute), false))
                {
                    if (attr is Attributes.DBTableNameAttribute) { _tableName = ((Attributes.DBTableNameAttribute)attr).TableName; attr_found = true; break; }
                }
            return new TableDef(_tableName, _pk, _pk_memberinfo);
        }
    }
}
