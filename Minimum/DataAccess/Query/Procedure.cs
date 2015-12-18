using Minimum.Connection;
using Minimum.DataAccess.Statement;
using Minimum.Proxy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace Minimum.DataAccess
{
    public class Procedure<T>
    {
        public IMap _map;
        private IStatement _statement;
        private IConnection _connection;

        public Procedure(IConnection connection, IMap map)
        {            
            _map = map;
            
            _connection = connection;
            _statement = connection.NewStatement();
        }

        public IList<T> Execute(string query)
        {
            IList<T> list = new List<T>();

            using (DbConnection connection = _connection.NewConnection())
            {
                connection.Open();
                
                DbCommand command = connection.CreateCommand();
                command.CommandText = query;

                using (IDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        T entity = (T)Activator.CreateInstance(_map.Type);

                        for (int i = 0; i < dataReader.FieldCount; i++)
                        {
                            string column = dataReader.GetName(i);
                            Property property = _map.Properties.FirstOrDefault(p => p.ColumnName == column);
                            if (property == null) { continue; }

                            object dataValue = dataReader[_map.Properties[i].ColumnName];
                            if (dataValue != DBNull.Value)
                            {
                                _map.Properties[i].PropertyInfo.SetValue(entity, _statement.FormatReadValue(dataValue, _map.Properties[i].PropertyInfo.PropertyType), null);
                            }
                        }

                        list.Add(entity);
                    }
                }

                for (int i = 0; i < list.Count; i++)
                {
                    Select(list[i], _map, connection);
                }

                //connection.Close();
            }

            return list;
        }

        public int ExecuteScalar(string query)
        {
            int rows = 0;

            using (DbConnection connection = _connection.NewConnection())
            {
                connection.Open();

                DbCommand command = connection.CreateCommand();
                command.CommandText = query;

                object result = command.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    rows = Convert.ToInt32(result);
                }

                //connection.Close();
            }

            return rows;
        }

        private void Select(object entity, IMap map, DbConnection connection)
        {
            for (int i = 0; i < map.Relations.Count; i++)
            {
                if (map.Relations[i].IsLazy) { continue; }

                if (!map.Relations[i].IsCollection)
                {
                    object property = map.Relations[i].IsInheritance ? entity : map.Relations[i].PropertyInfo.GetValue(entity);
                    if (property == null) { continue; }

                    Select(property, map.Relations[i].JoinMap, connection);
                    continue;
                }

                Type listType = typeof(List<>).MakeGenericType(map.Relations[i].Type);
                map.Relations[i].PropertyInfo.SetValue(entity, Activator.CreateInstance(listType));

                object listProperty = map.Relations[i].PropertyInfo.GetValue(entity);

                IList<Criteria> criterias = new List<Criteria>();
                for (int j = 0; j < map.Relations[i].On.Length; j++)
                {
                    object joinWhere = map.Properties.FirstOrDefault(p => p.ColumnName == map.Relations[i].On[j].ForeignKey).PropertyInfo.GetValue(entity);
                    Property property = map.Relations[i].JoinMap.Properties.FirstOrDefault(p => p.ColumnName == map.Relations[i].On[j].PrimaryKey);
                    if (property == null) { throw new ArgumentException("Invalid On criteria for Join."); }

                    criterias.Add(Criteria.EqualTo(property.PropertyInfo.Name, joinWhere));
                }

                DbCommand command = connection.CreateCommand();
                command.CommandText = _statement.Select(map.Relations[i].JoinMap, criterias);

                using (IDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        object element = Load(map.Relations[i].JoinMap, dataReader);

                        (listProperty as IList).Add(element);
                    }
                }

                for (int j = 0; j < (listProperty as IList).Count; j++)
                {
                    Select((listProperty as IList)[j], map.Relations[i].JoinMap, connection);
                }
            }
        }

        private object Load(IMap map, IDataReader dataReader)
        {
            bool useProxy = map.Relations.Any(r => r.IsLazy);

            if (useProxy == false)
            {
                IMap current = map;
                Relation parent = map.Relations.FirstOrDefault(r => r.IsInheritance);
                while (parent != null)
                {
                    current = parent.JoinMap;
                    useProxy = current.Relations.Any(r => r.IsLazy);
                    if (useProxy) { break; }
                    parent = current.Relations.FirstOrDefault(r => r.IsInheritance);
                }
            }

            if (useProxy)
            {
                IProxy proxy = Proxies.GetInstance().GetProxy(map.Type);
                return LoadProxy(proxy, map, dataReader);
            }
            else
            {
                object entity = Activator.CreateInstance(map.Type);
                return LoadObject(entity, map, dataReader);
            }
        }

        private object LoadObject(object entity, IMap map, IDataReader dataReader)
        {
            bool isEmpty = true;
            for (int i = 0; i < map.Properties.Count; i++)
            {
                object dataValue = dataReader[map.Alias + "_" + map.Properties[i].ColumnName];
                if (dataValue != DBNull.Value)
                {
                    isEmpty = false;
                    map.Properties[i].PropertyInfo.SetValue(entity, _statement.FormatReadValue(dataValue, map.Properties[i].PropertyInfo.PropertyType), null);
                }
            }

            for (int i = 0; i < map.Relations.Count; i++)
            {
                if (map.Relations[i].IsCollection || map.Relations[i].IsLazy) { continue; }

                if (map.Relations[i].IsInheritance)
                {
                    if (LoadObject(entity, map.Relations[i].JoinMap, dataReader) != null) { isEmpty = false; }
                }
                else
                {
                    object property = Load(map.Relations[i].JoinMap, dataReader);
                    if (property != null)
                    {
                        isEmpty = false;
                        map.Relations[i].PropertyInfo.SetValue(entity, property);
                    }
                }
            }

            return isEmpty ? null : entity;
        }

        private object LoadProxy(object entity, IMap map, IDataReader dataReader)
        {
            bool isEmpty = true;
            for (int i = 0; i < map.Properties.Count; i++)
            {
                object dataValue = dataReader[map.Alias + "_" + map.Properties[i].ColumnName];
                if (dataValue != DBNull.Value)
                {
                    isEmpty = false;
                    map.Properties[i].PropertyInfo.SetValue(entity, _statement.FormatReadValue(dataValue, map.Properties[i].PropertyInfo.PropertyType), null);
                }
            }

            for (int i = 0; i < map.Relations.Count; i++)
            {
                if (map.Relations[i].IsCollection && !map.Relations[i].IsLazy) { continue; }

                if (map.Relations[i].IsInheritance)
                {
                    if (LoadProxy(entity, map.Relations[i].JoinMap, dataReader) != null) { isEmpty = false; }
                }
                else if (map.Relations[i].IsLazy)
                {
                    Relation relation = map.Relations[i];
                    IList<Criteria> criterias = new List<Criteria>();
                    for (int j = 0; j < map.Relations[i].On.Length; j++)
                    {
                        object joinWhere = dataReader[map.Alias + "_" + map.Relations[i].On[j].ForeignKey];
                        Property property = map.Relations[i].JoinMap.Properties.FirstOrDefault(p => p.ColumnName == map.Relations[i].On[j].PrimaryKey);
                        if (property == null) { throw new ArgumentException("Invalid On criteria for Join."); }

                        criterias.Add(Criteria.EqualTo(property.PropertyInfo.Name, joinWhere));
                    }

                    if (map.Relations[i].PropertyInfo.PropertyType.IsGenericType && map.Relations[i].PropertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(LazyList<>))
                    {
                        Type genericType = typeof(Query<>).MakeGenericType(relation.Type);
                        IQuery queryProxy = (IQuery)Activator.CreateInstance(genericType, _connection, relation.JoinMap);
                        queryProxy.Where(criterias);

                        Type listType = typeof(LazyList<>).MakeGenericType(relation.Type);
                        object list = Activator.CreateInstance(listType, queryProxy);
                        relation.PropertyInfo.SetValue(entity, list);

                        continue;
                    }

                    (entity as IProxy).Add(relation.PropertyInfo.GetGetMethod().Name, (object instance, object[] iargs) =>
                    {
                        Type genericType = typeof(Query<>).MakeGenericType(relation.Type);
                        IQuery queryProxy = (IQuery)Activator.CreateInstance(genericType, _connection, relation.JoinMap);
                        queryProxy.Where(criterias);

                        //bool isCollection = relation.PropertyInfo.PropertyType.IsGenericType && relation.PropertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IList<>) ? true : false;
                        MethodInfo setMethod = relation.PropertyInfo.GetSetMethod();
                        MethodInfo getMethod = genericType.GetMethod("Select");

                        object property = getMethod.Invoke(queryProxy, new object[] { });
                        if (!relation.IsCollection) { property = (property as IList).Count > 0 ? (property as IList)[0] : null; }

                        setMethod.Invoke(instance, new object[] { property });
                        return property;
                    }, Run.Once);
                }
                else
                {
                    object property = Load(map.Relations[i].JoinMap, dataReader);
                    if (property != null)
                    {
                        isEmpty = false;
                        map.Relations[i].PropertyInfo.SetValue(entity, property);
                    }
                }
            }

            return isEmpty ? null : entity;
        }
    }
}
