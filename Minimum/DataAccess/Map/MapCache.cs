using System;
using System.Collections.Generic;
using System.Linq;

namespace Minimum.DataAccess
{
    internal class MapCache
    {
        private static MapCache _instance;
        private IList<IMap> _maps;

        private MapCache()
        { _maps = new List<IMap>(); }

        public static MapCache GetInstance()
        {
            if (_instance == null)
            { _instance = new MapCache(); }

            return _instance;
        }

        public IMap Load(Type type)
        {
            return _maps.FirstOrDefault(m => m.Type == type);
        }

        public void Save(IMap map)
        {
            _maps.Add(map);
        }
    }
}
