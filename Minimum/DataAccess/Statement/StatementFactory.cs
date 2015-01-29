using System;
using Minimum.Connection;
using Minimum.Connection.Interfaces;

namespace Minimum.DataAccess.Statement
{
    internal class StatementFactory
    {
        public static IStatement GetStatement(IConnectionInfo connectionInfo)
        {
            switch (connectionInfo.Type)
            {
                case ConnectionType.Access:
                case ConnectionType.DB2:
                case ConnectionType.MySQL:
                case ConnectionType.Oracle:
                    { throw new NotImplementedException(); }
                case ConnectionType.SQL:
                    { return new SQLStatement(); }
                default:
                    { throw new InvalidOperationException(); }
            }
        }
    }
}
