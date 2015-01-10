using System;
using System.Data;
using System.Data.SqlClient;
using Minimum.Connection.Interfaces;

namespace Minimum.Connection
{
    public class SQLConnection : IConnection
    {
        private SqlConnection _sqlConnection = null;

        public SQLConnection(string connectionString)
        {
            _sqlConnection = new SqlConnection(connectionString);
        }

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

        public ICommand GetCommand()
        {
            return new SQLCommand(this);
        }
        
        #region Connection Command
        public class SQLCommand : ICommand
        {
            private SqlCommand _sqlCommand = null;

            public SQLCommand(SQLConnection sqlConnection)
            {
                _sqlCommand = sqlConnection._sqlConnection.CreateCommand();
            }

            public IDataReader ExecuteReader()
            {
                if (_sqlCommand.Connection.State != ConnectionState.Open)
                { _sqlCommand.Connection.Open(); }
                
                return _sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
            }

            public int ExecuteNonQuery()
            {
                if (_sqlCommand.Connection.State != ConnectionState.Open)
                { _sqlCommand.Connection.Open(); }

                return _sqlCommand.ExecuteNonQuery();
            }

            public object ExecuteScalar()
            {
                if (_sqlCommand.Connection.State != ConnectionState.Open)
                { _sqlCommand.Connection.Open(); }

                return _sqlCommand.ExecuteScalar();
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

            public CommandType CommandType
            {
                get
                { return _sqlCommand.CommandType; }
                set
                { _sqlCommand.CommandType = value; }
            }

            public string CommandText
            {
                get
                { return _sqlCommand.CommandText; }
                set
                { _sqlCommand.CommandText = value; }
            }
        }
        #endregion
    }
}