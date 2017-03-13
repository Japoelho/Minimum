using System;
using System.Collections.Generic;

namespace Minimum.DataAccess
{
    public interface IMap
    {
        IMap Parent { get; set; }
        string Alias { get; }
        string Name { get; }
        Type Type { get; }
        bool IsDynamic { get; }
        IList<Property> Properties { get; }
        IList<Relation> Relations { get; }
        string QueryText { get; }        

        Property Property(string property);
        Relation Relation(string property);
        Relation Relation(Type baseType);

        void Command(string text);
        void ToDatabase(string database);
        void ToSchema(string schema);
        void ToTable(string table);
        void HasAlias(string alias);
    }
}