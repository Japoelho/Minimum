using System;

namespace Minimum.DataAccess
{
    public interface IMapper
    {
        IMap Map(Type type);
    }
}
