/*******************************************************
 * Project: FFLib V1.0
 * Title: DBTable.cs
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
using System.Reflection;
using System.Data;
using Sql = System.Data.SqlClient;
using System.Text.RegularExpressions;
using FFLib.Data.Attributes;
using FFLib.Data.DBProviders;
using FFLib.Extensions;
using System.Diagnostics;

namespace FFLib.Data
{
    public interface IDBTable<T> where T : class, new()
    {
        T Load(int ID);
        T[] Load(string sql,Sql.SqlParameter[] SqlParams);
        T[] Load(string SqlText, SqlMacro[] SqlMacros);
        T[] Load(string SqlText, SqlMacro[] SqlMacros, Sql.SqlParameter[] SqlParams);
        T LoadOne(string SqlText);
        T LoadOne(string SqlText, System.Data.SqlClient.SqlParameter[] paramList);
        Dictionary<string, T> LoadAssoc(string sql, Sql.SqlParameter[] SqlParams, string key);
        Dictionary<string, T> LoadAssoc(string sql, SqlMacro[] SqlMacros, Sql.SqlParameter[] SqlParams, string keyfield);
        Dictionary<string, List<T>> LoadAssocList(string sql, Sql.SqlParameter[] SqlParams, string keyfield);
        int Execute(string SqlText, Sql.SqlParameter[] SqlParams);
        int Execute(string SqlText, SqlMacro[] SQLMacros);
        int Execute(string SqlText, SqlMacro[] SQLMacros, Sql.SqlParameter[] SqlParams);
        object ExecuteScalar(string SqlText);
        object ExecuteScalar(string SqlText, SqlMacro[] SQLMacros);
        object ExecuteScalar(string SqlText, SqlMacro[] SQLMacros, Sql.SqlParameter[] SqlParams);
        string ParseSql(string SqlText);
        string ParseSql(string SqlText, SqlMacro[] SQLMacros);
        void Save(T obj);
    }

    public class DBTable<T> : IDBTable<T> where T : class, new()
    {
        protected IDBConnection _conn = null;
        protected IDBProvider _dbProvider = null;
        protected IDBContext _dbContext = null;
        protected string _pk = null;
        protected MemberInfo _pk_memberinfo = null;
        protected string _tableName = null;

        //static Regex subTableNameRegex = new Regex("#__TableName", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        //static Regex subPKRegex = new Regex("#__PK", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public DBTable(IDBConnection Conn)
            : this(Conn, null)
        { }

        public DBTable(IDBConnection Conn, string tableName)
            : base()
        {
            _conn = Conn;
            DBTableHelper.TableDef tableDef = DBTableHelper.GetTableDef<T>(tableName);
            _pk = tableDef.PK;
            _pk_memberinfo = tableDef.PK_MemberInfo;
            _tableName = tableDef.TableName;
            _dbProvider = _conn.dbProvider;
        }

        public DBTable(IDBContext dbContext, IDBConnection Conn)
            : this(Conn, null){}

        public DBTable(IDBContext dbContext, IDBConnection Conn, string tableName)
            : this(Conn, tableName)
        {
            _dbContext = dbContext;
        }

        public IDBConnection DBConnection { get { return _conn; } }

        public virtual T Load(int ID)
        {
            string sqlText = this.ParseSql("SELECT * FROM #__TableName WHERE #__PK = @id");
            Sql.SqlParameter id = new Sql.SqlParameter("@id", ID);
            T[] rows = this.Load(sqlText, new Sql.SqlParameter[] { id });
            if (rows == null || rows.Length == 0) return new T();
            return rows[0];
        }

        public virtual T LoadOne(string SqlText)
        {
            return this.LoadOne(SqlText, null);
        }

        public virtual T LoadOne(string SqlText, System.Data.SqlClient.SqlParameter[] paramList)
        {
            T[] results;
            results = this.Load(SqlText, null, paramList);
            if (results == null || results.Length == 0) return null;
            return results[0];
        }

        public virtual T[] Load(string SqlText)
        {
            return this.Load(SqlText, null, null);
        }

        public virtual T[] Load(string SqlText, Sql.SqlParameter[] SqlParams)
        {
            return this.Load(SqlText, null, SqlParams);
        }

        public virtual T[] Load(string SqlText, SqlMacro[] SqlMacros) {
            return this.Load(SqlText, SqlMacros, null);
        }

        public virtual T[] Load(string SqlText, SqlMacro[] SqlMacros, Sql.SqlParameter[] SqlParams) {
            System.Data.IDataReader reader = null;
           // Sql.SqlCommand sqlCmd = new Sql.SqlCommand(this.ParseSql(SqlText,SqlMacros), _conn.Connection);
           // if (SqlParams != null) sqlCmd.Parameters.AddRange(SqlParams);
            try{
                _conn.Open();
                //long t = DateTime.Now.Ticks;
                //Debug.WriteLine("DB Load Start:" + (DateTime.Now.Ticks - t));
                reader = _dbProvider.ExecuteReader(_conn, this.ParseSql(SqlText, SqlMacros), SqlParams );
                //Debug.WriteLine("DB Reader Done:" + (DateTime.Now.Ticks - t));
                T[] r = this.Bind(reader);
                //Debug.WriteLine("Bind done:" + (DateTime.Now.Ticks - t));
                return r;
            }
            finally {
                if (reader != null && !reader.IsClosed ) {reader.Close(); reader.Dispose();}
                if (_conn != null && !_conn.InTrx && _conn.State != ConnectionState.Closed) _conn.Close();
            }
        }

        public virtual Dictionary<string, T> LoadAssoc(string sql, Sql.SqlParameter[] SqlParams, string keyfield)
        {
            return LoadAssoc(sql, null, SqlParams, keyfield);
        }

        public virtual Dictionary<string, T> LoadAssoc(string sql, SqlMacro[] SqlMacros, Sql.SqlParameter[] SqlParams, string keyfield)
        {
            Dictionary<string, T> results = new Dictionary<string, T>();
            if (string.IsNullOrEmpty(keyfield) || keyfield.Trim() == string.Empty) return results;
            if (string.IsNullOrEmpty(sql) || sql.Trim() == string.Empty) return results;

            T[] rows = this.Load(sql, SqlMacros, SqlParams);

            foreach (T row in rows)
            {
                MemberInfo[] mi = row.GetType().GetMember(keyfield, MemberTypes.Field | MemberTypes.Property,BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (mi == null || mi.Length == 0) continue;
                switch (mi[0].MemberType ){
                    case MemberTypes.Property:
                            PropertyInfo pi = mi[0] as PropertyInfo;
                            this.AddRowAssoc(results, pi, row);
                        break;
                    case MemberTypes.Field:
                            FieldInfo fi = mi[0] as FieldInfo;
                            this.AddRowAssoc(results, fi, row);
                        break;
                    default: continue;
                }
            }
            return results;
        }

        public virtual Dictionary<string, List<T>> LoadAssocList(string sql, Sql.SqlParameter[] SqlParams, string keyfield)
        {
            return LoadAssocList(sql, null, SqlParams, keyfield);
        }

        public virtual Dictionary<string, List<T>> LoadAssocList(string sql, SqlMacro[] SqlMacros, Sql.SqlParameter[] SqlParams, string keyfield)
        {
            Dictionary<string, List<T>> results = new Dictionary<string, List<T>>();
            if (string.IsNullOrEmpty(keyfield) || keyfield.Trim() == string.Empty) return results;
            if (string.IsNullOrEmpty(sql) || sql.Trim() == string.Empty) return results;

            T[] rows = this.Load(sql, SqlMacros, SqlParams);

            foreach (T row in rows)
            {
                MemberInfo[] mi = row.GetType().GetMember(keyfield, MemberTypes.Field | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (mi == null || mi.Length == 0) continue;
                switch (mi[0].MemberType)
                {
                    case MemberTypes.Property:
                        PropertyInfo pi = mi[0] as PropertyInfo;
                        this.AddRowAssocList(results, pi, row);
                        break;
                    case MemberTypes.Field:
                        FieldInfo fi = mi[0] as FieldInfo;
                        this.AddRowAssocList(results, fi, row);
                        break;
                    default: continue;
                }
            }
            return results;
        }

        public virtual R[] LoadScalarArray<R>(int index, string SqlText, SqlMacro[] SqlMacros, Sql.SqlParameter[] SqlParams) where R:struct
        {
            System.Data.IDataReader reader = null;
            List<R> r = new List<R>();
            // Sql.SqlCommand sqlCmd = new Sql.SqlCommand(this.ParseSql(SqlText,SqlMacros), _conn.Connection);
            // if (SqlParams != null) sqlCmd.Parameters.AddRange(SqlParams);
            try
            {
                _conn.Open();
                //long t = DateTime.Now.Ticks;
                //Debug.WriteLine("DB Load Start:" + (DateTime.Now.Ticks - t));
                reader = _dbProvider.ExecuteReader(_conn, this.ParseSql(SqlText, SqlMacros), SqlParams);
                //Debug.WriteLine("DB Reader Done:" + (DateTime.Now.Ticks - t));
                //T[] r = this.Bind(reader);
                while (reader.Read())
                {
                    object t = reader.GetValue(index);
                    if (t == DBNull.Value && typeof(R).IsValueType)
                    {
                        R q = new R(); //create a min value struct of type R
                        r.Add(q);
                    }
                    else
                    {
                        r.Add((R)t);
                    }
                }
                //Debug.WriteLine("Bind done:" + (DateTime.Now.Ticks - t));
                return r.ToArray();
            }
            finally
            {
                if (reader != null && !reader.IsClosed) { reader.Close(); reader.Dispose(); }
                if (_conn != null && !_conn.InTrx && _conn.State != ConnectionState.Closed) _conn.Close();
            }
        }

        protected virtual void AddRowAssoc(Dictionary<string,T> AssocList, PropertyInfo pi, T row){
            object key = pi.GetValue(row,null);
            if (AssocList.ContainsKey(key.ToString())) return;

            AssocList.Add(key.ToString(), row);
        }

        protected virtual void AddRowAssoc(Dictionary<string, T> AssocList, FieldInfo fi, T row)
        {
            object key = fi.GetValue(row);
            if (AssocList.ContainsKey(key.ToString())) return;

            AssocList.Add(key.ToString(), row);
        }

        protected virtual void AddRowAssocList(Dictionary<string, List<T>> AssocList, PropertyInfo pi, T row)
        {
            object key = pi.GetValue(row, null);
            if (AssocList.ContainsKey(key.ToString())) AssocList[key.ToString()].Add(row);
            else
                AssocList.Add(key.ToString(),new List<T>(new T[]{row}));
        }

        protected virtual void AddRowAssocList(Dictionary<string, List<T>> AssocList, FieldInfo fi, T row)
        {
            object key = fi.GetValue(row);
            if (AssocList.ContainsKey(key.ToString())) AssocList[key.ToString()].Add(row);
            else
                AssocList.Add(key.ToString(), new List<T>(new T[] { row }));
        }

        public int Execute(string SqlText)
        {
            return this.Execute(SqlText, null, null);
        }

        public int Execute(string SqlText, Sql.SqlParameter[] SqlParams)
        {
            return this.Execute(SqlText, null, SqlParams);
        }

        public int Execute(string SqlText, SqlMacro[] SQLMacros)
        {
            return this.Execute(SqlText, SQLMacros, null);
        }
        /// <summary>
        /// Execute SQL Batch returning the number of rows affected
        /// </summary>
        /// <param name="SqlText">SQL statements to be executed</param>
        /// <param name="SQLMacros">Array of SQLMacros. SQLMacros will be substituted in to the SQL String before execution</param>
        /// <param name="SqlParams">Array of SQL Parameters</param>
        /// <returns>int : number of rows affected. this may be altered by sql statements such as nocount</returns>
        public int Execute(string SqlText, SqlMacro[] SQLMacros, Sql.SqlParameter[] SqlParams)
        {
            //Sql.SqlCommand sqlCmd = _conn.CreateCommand(this.ParseSql(SqlText, SQLMacros));
            //if (SqlParams != null) sqlCmd.Parameters.AddRange(SqlParams);
            try
            {
                _conn.Open();
                return _dbProvider.ExecuteNonQuery(_conn, this.ParseSql(SqlText, SQLMacros), SqlParams);
            }
            finally
            {
                if (_conn != null && !_conn.InTrx && _conn.State != ConnectionState.Closed) _conn.Close();
            }
        }

        public object ExecuteScalar(string SqlText)
        {
            return this.ExecuteScalar(SqlText, null, null);
        }

        public object ExecuteScalar(string SqlText, Sql.SqlParameter[] SqlParams)
        {
            return this.ExecuteScalar(SqlText, null, SqlParams);
        }

        public object ExecuteScalar(string SqlText, SqlMacro[] SQLMacros)
        {
            return this.ExecuteScalar(SqlText, SQLMacros, null);
        }
        /// <summary>
        /// Execute SQL Batch returning the first value of the first row of the resultset
        /// </summary>
        /// <param name="SqlText">SQL statements to be executed</param>
        /// <param name="SQLMacros">Array of SQLMacros. SQLMacros will be substituted in to the SQL String before execution</param>
        /// <param name="SqlParams">Array of SQL Parameters</param>
        /// <returns>object : the first value of the first row of the resultset</returns>
        public object ExecuteScalar(string SqlText, SqlMacro[] SQLMacros, Sql.SqlParameter[] SqlParams)
        {
            //Sql.SqlCommand sqlCmd = _conn.CreateCommand(this.ParseSql(SqlText, SQLMacros));
            //if (SqlParams != null) sqlCmd.Parameters.AddRange(SqlParams);
            try
            {
                _conn.Open();
                return _dbProvider.ExecuteScalar<object>(_conn, this.ParseSql(SqlText, SQLMacros), SqlParams);
            }
            finally
            {
                if (_conn != null && !_conn.InTrx && _conn.State != ConnectionState.Closed) _conn.Close();
            }
        }

        public U ExecuteScalar<U>(string SqlText, SqlMacro[] SQLMacros, Sql.SqlParameter[] SqlParams)
        {
            return (U) ExecuteScalar(SqlText, SQLMacros, SqlParams);
        }

        protected virtual T[] Bind(IDataReader reader)
        {
            List<T> rows = new List<T>();
            try
            {
            if (reader == null || reader.IsClosed) return rows.ToArray();

            MemberInfo[] miList = typeof(T).GetMember("*", MemberTypes.Field | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Public);
            Dictionary<string,int> columnIdx = new Dictionary<string,int>();
            
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    columnIdx.Add(reader.GetName(i), i);
                }
                while (reader.Read())
                {
                    T dto = new T();
                    foreach (MemberInfo m in miList)
                    {
                        if (!columnIdx.ContainsKey(m.Name)) continue;
                        switch (m.MemberType)
                        {
                            case MemberTypes.Field:
                                FieldInfo fi = m as FieldInfo;
                                DBTable<T>._SetValue(dto, m, reader.GetValue(columnIdx[m.Name]) == DBNull.Value ? null : reader.GetValue(columnIdx[m.Name]));
                                break;
                            case MemberTypes.Property:
                                PropertyInfo pi = m as PropertyInfo;
                                if (pi.CanWrite) DBTable<T>._SetValue(dto, m, reader.GetValue(columnIdx[m.Name]) == DBNull.Value ? null : reader.GetValue(columnIdx[m.Name]));
                                break;
                        }
                    }
                    rows.Add(dto);
                    if (_dbContext != null) _dbContext.UpdateTableCache(_tableName, this.GetPKValue(dto).ToString(),dto);
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
            return rows.ToArray();
        }

        static private void _SetValue(Object obj, MemberInfo m, Object value)
        {
            string propTypeName = null;
            switch (m.MemberType)
            {
                case MemberTypes.Property:
                    {
                        PropertyInfo prop = m as PropertyInfo;
                        propTypeName = prop.PropertyType.Name;
                        if (prop.PropertyType.IsGenericType) { propTypeName = Nullable.GetUnderlyingType(prop.PropertyType).Name; }
                        if (prop.PropertyType.IsClass && value == null) { prop.SetValue(obj, null, null); return; }
                        prop.SetValue(obj, DBTable<T>.ConvertFieldValue(propTypeName,value), null);
                    }
                    break;
                case MemberTypes.Field:
                    {
                        FieldInfo prop = m as FieldInfo;
                        propTypeName = prop.FieldType.Name;
                        if (prop.FieldType.IsGenericType) { propTypeName = Nullable.GetUnderlyingType(prop.FieldType).Name; }
                        if (prop.FieldType.IsClass && value == null) { prop.SetValue(obj, null); return; }
                        prop.SetValue(obj, DBTable<T>.ConvertFieldValue(propTypeName,value));
                    }
                    break;
            }
            


        }

        protected static object ConvertFieldValue(string fieldtype, object value)
        {
            object result = null;
            switch (fieldtype.ToLower())
            {
                case "string": result = Convert.ToString(value).TrimEnd(); break;
                case "char": result = Convert.ToChar(value); break;
                case "byte": result = Convert.ToByte(value); break;
                case "short": result = Convert.ToByte(value); break;
                case "int": result = Convert.ToInt32(value); break;
                case "int16": result = Convert.ToInt16(value); break;
                case "int32": result = Convert.ToInt32(value); break;
                case "int64": result = Convert.ToInt64(value); break;
                case "uint16": result = Convert.ToUInt16(value); break;
                case "uint32": result = Convert.ToUInt32(value); break;
                case "uint64": result = Convert.ToUInt64(value); break;
                case "long": result = Convert.ToInt64(value); break;
                case "decimal": result = Convert.ToDecimal(value); break;
                case "single": result = Convert.ToSingle(value); break;
                case "double": result = Convert.ToDouble(value); break;
                case "boolean": result = Convert.ToBoolean(value); break;
                case "bool": result = Convert.ToBoolean(value); break;
                case "datetime": result = Convert.ToDateTime(value); break;
                case "guid": result = new Guid(Convert.ToString(value).TrimEnd()); break;
                case "date": result = Convert.ToDateTime(value); break;
                default: result = value; break;
            }
            return result;
        }

        protected static object ConvertValue(object value, MemberInfo target)
        {
            object result = null;
            string targetType ="";
            switch ( target.MemberType)
            {
                case MemberTypes.Property:
                    {
                        PropertyInfo prop = target as PropertyInfo;
                        targetType = prop.PropertyType.Name;
                        break;
                    }

                case MemberTypes.Field:
                    {
                        FieldInfo prop = target as FieldInfo;
                        targetType = prop.FieldType.Name;
                        break;
                    }

            }
            switch (targetType.ToLower())
            {
                case "string": result = Convert.ToString(value).TrimEnd(); break;
                case "char": result = Convert.ToChar(value); break;
                case "byte": result = Convert.ToByte(value); break;
                case "short": result = Convert.ToByte(value); break;
                case "int": result = Convert.ToInt32(value); break;
                case "int16": result = Convert.ToInt16(value); break;
                case "int32": result = Convert.ToInt32(value); break;
                case "int64": result = Convert.ToInt64(value); break;
                case "uint16": result = Convert.ToUInt16(value); break;
                case "uint32": result = Convert.ToUInt32(value); break;
                case "uint64": result = Convert.ToUInt64(value); break;
                case "long": result = Convert.ToInt64(value); break;
                case "decimal": result = Convert.ToDecimal(value); break;
                case "single": result = Convert.ToSingle(value); break;
                case "double": result = Convert.ToDouble(value); break;
                case "boolean": result = Convert.ToBoolean(value); break;
                case "bool": result = Convert.ToBoolean(value); break;
                case "datetime": result = Convert.ToDateTime(value); break;
                case "guid": result = new Guid(Convert.ToString(value).TrimEnd()); break;
                case "date": result = Convert.ToDateTime(value); break;
                default: result = value; break;
            }
            return result;
        }

        public string ParseSql(string SqlText)
        {
            return this.ParseSql(SqlText, null);
        }
        /// <summary>
        /// Parse SQL and substitute SQLMacros. the Macros #__TableName and #__PK will be automaticly substituted if defined for the Table and do not need to be explicity included in the Macros array.
        /// </summary>
        /// <param name="SqlText">SQL statements</param>
        /// <param name="SQLMacros">Array of Macros to substitute</param>
        /// <returns></returns>
        public string ParseSql(string SqlText, SqlMacro[] SQLMacros)
        {
            List<SqlMacro> macros = new List<SqlMacro>();
            if (this._tableName != null)
                macros.Add(new SqlMacro<string>("#__") { MacroType = SqlMacro.MacroTypes.Identifier, Name = "TableName", Value = this._tableName });
                //SqlText = subTableNameRegex.Replace(SqlText, this._tableName);
            if (this._pk != null)
                macros.Add(new SqlMacro<string>("#__") { MacroType = SqlMacro.MacroTypes.Identifier, Name = "PK", Value = this._pk });
                //SqlText = subPKRegex.Replace(SqlText, this._pk);
            if (SQLMacros != null && SQLMacros.Length > 0) macros.AddRange(SQLMacros);
            if (macros != null && macros.Count > 0)
                foreach (SqlMacro m in macros)
                    SqlText = SqlText.Replace(m.Token, m.ToString());
            return SqlText;
        }

        public virtual void Save(T obj){
            if (obj == null) return;

            bool isNew;
            bool isDirty = true; //defaults to true.
            if (obj is ISupportsIsDirty) isDirty = ((ISupportsIsDirty)obj).IsDirty();
            if (!isDirty) return;
      
            if (_pk_memberinfo == null) return;

            isNew = this.IsNew(obj);

            string sqlPrefix = isNew ? "INSERT INTO #__TableName \n" : "UPDATE #__TableName SET \n";
            

            List<string> fields = new List<string>();
            List<string> values = new List<string>();
            List<Sql.SqlParameter> sqlParams = new List<Sql.SqlParameter>();
            Int32 pidx = 0;

            MemberInfo[] members = obj.GetType().GetMembers( BindingFlags.Instance | BindingFlags.Public);
            if (members == null || members.Length == 0) return;
            foreach (MemberInfo m in members) {
                switch (m.MemberType ){
                    case MemberTypes.Property:{
                        PropertyInfo pi = m as PropertyInfo;
                        object[] attrs = pi.GetCustomAttributes(typeof(NotPersistedAttribute),false);
                        if (attrs != null && attrs.Length > 0) continue;
                        pidx++;
                        if (isNew){ 
                            fields.Add("["+pi.Name+"]");
                            values.Add("@p" + pidx.ToString()); 
                        } else {
                            fields.Add("["+pi.Name + "] = @p" + pidx.ToString());
                        }

                        if (pi.PropertyType.Equals(typeof(DateTime))
                            || (pi.PropertyType.IsGenericType && pi.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>))
                            && Nullable.GetUnderlyingType(pi.PropertyType).Equals(typeof(DateTime))))
                        {

                            if (pi.GetValue(obj,null)!= null && (DateTime)pi.GetValue(obj,null) == DateTime.MinValue) pi.SetValue(obj, null,null);
                        }

                        sqlParams.Add(new Sql.SqlParameter("@p" + pidx.ToString(), pi.GetValue(obj, null) == null ? DBNull.Value : pi.GetValue(obj, null)));
                    break;}
                    case MemberTypes.Field:{
                        FieldInfo fi = m as FieldInfo;
                        object[] attrs = fi.GetCustomAttributes(typeof(NotPersistedAttribute),false);
                        if (attrs != null && attrs.Length > 0) continue;
                        pidx++;
                        if (isNew){ 
                            fields.Add("["+fi.Name+"]");
                            values.Add("@p" + pidx.ToString());
                        } else {
                            fields.Add("["+fi.Name + "] = @p" + pidx.ToString());
                        }
                        
                        if (fi.FieldType.Equals(typeof(DateTime)) 
                            || (fi.FieldType.IsGenericType && fi.FieldType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)) 
                            && Nullable.GetUnderlyingType(fi.FieldType).Equals(typeof(DateTime)))){
                            if ((DateTime)fi.GetValue(obj) == DateTime.MinValue) fi.SetValue(obj,null);
                        }

                        sqlParams.Add(new Sql.SqlParameter("@p" + pidx.ToString(),fi.GetValue(obj)));
                        break;}
                    default: continue;
                }
            }

            if (fields.Count == 0) return;
            string sqlfields = isNew ? "(" + string.Join(",",fields.ToArray()) + ")" : string.Join(",",fields.ToArray());
            string sqlvalues = isNew ? " VALUES (" + string.Join(",", values.ToArray()) + ")" : "";
            string sqlCriteria = isNew ? "" : "\n WHERE #__PK = @pk";
            string sqlText = this.ParseSql(sqlPrefix + sqlfields + sqlvalues + sqlCriteria + (isNew ? "; SELECT SCOPE_IDENTITY();" : ""));

            sqlParams.Add(new Sql.SqlParameter("@pk", (int)GetPKValue(obj)));

            try
            {
                if (isNew)
                {
                    var result = _dbProvider.DBInsert<decimal>(_conn, sqlText, sqlParams.ToArray());
                    SetPKValue(obj, result);
                }
                else
                {
                    _dbProvider.DBUpdate(_conn, sqlText, sqlParams.ToArray());
                }
                //refresh from DB
                obj = this.Load((int)this.GetPKValue(obj));
            }
            finally
            {
                if (_conn != null && !_conn.InTrx && _conn.State != ConnectionState.Closed) _conn.Close();
            }

        }

        

        public bool IsNew(T obj){
            bool isNew = false;
            if (obj is ISupportsIsNew) isNew = ((ISupportsIsNew)obj).IsNew();
            switch (_pk_memberinfo.MemberType)
                {
                    case MemberTypes.Property:
                        PropertyInfo pi = _pk_memberinfo as PropertyInfo;
                        if (pi.PropertyType.IsClass) isNew = pi.GetValue(obj,null) == null ? true : false;
                        else isNew = pi.GetValue(obj,null).Equals((Activator.CreateInstance(pi.PropertyType))) ? true : false;
                        break;
                    case MemberTypes.Field:
                        FieldInfo fi = _pk_memberinfo as FieldInfo;
                        if (fi.FieldType.IsClass) isNew = fi.GetValue(obj) == null ? true : false;
                        else isNew = fi.GetValue(obj).Equals(Activator.CreateInstance(fi.FieldType)) ? true : false;
                        break;
                }
            return isNew;
        }

        public object GetPKValue(T obj){
            if (this._pk_memberinfo == null) throw new InvalidOperationException("Primary Key has not been defined");
            switch (_pk_memberinfo.MemberType)
            {
                case MemberTypes.Property:
                    {
                        PropertyInfo prop = _pk_memberinfo as PropertyInfo;
                        return prop.GetValue(obj, null);
                    }

                case MemberTypes.Field:
                    {
                        FieldInfo prop = _pk_memberinfo as FieldInfo;
                        return prop.GetValue(obj);
                    }

            }
            return null;
        }

        public object SetPKValue(T obj, object val)
        {
            if (this._pk_memberinfo == null) throw new InvalidOperationException("Primary Key has not been defined");
            switch (_pk_memberinfo.MemberType)
            {
                case MemberTypes.Property:
                    {
                        PropertyInfo prop = _pk_memberinfo as PropertyInfo;
                        prop.SetValue(obj,DBTable<T>.ConvertValue(val,_pk_memberinfo),null);
                        break;
                    }

                case MemberTypes.Field:
                    {
                        FieldInfo prop = _pk_memberinfo as FieldInfo;
                        prop.SetValue(obj, DBTable<T>.ConvertValue(val, _pk_memberinfo));
                        break;
                    }

            }
            return null;
        }
        
    }
}
