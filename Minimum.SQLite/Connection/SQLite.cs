using Minimum.DataAccess.Statement;
using System;
using System.Configuration;
using System.Data.Common;
using System.Data.SQLite;

namespace Minimum.Connection
{
    public class SQLite : IConnection
    {
        private const string ConnectionSettingsNotFound = "The requested connection [{0}] wasn't found in the configuration file.";

        public ConnectionType ConnectionType { get { return ConnectionType.SQL; } }
        public string ConnectionString { get; set; }
        public string ConnectionName { get; set; }

        public ConnectionTest TestConnection()
        {
            ConnectionTest result = new ConnectionTest();
            result.TestMessage = "Test successful";
            result.TestResult = TestResult.Successful;

            if (String.IsNullOrEmpty(ConnectionString))
            {
                ConnectionStringSettings connectionSettings = ConfigurationManager.ConnectionStrings[ConnectionName];
                if (connectionSettings == null)
                {
                    result.TestMessage = "Connection String not found in the application configuration file.";
                    result.TestResult = TestResult.InvalidConnectionString;
                    return result;
                }
                ConnectionString = connectionSettings.ConnectionString;
            }

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                result.TestMessage = ex.Message;
                result.TestResult = TestResult.UnableToConnect;
            }

            return result;
        }

        public DbConnection NewConnection()
        {
            if (String.IsNullOrEmpty(ConnectionString))
            {
                ConnectionStringSettings connectionSettings = ConfigurationManager.ConnectionStrings[ConnectionName];
                if (connectionSettings == null) { throw new ArgumentException(String.Format(ConnectionSettingsNotFound, ConnectionName)); }
                ConnectionString = connectionSettings.ConnectionString;
            }

            SQLiteConnection connection = new SQLiteConnection(ConnectionString);

            return connection;
        }

        public IStatement NewStatement()
        {
            return new SQLiteStatement();
        }

        public SQLite(string fileName)
        {
            ConnectionString = "Data Source=" + fileName + ";Version=3";
        }
    }
}
