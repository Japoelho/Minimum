using Minimum.DataAccess.Statement;
using System;
using System.Data.Common;

namespace Minimum.Connection
{
    public interface IConnection
    {
        ConnectionType ConnectionType { get; }
        string ConnectionString { get; set; }
        string ConnectionName { get; set; }

        DbConnection NewConnection();
        IStatement NewStatement();
        ConnectionTest TestConnection();
    }

    public enum ConnectionType
    {
        Access,
        DB2,
        MySQL,
        SQL,
        SQLite,
        Oracle
    }

    public class ConnectionTest
    {
        public string TestMessage { get; set; }
        public TestResult TestResult { get; set; }                
    }

    public enum TestResult
    {
        InvalidConnectionString,
        UnableToConnect,
        InvalidCredentials,
        InvalidDatabase,

        UnknownError,
        Successful
    }
}