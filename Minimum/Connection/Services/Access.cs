using System;
using System.Data;
using System.Data.OleDb;
using Minimum.Connection.Interfaces;

namespace Minimum.Connection
{
    public class AccessConnection : IConnection
    {
        #region [ Private ]
        private OleDbConnection _oleConnection = null;
        #endregion

        #region [ Constructor ]
        public AccessConnection(string connectionString)
        {
            _oleConnection = new OleDbConnection(connectionString);
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
                _oleConnection.Dispose();
            }
        }
        #endregion

        #region [ Public ]
        public ICommand GetCommand()
        {
            return new OleCommand(this);
        }
        #endregion

        #region [ Properties ]
        internal OleDbConnection Connection { get { return _oleConnection; } }
        #endregion
    }

    public class OleCommand : ICommand
    {
        #region [ Private ]
        private OleDbCommand _oleCommand = null;
        #endregion

        #region [ Constructor ]
        public OleCommand(AccessConnection accessConnection)
        {
            _oleCommand = accessConnection.Connection.CreateCommand();
        }
        #endregion

        #region [ Public ]
        public IDataReader ExecuteReader()
        {
            if (_oleCommand.Connection.State != ConnectionState.Open)
            { _oleCommand.Connection.Open(); }

            return _oleCommand.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public object ExecuteScalar()
        {
            if (_oleCommand.Connection.State != ConnectionState.Open)
            { _oleCommand.Connection.Open(); }

            return _oleCommand.ExecuteScalar();
        }

        public int ExecuteNonQuery()
        {
            if (_oleCommand.Connection.State != ConnectionState.Open)
            { _oleCommand.Connection.Open(); }

            return _oleCommand.ExecuteNonQuery();
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
        #endregion

        #region [ Properties ]
        public CommandType CommandType
        {
            get { return _oleCommand.CommandType; }
            set { _oleCommand.CommandType = value; }
        }

        public string CommandText
        {
            get { return _oleCommand.CommandText; }
            set { _oleCommand.CommandText = value; }
        }
        #endregion
    }
}