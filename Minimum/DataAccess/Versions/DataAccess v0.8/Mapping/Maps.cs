using System;
using System.Collections.Generic;
using System.Reflection;

namespace Minimum.DataAccess.V08.Mapping
{
    internal class ClassMap
    {
        public Table Table { get; set; }
        public Join[] Joins { get; set; }
        public Type Type { get; set; }
        public ClassMap BaseClass { get; set; }
        public IList<PropertyMap> Properties { get; set; }

        public ClassMap()
        {
            Properties = new List<PropertyMap>();
        }
    }

    internal class PropertyMap
    {
        public Identity Identity { get; set; }
        public Ignore Ignore { get; set; }
        public Table Table { get; set; }
        public Column Column { get; set; }
        public Join[] Joins { get; set; }
        public PropertyInfo Property { get; set; }
        public bool IsClass { get; set; }
        public bool IsCollection { get; set; }

        public PropertyMap()
        {
            IsClass = false;
            IsCollection = false;
        }
    }

    internal class AliasMap
    {
        private AliasGenerator _aliasGenerator = null;
        public AliasMap Parent { get; private set; }
        public IList<AliasMap> Aliases { get; private set; }
        public ClassMap ClassMap { get; private set; }
        public PropertyMap PropertyMap { get; private set; }
        public string Alias { get; private set; }

        public AliasMap(AliasGenerator aliasGenerator, ClassMap classMap, PropertyMap propertyMap)
        {
            _aliasGenerator = aliasGenerator;

            Aliases = new List<AliasMap>();
            Alias = _aliasGenerator.GenerateAlias();
            ClassMap = classMap;
            PropertyMap = propertyMap;
        }

        public AliasMap MapAlias(ClassMap classMap, PropertyMap propertyMap)
        {
            AliasMap alias = new AliasMap(_aliasGenerator, classMap, propertyMap);
            alias.Parent = this;

            Aliases.Add(alias);

            return alias;
        }
    }

    internal class AliasGenerator
    {
        private int _uniqueID;
        public string UniqueName { get; private set; }

        public AliasGenerator(string uniqueName)
        {
            _uniqueID = 0;
            UniqueName = uniqueName;
        }

        public string GenerateAlias()
        {
            return UniqueName + _uniqueID++.ToString();
        }
    }
}
