using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Minimum.Proxy;
using Minimum.Connection;
using Minimum.Connection.Interfaces;
using Minimum.DataAccess.Mapping;
using Minimum.DataAccess.Statement;

namespace Minimum.DataAccess
{
    public class Query
    {
        //private object _loaded;
        private Alias _alias;
        private AliasManager _aliasManager;
        //private Query _parent;
        private QueryMap _original;
        private IStatement _syntax;
        private IConnectionInfo _connectionInfo;

        private string _database;
        private string _schema;
        private string _table;

        internal IList<Column> Columns { get; private set; }
        internal IList<Join> Joins { get; private set; }
        internal IList<Criteria> Criterias { get; private set; }

        public string Alias { get { return _alias.Value; } }

        public string Table { get { return (_database != null ? _database + "." : "") + (_schema != null ? _schema + "." : "") + _table; } }
        public Type Type { get { return _original.Type; } }

        public Query(IConnectionInfo connectionInfo, QueryMap queryMap)
        {
            _connectionInfo = connectionInfo;
            _syntax = StatementFactory.GetStatement(_connectionInfo);

            Columns = new List<Column>();
            Joins = new List<Join>();
            Criterias = new List<Criteria>();

            From(queryMap);
        }

        internal Query(Query parent)
        {
            _aliasManager = parent._aliasManager;
            _connectionInfo = parent._connectionInfo;
            _syntax = parent._syntax;

            Columns = new List<Column>();
            Joins = new List<Join>();
            Criterias = new List<Criteria>();
        }
        
        internal Query From(QueryMap map)
        {
            _aliasManager = new AliasManager();

            return _From(map);
        }

        public Query Where(params Criteria[] criteria)
        {
            for (int i = 0; i < criteria.Length; i++)
            { Criterias.Add(criteria[i]); }

            return this;
        }

        public Query Where<T>(Expression<Func<T, bool>> expression)
        {
            Criterias.Add(Parser.Criteria(expression));
            
            return this;
        }

        public void Column(string column, string property, Query from)
        {
            _Column(column, Type.GetProperty(property), "{0}", false, from);
        }

        public void Count(string column, string property, Query from)
        {
            _Column(column, Type.GetProperty(property), "COUNT({0})", true, from);
        }
        
        public Query Join(QueryMap map, JoinType joinType, params On[] on)
        {
            JoinMap joinMap = new JoinMap() { Query = map, JoinType = joinType, On = on };            
            Join join = new Join(joinMap);
            join.Query = new Query(this);
            join.Query._original = map;
            join.Query._alias = _aliasManager.NewAlias(map.Type);
            join.Query._database = map.Database;
            join.Query._schema = map.Schema;
            join.Query._table = map.Table;

            Joins.Add(join);

            return join.Query;
        }

        public IList<T> Select<T>() where T : class
        {
            IList<T> list = new List<T>();
            string s = _syntax.Select(this);
            using (IConnection connection = ConnectionFactory.GetConnection(_connectionInfo))
            {
                ICommand command = connection.GetCommand();
                command.CommandText = _syntax.Select(this);

                using (IDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        T element = (T)_Load(this, dataReader);
                        list.Add(element);
                    }
                }
            }

            return list;
        }

        public T Update<T>(T element) where T : class
        {            
            using (IConnection connection = ConnectionFactory.GetConnection(_connectionInfo))
            {
                ICommand command = connection.GetCommand();
                command.CommandText = _syntax.Update(this, element);

                command.ExecuteNonQuery();
            }

            return element;
        }

        public T Insert<T>(T element) where T : class
        {            
            using (IConnection connection = ConnectionFactory.GetConnection(_connectionInfo))
            {
                ICommand command = connection.GetCommand();
                command.CommandText = _syntax.Insert(this, element);

                object elementID = command.ExecuteScalar();

                Column identity = Columns.FirstOrDefault(p => p.IsKey);
                if (identity != null) { identity.Property.SetValue(element, Convert.ChangeType(elementID, identity.ResolvedType)); }
            }

            return element;
        }

        public T Delete<T>(T element) where T : class
        {
            string s = _syntax.Delete(this, element);
            using (IConnection connection = ConnectionFactory.GetConnection(_connectionInfo))
            {
                ICommand command = connection.GetCommand();
                command.CommandText = _syntax.Delete(this, element);

                command.ExecuteNonQuery();
            }

            return element;
        }

        #region [ Private ]
        private Query ResolveLoop(PropertyInfo property)
        {
            //if (property == null) { return null; }
            //if (Joins.Any(j => j.Property == property)) { return this; }

            //return _parent != null ? _parent.ResolveLoop(property) : null;
            return null;
        }

        private Query _From(QueryMap map)
        {
            _original = map;
            _alias = _aliasManager.NewAlias(map.Type);

            _database = _original.Database;
            _schema = _original.Schema;
            _table = _original.Table;

            for (int i = 0; i < map.Joins.Count; i++)
            {
                Join join = new Join(map.Joins[i]);
                join.Query = new Query(this)._From(map.Joins[i].Query); //ResolveLoop(property) ?? new Query(_connectionInfo, _aliasManager).From(map.Joins[i].Query, map.Joins[i].Property);
                Joins.Add(join);
            }

            for (int i = 0; i < map.Columns.Count; i++)
            {
                Column column = new Column(map.Columns[i]);
                column.Query = this; //TODO
                Columns.Add(column);
            }

            return this;
        }

