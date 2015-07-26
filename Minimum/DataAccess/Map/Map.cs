using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Minimum.DataAccess
{
    public class Map<T> : IMap where T : class
    {
        private string _database;
        private string _schema;
        private string _table;
        private string _alias;

        public IMap Parent { get; set; }
        public string Name { get { return (_database != null ? _database + "." : null) + (_schema != null ? _schema + "." : null) + _table; } }
        public string Alias { get { return _alias; } }
        public Type Type { get; private set; }
        public IList<Property> Properties { get; private set; }
        public IList<Relation> Relations { get; private set; }
        public string QueryText { get; private set; }

        public Map()
        {
            Type = typeof(T);
            Properties = new List<Property>();
            Relations = new List<Relation>();
        }

        public void Command(string command)
        {
            QueryText = command;
        }

        public void ToTable(string table)
        {
            _table = table;
        }

        public void ToSchema(string schema)
        {
            _schema = schema;
        }

        public void ToDatabase(string database)
        {
            _database = database;
        }

        public void HasAlias(string alias)
        {
            _alias = alias;
        }

        public Property Property(string property)
        {
            PropertyInfo propertyInfo = Type.GetProperty(property);
            if (propertyInfo == null) { return null; }

            Property p = new Property(propertyInfo);
            Properties.Add(p);

            return p;
        }

        public Property Property<P>(Expression<Func<T, P>> expression)
        {
            PropertyInfo propertyInfo = MapExpression.MapProperty(expression);
            if (propertyInfo == null) { return null; }

            Property p = new Property(propertyInfo);
            Properties.Add(p);

            return p;
        }

        public Relation Relation(Type baseType)
        {
            Relation relation = new Relation();
            Relations.Add(relation);

            return relation;
        }

        public Relation Relation(string property)
        {
            PropertyInfo propertyInfo = Type.GetProperty(property);
            if (propertyInfo == null) { return null; }
            //if (!propertyInfo.PropertyType.IsClass || propertyInfo.PropertyType.Equals(typeof(String)) || propertyInfo.PropertyType.Equals(typeof(Object))) { return null; }

            Relation relation = new Relation();
            Relations.Add(relation);

            return relation;
        }

        public Relation Relation<P>(Expression<Func<T, P>> expression) where P : class
        {
            PropertyInfo propertyInfo = MapExpression.MapProperty(expression);
            if (propertyInfo == null) { return null; }
            if (!propertyInfo.PropertyType.IsClass || propertyInfo.PropertyType.Equals(typeof(String)) || propertyInfo.PropertyType.Equals(typeof(Object))) { return null; }

            Relation relation = new Relation();
            Relations.Add(relation);

            return relation;
        }
    }

    public class Property
    {
        internal string ColumnName { get; private set; }
        
        internal bool IsIdentity { get; private set; }

        internal Type Type { get; private set; }
        internal PropertyInfo PropertyInfo { get; private set; }

        public Property(PropertyInfo propertyInfo)
        {
            Type = propertyInfo.PropertyType.IsGenericType ? propertyInfo.PropertyType.GetGenericArguments()[0] : propertyInfo.PropertyType;
            PropertyInfo = propertyInfo;

            // - Default Value
            ColumnName = propertyInfo.Name;
        }

        public Property ToColumn(Column column)
        {
            if (column != null) { ColumnName = column.Name; }

            return this;
        }

        public Property ToColumn(string name)
        {
            ColumnName = name;

            return this;
        }

        public Property Identity(bool value = true)
        {
            IsIdentity = value;

            return this;
        }
    }

    public class Relation
    {
        internal IMap JoinMap { get; private set; }
        internal JoinType JoinType { get; private set; }
        internal On[] On { get; private set; }
        internal bool IsInheritance { get; private set; }
        internal bool IsLazy { get; private set; }
        internal bool IsCollection { get; private set; }
        internal PropertyInfo PropertyInfo { get; private set; }
        internal Type Type { get; private set; }

        public Relation()
        { }

        internal Relation JoinWith(IMap map)
        {
            JoinMap = map;

            return this;
        }

        internal Relation JoinAs(Join join)
        {
            JoinType = join != null ? join.JoinType : JoinType.LeftJoin;

            return this;
        }

        internal Relation JoinOn(On[] on)
        {
            On = on;

            return this;
        }

        internal Relation Property(PropertyInfo propertyInfo)
        {
            Type = propertyInfo.PropertyType.IsGenericType ? propertyInfo.PropertyType.GetGenericArguments()[0] : propertyInfo.PropertyType;
            PropertyInfo = propertyInfo;
            IsCollection = (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IList<>)) || typeof(IList).IsAssignableFrom(propertyInfo.PropertyType);

            return this;
        }

        internal Relation Inherits(bool value = true)
        {
            IsInheritance = value;

            return this;
        }

        internal Relation Lazy(Lazy lazy)
        {            
            IsLazy = lazy != null ? lazy.IsLazy : false;

            return this;
        }

        internal Relation Lazy(bool value = true)
        {
            IsLazy = value;

            return this;
        }
    }
}
