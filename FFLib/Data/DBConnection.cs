/*******************************************************
 * Project: FFLib V1.0
 * Title: DBConnection.cs
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

namespace FFLib.Data
{
    public class DBConnection : System.Data.IDbConnection
    {
        System.Data.SqlClient.SqlConnection  _conn;
        DBTransaction _trx;
        int _trxCnt = 0;


        public DBConnection(string ConnectionString)
        {
            CommandTimeout = 30;
            _conn = new System.Data.SqlClient.SqlConnection(ConnectionString);
        }

        internal System.Data.SqlClient.SqlConnection Connection { get { return _conn; } }

        public void Open()
        {
            if (_conn.State == System.Data.ConnectionState.Open) return;
            _conn.Open();
        }

        public void Close()
        {
            if (_conn.State == System.Data.ConnectionState.Closed) return;
            _conn.Close();
        }

        public int CommandTimeout { get; set; }

        public string ConnectionString
        {
            get { return _conn.ConnectionString; }
        }

        public System.Data.ConnectionState State
        {
            get { return _conn.State; }
        }

        public bool InTrx
        {
            get { return _trxCnt > 0; }
        }

        public DBTransaction DBTransaction
        {
            get { return _trx; }
        }
        public DBTransaction BeginTransaction()
        {
            _conn.Open();
            if (_trx == null) _trx = new DBTransaction(_conn.BeginTransaction(), this);
            _trxCnt++;
            return _trx;
        }

        System.Data.IDbTransaction System.Data.IDbConnection.BeginTransaction()
        {
            return this.BeginTransaction();
        }

        System.Data.IDbTransaction System.Data.IDbConnection.BeginTransaction(System.Data.IsolationLevel level)
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            if (_trx == null) { _trxCnt = 0; return; }
            _trx.Transaction.Commit();
            if (_trxCnt > 0) _trxCnt--;
        }

        public void Rollback()
        {
            if (_trx == null) { _trxCnt = 0; return; }
            _trx.Transaction.Rollback();
            _trxCnt = 0;
        }

        public void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        string System.Data.IDbConnection.ConnectionString
        {
            get
            {
                return _conn.ConnectionString;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int ConnectionTimeout
        {
            get { return _conn.ConnectionTimeout; }
        }

        public System.Data.IDbCommand CreateCommand()
        {
            return this.CreateCommand(null);
        }

        public System.Data.SqlClient.SqlCommand CreateCommand(string CmdText)
        {
            System.Data.SqlClient.SqlCommand sqlCmd = new System.Data.SqlClient.SqlCommand(CmdText);
            sqlCmd.Connection = _conn;
            sqlCmd.CommandTimeout = this.CommandTimeout;
            if (InTrx) sqlCmd.Transaction = (System.Data.SqlClient.SqlTransaction)_trx.Transaction;
            return sqlCmd;
        }

        public string Database
        {
            get { throw new NotImplementedException(); }
        }

        private bool isDisposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if(!isDisposed)
            {
                if (disposing)
                {
                if (_trx != null) { _trx.Dispose(); _trx = null; }
                if (_conn != null) { _conn.Dispose(); _conn = null; }
                }
            }
            // Code to dispose the unmanaged resources
            // held by the class
            _trx = null;
            _conn = null;
            isDisposed = true;
            
        }
        ~DBConnection()
        {
            Dispose (false);
        }
    }


    public class DBTransaction : System.Data.IDbTransaction
    {
        System.Data.IDbTransaction _trx;
        DBConnection _conn;

        internal DBTransaction(System.Data.IDbTransaction Transaction, DBConnection Connection)
        {
            _trx = Transaction;
            _conn = Connection;
        }

        internal System.Data.IDbTransaction Transaction { get { return _trx; } }

        public void Commit()
        {
            _conn.Commit();
        }

        public System.Data.IDbConnection Connection
        {
            get { return _conn; }
        }

        public System.Data.IsolationLevel IsolationLevel
        {
            get { throw new NotImplementedException(); }
        }

        public void Rollback()
        {
            _conn.Rollback();
        }

        private bool isDisposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if(!isDisposed)
            {
                if (disposing)
                {
                if (_trx != null) { _trx.Dispose(); _trx = null; }
                }
            }
            // Code to dispose the unmanaged resources
            // held by the class
            _trx = null;
            _conn = null;
            isDisposed = true;
            
        }
        ~DBTransaction()
        {
            Dispose (false);
        }
    }
}