        private void _Column(string column, PropertyInfo property, string command, bool isAggregate, Query from)
        {
            Column exists = Columns.FirstOrDefault(c => c.Property == property);
            if (exists != null) { Columns.Remove(exists); }

            ColumnMap columnMap = new ColumnMap()
            {
                Name = column,
                Command = command,
                Property = property,
                ResolvedType = property.PropertyType.IsGenericType ? property.PropertyType.GetGenericArguments()[0] : property.PropertyType,
                IsAggregate = isAggregate
            };
            Column newCol = new Column(columnMap);
            newCol.Query = from;
            Columns.Add(newCol);
        }

        private object _Load(Query query, IDataReader dataReader)
        {
            bool useProxy = query.Joins.Any(j => j.IsLazy);

            Query current = query;
            Join parent = query.Joins.FirstOrDefault(j => j.IsBase);
            while (parent != null)
            {
                current = parent.Query;
                useProxy = current.Joins.Any(j => j.IsLazy);
                if (useProxy) { break; }
                parent = current.Joins.FirstOrDefault(j => j.IsBase);
            }

            if (useProxy)
            {
                IProxy proxy = Proxies.GetInstance().GetProxy(query.Type);
                return _LoadProxy(proxy, query, dataReader);
            }
            else
            {
                object element = Activator.CreateInstance(query.Type);
                return _LoadObject(element, query, dataReader);
            }
        }

        private object _LoadObject(object element, Query query, IDataReader dataReader)
        {
            for (int i = 0; i < query.Columns.Count; i++)
            {
                object dataValue = dataReader[query.Columns[i].Query.Alias + "_" + query.Columns[i].Name];
                if (dataValue != DBNull.Value)
                {
                    query.Columns[i].Property.SetValue(element, _syntax.FormatReadValue(dataValue, query.Columns[i].Property.PropertyType), null);
                }
            }

            for (int i = 0; i < query.Joins.Count; i++)
            {
                if (query.Joins[i].Property == null) { continue; }

                if (query.Joins[i].IsBase)
                { _LoadObject(element, query.Joins[i].Query, dataReader); }
                else
                { query.Joins[i].Property.SetValue(element, _Load(query.Joins[i].Query, dataReader)); }
            }

            return element;
        }

        private object _LoadProxy(object element, Query query, IDataReader dataReader)
        {
            for (int i = 0; i < query.Columns.Count; i++)
            {
                object dataValue = dataReader[query.Columns[i].Query.Alias + "_" + query.Columns[i].Name];
                if (dataValue != DBNull.Value)
                {
                    query.Columns[i].Property.SetValue(element, _syntax.FormatReadValue(dataValue, query.Columns[i].Property.PropertyType), null);
                }
            }

            for (int i = 0; i < query.Joins.Count; i++)
            {
                if (query.Joins[i].IsBase)
                { 
                    _LoadProxy(element, query.Joins[i].Query, dataReader); 
                }
                else if (query.Joins[i].Property == null) { continue; }
                else if (query.Joins[i].IsLazy)
                {
                    Join joinMap = query.Joins[i];
                    IList<Criteria> criterias = new List<Criteria>();
                    for (int j = 0; j < query.Joins[i].On.Length; j++)
                    {
                        object joinWhere = dataReader[query.Alias + "_" + query.Joins[i].On[j].PrimaryKey];
                        Column column = query.Joins[i].Query.Columns.FirstOrDefault(p => p.Name == query.Joins[i].On[j].ForeignKey);
                        if (column == null) { throw new ArgumentException("Eita"); } //TODO:

                        criterias.Add(Criteria.EqualTo(column.Property.Name, joinWhere));
                    }

                    (element as IProxy).Add(query.Joins[i].Property.GetGetMethod().Name, (object instance, object[] iargs) =>
                    {
                        Query queryProxy = new Query(_connectionInfo, joinMap.Query._original);
                        queryProxy.Where(criterias.ToArray());                        

                        bool isCollection = joinMap.Property.PropertyType.IsGenericType && joinMap.Property.PropertyType.GetGenericTypeDefinition() == typeof(IList<>) ? true : false;
                        MethodInfo setMethod = joinMap.Property.GetSetMethod();
                        MethodInfo typedSelect = typeof(Query).GetMethod("Select", Type.EmptyTypes).MakeGenericMethod(queryProxy.Type);
                        
                        object property = typedSelect.Invoke(queryProxy, new object[] { });
                        if (!isCollection) { property = (property as IList).Count > 0 ? (property as IList)[0] : null; }

                        setMethod.Invoke(instance, new object[] { property });
                        return property;
                    }, Run.Once);
                }
                else
                { 
                    query.Joins[i].Property.SetValue(element, _Load(query.Joins[i].Query, dataReader)); 
                }
            }

            return element;
        }
        #endregion
    }

    internal class Column
    {
        private ColumnMap _original;
        public Query Query { get; set; }
        
        public string Name { get { return _original.Name; } }
        public string Command { get { return _original.Command; } }
        public PropertyInfo Property { get { return _original.Property; } }
        public Type ResolvedType { get { return _original.ResolvedType; } }
        
        public bool IsKey { get { return _original.IsKey; } }
        public bool IsAggregate { get { return _original.IsAggregate; } }

        public Column(ColumnMap map)
        {
            _original = map;
        }
    }

    internal class Join
    {
        private JoinMap _original;
        public Query Query { get; set; }
                
        public JoinType JoinType { get { return _original.JoinType; } }
        public PropertyInfo Property { get { return _original.Property; } }
        public On[] On { get { return _original.On; } }
        
        public bool IsBase { get { return _original.IsBase; } }
        public bool IsLazy { get; set; }

        public Join(JoinMap map)
        {
            _original = map;
            IsLazy = _original.IsLazy;
        }
    }    
}