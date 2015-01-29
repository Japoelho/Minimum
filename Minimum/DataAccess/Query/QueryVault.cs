using System;
using System.Linq;
using System.Collections.Generic;

namespace Minimum.DataAccess
{
    internal class QueryVault
    {
        private static QueryVault _instance;
        private IList<QueryMap> _vault;

        private QueryVault()
        { _vault = new List<QueryMap>(); }

        public static QueryVault GetInstance()
        {
            if (_instance == null)
            { _instance = new QueryVault(); }

            return _instance;
        }

        public QueryMap LoadQuery(Type type)
        {
            return _vault.FirstOrDefault(q => q.Type.Equals(type));
        }

        public void SaveQuery(QueryMap query)
        {
            _vault.Add(query);
        }
    }
}
