﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        public bool IsDynamic { get; set; }
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
            PropertyInfo propertyInfo = Type.GetProperties().FirstOrDefault(p => p.Name == property && p.DeclaringType == Type) ?? Type.GetProperty(property);
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
        public string Name { get { return _propertyInfo != null ? _propertyInfo.Name : ColumnName; } }
        public string ColumnName { get; private set; }
        public bool IsIdentity { get; private set; }
        public bool IsCascade { get; private set; }
        public Type Type { get; private set; }
        
        private PropertyInfo _propertyInfo;

        public Property()
        {
            Type = typeof(Object);
        }

        public Property(PropertyInfo propertyInfo)
        {            
            Type = propertyInfo.PropertyType.IsGenericType ? propertyInfo.PropertyType.GetGenericArguments()[0] : propertyInfo.PropertyType;
            _propertyInfo = propertyInfo;

            // - Default Value
            ColumnName = propertyInfo.Name;
        }

        public object GetValue(object entity)
        {
            if (_propertyInfo == null) { return (entity as IDictionary<string, object>)[ColumnName]; }

            return _propertyInfo.GetValue(entity);
        }

        public void SetValue(object entity, object value)
        {
            if (_propertyInfo == null) { (entity as IDictionary<string, object>)[ColumnName] = value; }

            _propertyInfo.SetValue(entity, value);
        }

        public bool IsNullable()
        {
            if (_propertyInfo == null) { return true; }

            if (Attribute.GetCustomAttribute(_propertyInfo, typeof(RequiredAttribute)) != null) { return false; }

            if (_propertyInfo.PropertyType.IsGenericType && _propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            { return true; }

            if (_propertyInfo.PropertyType.IsEnum) { return false; }

            switch (_propertyInfo.PropertyType.Name)
            {
                case "Int16":
                case "Int32":
                case "Int64":
                case "Boolean":
                    return false;
                case "Single":
                case "Decimal":
                    return true;
                case "DateTime":
                    return false;
                case "String":
                case "Object":
                default:
                    return true;
            }
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

        public Property Cascade(bool value = true)
        {
            IsCascade = value;

            return this;
        }
    }

    public class Relation
    {
        public IMap JoinMap { get; private set; }
        public JoinType JoinType { get; private set; }
        public Order[] Order { get; private set; }
        public On[] On { get; private set; }
        public bool IsInheritance { get; private set; }
        public bool IsLazy { get; private set; }
        public bool IsCollection { get; private set; }
        public bool IsCascade { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }
        public Type Type { get; private set; }

        public Relation()
        { }

        public Relation JoinWith(IMap map)
        {
            JoinMap = map;

            return this;
        }

        public Relation JoinAs(Join join)
        {
            JoinType = join != null ? join.JoinType : JoinType.LeftJoin;

            return this;
        }

        public Relation JoinOn(On[] on)
        {
            On = on;

            return this;
        }

        public Relation OrderBy(Order[] order)
        {
            Order = order;

            return this;
        }

        public Relation Property(PropertyInfo propertyInfo)
        {
            Type = propertyInfo.PropertyType.IsGenericType ? propertyInfo.PropertyType.GetGenericArguments()[0] : propertyInfo.PropertyType;
            PropertyInfo = propertyInfo;
            IsCollection = (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IList<>)) || typeof(IList).IsAssignableFrom(propertyInfo.PropertyType);

            return this;
        }

        public Relation Inherits(bool value = true)
        {
            IsInheritance = value;

            return this;
        }

        public Relation Lazy(Lazy lazy)
        {            
            IsLazy = lazy != null ? lazy.IsLazy : false;

            return this;
        }

        public Relation Lazy(bool value = true)
        {
            IsLazy = value;

            return this;
        }

        public Relation Cascade(bool value = true)
        {
            IsCascade = value;

            return this;
        }
    }
}
