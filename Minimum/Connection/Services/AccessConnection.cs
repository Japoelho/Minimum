using System;
using System.Data;
using System.Data.OleDb;
using Minimum.Connection.Interfaces;

namespace Minimum.Connection
{
    public class AccessConnection : IConnection
    {
        private OleDbConnection _oleConnection = null;

        public AccessConnection(string connectionString)
        {
            _oleConnection = new OleDbConnection(connectionString);
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
                _oleConnection.Dispose();
            }
        }

        public ICommand GetCommand()
        {
            return new OleCommand(this);
        }

        #region Connection Command
        public class OleCommand : ICommand
        {
            private OleDbCommand _oleCommand = null;

            public OleCommand(AccessConnection accessConnection)
            {
                _oleCommand = accessConnection._oleConnection.CreateCommand();
            }

            public IDataReader ExecuteReader()
            {
                if (_oleCommand.Connection.State != ConnectionState.Open)
                { _oleCommand.Connection.Open(); }

                return _oleCommand.ExecuteReader(CommandBehavior.CloseConnection);
            }

            public int ExecuteNonQuery()
            {
                if (_oleCommand.Connection.State != ConnectionState.Open)
                { _oleCommand.Connection.Open(); }

                return _oleCommand.ExecuteNonQuery();
            }

            public object ExecuteScalar()
            {
                if (_oleCommand.Connection.State != ConnectionState.Open)
                { _oleCommand.Connection.Open(); }

                return _oleCommand.ExecuteScalar();
            }

            public void AddParameterInput(string parameterName, DbType parameterType, object parameterValue)
            {                
                OleDbParameter oleParameter = new OleDbParameter();
                oleParameter.ParameterName = parameterName;
                oleParameter.DbType = parameterType;
                oleParameter.Value = parameterValue;
                oleParameter.Direction = ParameterDirection.Input;

                _oleCommand.Parameters.Add(oleParameter);
            }

            public void AddParameterInput(string parameterName, DbType parameterType, object parameterValue, int parameterSize)
            {
                OleDbParameter oleParameter = new OleDbParameter();
                oleParameter.ParameterName = parameterName;
                oleParameter.DbType = parameterType;
                oleParameter.Value = parameterValue;
                oleParameter.Size = parameterSize;
                oleParameter.Direction = ParameterDirection.Input;

                _oleCommand.Parameters.Add(oleParameter);
            }

            public void AddParameterOutput(string parameterName, DbType parameterType)
            {
                OleDbParameter oleParameter = new OleDbParameter();
                oleParameter.ParameterName = parameterName;
                oleParameter.DbType = parameterType;
                oleParameter.Direction = ParameterDirection.Output;

                _oleCommand.Parameters.Add(oleParameter);
            }

            public void AddParameterOutput(string parameterName, DbType parameterType, int parameterSize)
            {
                OleDbParameter oleParameter = new OleDbParameter();
                oleParameter.ParameterName = parameterName;
                oleParameter.DbType = parameterType;
                oleParameter.Size = parameterSize;
                oleParameter.Direction = ParameterDirection.Output;

                _oleCommand.Parameters.Add(oleParameter);
            }

            public object GetParameterOutput(string parameterName)
            {
                return _oleCommand.Parameters[parameterName].Value;
            }

            public CommandType CommandType
            {
                get
                { return _oleCommand.CommandType; }
                set
                { _oleCommand.CommandType = value; }
            }

            public string CommandText
            {
                get
                { return _oleCommand.CommandText; }
                set
                { _oleCommand.CommandText = value; }
            }
        }
        #endregion
    }
}