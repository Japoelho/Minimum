using System;

namespace Minimum.DataAccess
{
    internal class AliasFactory
    {
        private int _uniqueCounter;

        public AliasFactory()
        { _uniqueCounter = 0; }

        public string GenerateAlias(Type type)
        {
            return type.Name.Substring(0, 1) + _uniqueCounter++.ToString();
        }
    }
}
