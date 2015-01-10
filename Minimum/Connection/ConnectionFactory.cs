using System.Configuration;
using Minimum.Connection.Interfaces;
using Minimum.DataAccess.Mapping;
using Minimum.DataAccess.Statement;

namespace Minimum.Connection
{
    public sealed class ConnectionFactory
    {
        public static IConnection GetConnection(string connectionName)
        {
            ConnectionStringSettings connectionSettings = ConfigurationManager.ConnectionStrings[connectionName];

            return new SQLConnection(connectionSettings.ConnectionString);
        }

        internal static IStatement GetStatement(Maps maps)
        {
            return new SQLStatement(maps);
        }

        #region [ Old Versions Compatibility ]
        internal static Minimum.DataAccess.V08.Statement.IStatement GetStatement(Minimum.DataAccess.V08.Mapping.Mappings maps)
        {
            return new Minimum.DataAccess.V08.Statement.SQLStatement(maps);
        }
        #endregion
    }
}