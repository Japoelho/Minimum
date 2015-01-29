using System.Data;

namespace Minimum.Connection.Interfaces
{
    public interface ICommand
    {
        CommandType CommandType { get; set; }
        string CommandText { get; set; }

        IDataReader ExecuteReader();
        object ExecuteScalar(); 
        int ExecuteNonQuery();        
        void AddParameterInput(string parameterName, DbType parameterType, object parameterValue);
        void AddParameterInput(string parameterName, DbType parameterType, object parameterValue, int parameterSize);
        void AddParameterOutput(string parameterName, DbType parameterType);
        void AddParameterOutput(string parameterName, DbType parameterType, int parameterSize);
        object GetParameterOutput(string parameterName);
    }
}