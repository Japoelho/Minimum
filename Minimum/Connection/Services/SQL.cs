using System;
using System.Data;
using System.Data.SqlClient;
using Minimum.Connection.Interfaces;

namespace Minimum.Connection
{
    public class SQLConnection : IConnection
    {
        #region [ Private ]
        private SqlConnection _sqlConnection = null;
        #endregion

        #region [ Constructor ]
        public SQLConnection(string connectionString)
        {
            _sqlConnection = new SqlConnection(connectionString);
        }
        #endregion

        #region [ IDisposable ]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sqlConnection.Dispose();
            }
        }
        #endregion

        #region [ Public ]
        public ICommand GetCommand()
        {
            return new SQLCommand(this);
        }
        #endregion

        #region [ Properties ]
        internal SqlConnection Connection { get { return _sqlConnection; } }
        #endregion
    }

    public class SQLCommand : ICommand
    {
        #region [ Private ]
        private SqlCommand _sqlCommand = null;
        #endregion

        #region [ Constructor ]
        public SQLCommand(SQLConnection sqlConnection)
        {
            _sqlCommand = sqlConnection.Connection.CreateCommand();
        }
        #endregion

        #region [ Public ]
        public IDataReader ExecuteReader()
        {
            if (_sqlCommand.Connection.State != ConnectionState.Open)
            { _sqlCommand.Connection.Open(); }

            return _sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public object ExecuteScalar()
        {
            if (_sqlCommand.Connection.State != ConnectionState.Open)
            { _sqlCommand.Connection.Open(); }

            return _sqlCommand.ExecuteScalar();
        }

        public int ExecuteNonQuery()
        {
            if (_sqlCommand.Connection.State != ConnectionState.Open)
            { _sqlCommand.Connection.Open(); }

            return _sqlCommand.ExecuteNonQuery();
        }

        public void AddParameterInput(string parameterName, DbType parameterType, object parameterValue)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = parameterName;
            sqlParameter.DbType = parameterType;
            sqlParameter.Value = parameterValue;
            sqlParameter.Direction = ParameterDirection.Input;

            _sqlCommand.Parameters.Add(sqlParameter);
        }

        public void AddParameterInput(string parameterName, DbType parameterType, object parameterValue, int parameterSize)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = parameterName;
            sqlParameter.DbType = parameterType;
            sqlParameter.Value = parameterValue;
            sqlParameter.Size = parameterSize;
            sqlParameter.Direction = ParameterDirection.Input;

            _sqlCommand.Parameters.Add(sqlParameter);
        }

        public void AddParameterOutput(string parameterName, DbType parameterType)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = parameterName;
            sqlParameter.DbType = parameterType;
            sqlParameter.Direction = ParameterDirection.Output;

            _sqlCommand.Parameters.Add(sqlParameter);
        }

        public void AddParameterOutput(string parameterName, DbType parameterType, int parameterSize)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = parameterName;
            sqlParameter.DbType = parameterType;
            sqlParameter.Size = parameterSize;
            sqlParameter.Direction = ParameterDirection.Output;

            _sqlCommand.Parameters.Add(sqlParameter);
        }

        public object GetParameterOutput(string parameterName)
        {
            return _sqlCommand.Parameters[parameterName].Value;
        }
        #endregion

        #region [ Properties ]
        public CommandType CommandType
        {
            get { return _sqlCommand.CommandType; }
            set { _sqlCommand.CommandType = value; }
        }

        public string CommandText
        {
            get { return _sqlCommand.CommandText; }
            set { _sqlCommand.CommandText = value; }
        }
        #endregion
    }
    
    public class SQLInfo : IConnectionInfo
    {        
        #region [ Constructor ]
        public SQLInfo()
        { }

        public SQLInfo(string connectionName)
        {
            ConnectionName = connectionName;
        }
        #endregion
        
        #region [ Properties ]
        public ConnectionType Type { get { return ConnectionType.SQL; } }
        public string ConnectionName { get; set; }
        public string ConnectionString { get; set; }
        #endregion
    }
}