using System;
using System.Collections.Generic;

namespace Minimum.DataAccess.Mapping
{
    internal class Alias
    {
        public string Value { get; set; }
    }

    internal class AliasManager
    {
        private int _uniqueID;
        private IList<Alias> _aliases;

        public AliasManager()
        {
            _aliases = new List<Alias>();
        }

        public Alias NewAlias(Type type)
        {
            Alias alias = new Alias();
            alias.Value = type.Name.Substring(0, 1) + _uniqueID++.ToString();

            _aliases.Add(alias);

            return alias;
        }
    }
}