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
            _conn = new System.Data.SqlClient.SqlConnection(ConnectionString);
        }

        internal System.Data.SqlClient.SqlConnection Connection { get { return _conn; } }

        public void Open()
        {
            _conn.Open();
        }

        public void Close()
        {
            _conn.Close();
        }

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

        public DBTransaction BeginTransaction()
        {
            if (_trx == null) _trx = new DBTransaction(_conn.BeginTransaction(), this);
            _trxCnt++;
            return _trx;
        }

        System.Data.IDbTransaction System.Data.IDbConnection.BeginTransaction()
        {
            if (_trx == null) _trx = new DBTransaction(_conn.BeginTransaction(), this);
            _trxCnt++;
            return _trx;
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
            if (_trxCnt > 0) _trxCnt--;
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
            throw new NotImplementedException();
        }

        public string Database
        {
            get { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
            _trx.Dispose();
            _trx = null;
            _conn.Dispose();
            _conn = null;
        }
        /******************/

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

        public void Dispose()
        {
            _trx = null;
            _conn = null;
        }
    }
}
