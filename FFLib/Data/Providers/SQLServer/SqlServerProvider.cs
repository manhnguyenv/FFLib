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
using System.Data;
using System.Data.SqlClient;

namespace FFLib.Data.DBProviders
{
    public interface ISQLDBConnection : IDBConnection
    {

    }

    public class SqlServerProvider : IDBProvider 
    {
        public IDbConnection CreateConnection(string connectionString){
            return new SqlConnection(connectionString);
        }

        public IDbCommand CreateCommand(IDbConnection Connection, string CmdText)
        {
            return (IDbCommand)new SqlCommand(CmdText,(SqlConnection)Connection);
        }

        public IDataReader ExecuteReader(IDBConnection conn, string sqlText, SqlParameter[] sqlParams)
        {
            IDbCommand sqlCmd = conn.CreateCommand();
            sqlCmd.CommandText = sqlText;
            sqlCmd.CommandType = CommandType.Text;

            if (sqlParams != null && sqlParams.Length > 0) ((SqlParameterCollection)sqlCmd.Parameters).AddRange(sqlParams);

            if (conn.State == ConnectionState.Closed) conn.Open();
            return sqlCmd.ExecuteReader();

        }

        public U DBInsert<U>(IDBConnection conn, string sqlText, SqlParameter[] sqlParams)
        {
            IDbCommand sqlCmd = conn.CreateCommand();
            sqlCmd.CommandText = sqlText;
            sqlCmd.CommandType = CommandType.Text;

            if (sqlParams != null && sqlParams.Length > 0) ((SqlParameterCollection)sqlCmd.Parameters).AddRange(sqlParams);

            if (conn.State == ConnectionState.Closed) conn.Open();
            U result = (U)sqlCmd.ExecuteScalar();

            return result;
        }

        public void DBUpdate(IDBConnection conn, string sqlText, SqlParameter[] sqlParams)
        {
            IDbCommand sqlCmd = conn.CreateCommand();
            sqlCmd.CommandText = sqlText;
            sqlCmd.CommandType = CommandType.Text;

            if (sqlParams != null && sqlParams.Length > 0) ((SqlParameterCollection)sqlCmd.Parameters).AddRange(sqlParams);

                if (conn.State == ConnectionState.Closed) conn.Open();
                object result = sqlCmd.ExecuteNonQuery();
                
        }

        public U ExecuteScalar<U>(IDBConnection conn, string sqlText, SqlParameter[] sqlParams)
        {
            IDbCommand sqlCmd = conn.CreateCommand();
            sqlCmd.CommandText = sqlText;
            sqlCmd.CommandType = CommandType.Text;

            if (sqlParams != null && sqlParams.Length > 0) ((SqlParameterCollection)sqlCmd.Parameters).AddRange(sqlParams);

            if (conn.State == ConnectionState.Closed) conn.Open();
            U result = (U)sqlCmd.ExecuteScalar();

            return result;
        }

        public int ExecuteNonQuery(IDBConnection conn, string sqlText, SqlParameter[] sqlParams)
        {
            IDbCommand sqlCmd = conn.CreateCommand();
            sqlCmd.CommandText = sqlText;
            sqlCmd.CommandType = CommandType.Text;

            if (sqlParams != null && sqlParams.Length > 0) ((SqlParameterCollection)sqlCmd.Parameters).AddRange(sqlParams);

            if (conn.State == ConnectionState.Closed) conn.Open();
            return sqlCmd.ExecuteNonQuery();

        }
    }
}
