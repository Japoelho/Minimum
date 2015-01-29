using System;
using System.Collections.Generic;
using System.Reflection;
using Minimum.DataAccess.Mapping;

namespace Minimum.DataAccess
{
    /// <summary>
    /// This class represents the mapping of a Type with a Table in the database.    
    /// </summary>
    public class QueryMap
    {
        #region [ Messages ]
        private const string InvalidTypeIsNull = "The type can not be null.";
        private const string InvalidType = "The type {0} is invalid for mapping, only non-generic, non-value types are valid for queries.";
        private const string InvalidIdentity_PropertyIsNull = "The property for the Identity can not be null.";
        private const string InvalidJoin_NoCriteria = "The parameter criteria On is required to have at least one.";
        #endregion

        #region [ Properties ]
        internal Type Type { get; private set; }
        public string Database { get; set; }
        public string Schema { get; set; }
        public string Table { get; set; }

        internal IList<ColumnMap> Columns { get; private set; }
        internal IList<JoinMap> Joins { get; private set; }
        #endregion

        #region [ Constructor ]
        public QueryMap(Type type)
        {
            if (type == null) { throw new ArgumentException(InvalidTypeIsNull); }
            if (type.IsValueType || type.IsGenericType || type.Equals(typeof(System.String)) || type.Equals(typeof(System.Object)) || !type.IsClass)
            { throw new ArgumentException(String.Format(InvalidType, type.Name)); }

            Type = type;

            Columns = new List<ColumnMap>();
            Joins = new List<JoinMap>();
        }
        #endregion

        #region [ Public ]
        public void Identity(string column, string property) { Identity(column, Type.GetProperty(property)); }
        public void Identity(string column, PropertyInfo property)
        {
            if (property == null) { throw new ArgumentException(InvalidIdentity_PropertyIsNull); }
            Columns.Add(new ColumnMap()
            {
                Query = this,
                Name = column,
                Command = "{0}",
                Property = property,                
                ResolvedType = property.PropertyType.IsGenericType ? property.PropertyType.GetGenericArguments()[0] : property.PropertyType,
                IsKey = true
            });
        }

        public void Column(string column, string property, QueryMap query = null) { Column(column, Type.GetProperty(property), query); }
        public void Column(string column, PropertyInfo property, QueryMap query = null)
        {
            Columns.Add(new ColumnMap()
            {
                Query = query ?? this,
                Name = column,
                Command = "{0}",
                Property = property,
                ResolvedType = property.PropertyType.IsGenericType ? property.PropertyType.GetGenericArguments()[0] : property.PropertyType
            });
        }

        public void Count(string column, string property, QueryMap query = null) { Count(column, Type.GetProperty(property), query); }
        public void Count(string column, PropertyInfo property, QueryMap query = null)
        {
            Columns.Add(new ColumnMap()
            {
                Query = query ?? this,
                Name = column,
                Command = "COUNT({0})",
                Property = property,
                ResolvedType = property.PropertyType.IsGenericType ? property.PropertyType.GetGenericArguments()[0] : property.PropertyType,
                IsAggregate = true
            });
        }

        public void Join(QueryMap query, string property, JoinType joinType, params On[] on) { Join(query, Type.GetProperty(property), joinType, on); }
        public void Join(QueryMap query, PropertyInfo property, JoinType joinType, params On[] on)
        {
            if (on.Length == 0) { throw new ArgumentException(InvalidJoin_NoCriteria); }
            Joins.Add(new JoinMap()
            {
                Query = query,
                Property = property,
                JoinType = joinType,
                On = on
            });
        }

        public void Lazy(QueryMap query, string property, JoinType joinType, params On[] on) { Lazy(query, Type.GetProperty(property), joinType, on); }
        public void Lazy(QueryMap query, PropertyInfo property, JoinType joinType, params On[] on)
        {
            if (on.Length == 0) { throw new ArgumentException(InvalidJoin_NoCriteria); }
            Joins.Add(new JoinMap()
            {
                Query = query,
                Property = property,
                JoinType = joinType,
                On = on,
                IsLazy = true
            });
        }
        
        public void Base(QueryMap query, JoinType joinType, params On[] on) 
        {
            if (on.Length == 0) { throw new ArgumentException(InvalidJoin_NoCriteria); }
            Joins.Add(new JoinMap()
            {
                Query = query,                
                JoinType = joinType,
                On = on,
                IsBase = true
            });
        }
        #endregion
    }

    internal class ColumnMap
    {
        internal QueryMap Query { get; set; }

        internal string Name { get; set; }
        internal string Command { get; set; }
        internal PropertyInfo Property { get; set; }
        internal Type ResolvedType { get; set; }
        
        internal bool IsKey { get; set; }
        internal bool IsAggregate { get; set; }
    }

    internal class JoinMap
    {
        internal QueryMap Query { get; set; }

        internal JoinType JoinType { get; set; }
        internal PropertyInfo Property { get; set; }
        internal On[] On { get; set; }

        public bool IsBase { get; set; }
        public bool IsLazy { get; set; }
    }
}
