using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Minimum.DataAccess.Mapping
{
    internal class Maps
    {
        private const string InvalidType = "The type {0} isn't valid for mapping. Accepted types are non-value types, not-generic user defined classes.";
        private const string AttributeNotFound = "The type {0} doesn't have the \"Table\" attribute defined.";
        private const string NoIdentityColumn = "The type {0} doesn't have an Identity property defined.";
        internal const string CoultNotResolveProperty = "The property {0} wasn't found in the type {1}.";

        private IList<Table> _tables;

        public Maps()
        {
            _tables = new List<Table>();
        }

        public Table Map(Type type)
        {
            if (type.IsValueType || type.IsGenericType || type.Equals(typeof(System.String)) || type.Equals(typeof(System.Object)) || !type.IsClass)
            { throw new ArgumentException(String.Format(InvalidType, type.Name)); }

            Table table = _tables.FirstOrDefault(t => t.Type.Equals(type));
            if (table != null) { return table; }

            table = Attribute.GetCustomAttribute(type, typeof(Table)) as Table;
            if (table == null)
            { throw new ArgumentException(String.Format(AttributeNotFound, type.Name)); }

            table.Type = type;
            table.Columns = new List<Column>();
            table.Joins = Attribute.GetCustomAttributes(type, typeof(Join)) as Join[];
            if (type.BaseType != null && !type.BaseType.Equals(typeof(System.Object)))
            {
                table.Base = Map(type.BaseType);
            }

            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (!property.DeclaringType.Equals(type)) { continue; }

                Column column = Attribute.GetCustomAttribute(property, typeof(Column)) as Column ?? new Column();                
                //column.Table = table; //TODO?
                column.Name = column.Name ?? property.Name;
                column.Property = property;                
                //column.Aggregate = Attribute.GetCustomAttribute(property, typeof(Aggregate)) as Aggregate;
                column.Joins = Attribute.GetCustomAttributes(property, typeof(Join)) as Join[];
                column.IsIgnore = Attribute.GetCustomAttribute(property, typeof(Ignore)) != null ? true : false;
                column.IsIdentity = Attribute.GetCustomAttribute(property, typeof(Key)) != null ? true : false;
                //column.IsAggregate = column.Aggregate != null ? true : false;
                column.IsClass = property.PropertyType.IsClass && !property.PropertyType.Equals(typeof(System.String)) && !property.PropertyType.Equals(typeof(System.Object)) ? true : false;
                column.IsCollection = property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(IList<>) ? true : false;
                
                if (column.IsClass == true) { Map(property.PropertyType); }
                if (column.IsCollection == true) { Map(property.PropertyType.GetGenericArguments()[0]); }

                table.Columns.Add(column);
            }

            if (table.Columns.FirstOrDefault(c => c.IsIdentity) == null) { throw new ArgumentException(String.Format(NoIdentityColumn, type.Name)); }
            if (!_tables.Contains(table)) { _tables.Add(table); }
            return table;
        }
    }

    internal class Alias
    {
        private AliasGenerator _generator = null;
        public string Name { get; private set; }
        public Table Table { get; private set; }
        public Column Column { get; private set; }

        //public Alias Parent { get; private set; }
        public IList<Alias> Aliases { get; private set; }

        public Alias(string prefix, Table table, Column column)
        {
            _generator = new AliasGenerator(prefix);

            Name = _generator.GenerateAlias();            
            Table = table;
            Column = column;
            Aliases = new List<Alias>();
        }

        private Alias(AliasGenerator generator, Table table, Column column)
        {
            _generator = generator;

            Name = _generator.GenerateAlias();
            Table = table;
            Column = column;
            Aliases = new List<Alias>();
        }

        public Alias MapAlias(Table table, Column column)
        {
            Alias alias = Aliases.FirstOrDefault(a => a.Table == table && a.Column == column);
            if (alias != null) { return alias; }

            alias = new Alias(_generator, table, column);
            //alias.Parent = this;
            Aliases.Add(alias);
            
            return alias;
        }

        public class AliasGenerator
        {
            private int _uniqueID;
            private string _uniqueName;

            public AliasGenerator(string namePrefix)
            {
                _uniqueID = 0;
                _uniqueName = namePrefix;
            }

            public string GenerateAlias()
            {
                return _uniqueName + _uniqueID++.ToString();
            }
        }
    }    
}
