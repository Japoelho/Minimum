namespace Minimum.Connection.Interfaces
{
    public interface IConnectionInfo
    {
        ConnectionType Type { get; }
        string ConnectionName { get; set; }
        string ConnectionString { get; set; }
    }
}
