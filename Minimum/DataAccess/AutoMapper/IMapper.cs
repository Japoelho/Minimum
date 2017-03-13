using System;

namespace Minimum.DataAccess
{
    public interface IMapper
    {
        IMap Map(Type type);
        IMap Map(Type type, params string[] properties);
        IMap DynamicMap(string table);
        IMap DynamicMap(string table, dynamic type);
    }
}
