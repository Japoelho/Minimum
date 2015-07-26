using Minimum.DataAccess.Statement;
using System;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;

namespace Minimum.Connection
{
    public class SQL : IConnection
    {
        private const string ConnectionSettingsNotFound = "The requested connection [{0}] wasn't found in the configuration file.";
        private SQLSyntax _syntax;

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

            string originalConnectionString = ConnectionString;
            string databaseName = null;
            bool hasDatabase = false;
            if (ConnectionString.ToUpper().IndexOf("DATABASE") > -1)
            {
                hasDatabase = true;

                int index = ConnectionString.ToUpper().IndexOf("DATABASE");
                int databaseIndex = ConnectionString.IndexOf("=", index) + 1;

                databaseName = ConnectionString.Substring(databaseIndex, ConnectionString.IndexOf(";", databaseIndex) - databaseIndex);
                ConnectionString = ConnectionString.Remove(index, ConnectionString.IndexOf(";", index) - index + 1);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    if (hasDatabase) 
                    {
                        SqlCommand command = connection.CreateCommand();
                        command.CommandText = "IF DB_ID('" + databaseName + "') IS NOT NULL SELECT 1 AS Result ELSE SELECT 0 AS Result";

                        if ((int)command.ExecuteScalar() == 0)
                        {
                            result.TestMessage = "Database not found.";
                            result.TestResult = TestResult.InvalidDatabase;
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.IndexOf("error: 26 - Error Locating Server/Instance Specified") > -1)
                {
                    result.TestResult = TestResult.UnableToConnect;
                }
                else if (ex.Message.IndexOf("Login failed for user") > -1)
                {
                    result.TestResult = TestResult.InvalidCredentials;
                }
                else
                {
                    result.TestResult = TestResult.UnknownError;
                }
                result.TestMessage = ex.Message;
            }

            ConnectionString = originalConnectionString;

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

            SqlConnection connection = new SqlConnection(ConnectionString);

            return connection;
        }

        public IStatement NewStatement()
        {
            switch (_syntax)
            {
                case SQLSyntax.SQL2008:
                    { return new SQL2008Statement(); }
                case SQLSyntax.SQL2012:
                    { return new SQL2012Statement(); }
                default:
                    { return new SQL2008Statement(); }
            }
        }

        public SQL(SQLSyntax syntax = SQLSyntax.SQL2008)
        { 
            _syntax = syntax;
        }

        public SQL(string connectionName, SQLSyntax syntax = SQLSyntax.SQL2008)
        {
            _syntax = syntax; 

            ConnectionName = connectionName;
        }
    }

    public enum SQLSyntax
    {
        SQL2008, SQL2012
    }
}