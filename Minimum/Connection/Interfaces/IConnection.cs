using System;

namespace Minimum.Connection.Interfaces
{
    public interface IConnection : IDisposable
    {
        ICommand GetCommand();
    }
}