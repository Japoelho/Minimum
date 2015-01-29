using System;
using System.Configuration;
using Minimum.Connection.Interfaces;

namespace Minimum.Connection
{
    public sealed class ConnectionFactory
    {
        #region [ Messages ]
        private const string ConnectionSettingsNotFound = "The requested connection [{0}] wasn't found in the configuration file.";
        #endregion

        #region [ Public ]
        public static IConnection GetConnection(IConnectionInfo connectionInfo)
        {
            if (String.IsNullOrEmpty(connectionInfo.ConnectionString)) 
            {
                ConnectionStringSettings connectionSettings = ConfigurationManager.ConnectionStrings[connectionInfo.ConnectionName];
                if (connectionSettings == null) { throw new ArgumentException(String.Format(ConnectionSettingsNotFound, connectionInfo.ConnectionName)); }
                connectionInfo.ConnectionString = connectionSettings.ConnectionString;
            }

            switch (connectionInfo.Type)
            {
                case ConnectionType.Access:
                case ConnectionType.DB2:
                case ConnectionType.MySQL:
                case ConnectionType.Oracle:
                    { throw new NotImplementedException(); }
                case ConnectionType.SQL:
                    { return new SQLConnection(connectionInfo.ConnectionString); }
                default:
                    { throw new InvalidOperationException(); }
            }
        }
        #endregion
    }
    
    public enum ConnectionType
    { 
        Access, 
        DB2, 
        MySQL, 
        SQL, 
        Oracle 
    }
}