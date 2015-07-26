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
        private IStatement _statement;
        private IConnection _connection;

        public IMap Map { get; private set; }

        public Procedure(IConnection connection, IMap map)
        {            
            Map = map;
            
            _connection = connection;
            _statement = connection.NewStatement();
        }

        public IList<T> Execute(params object[] parameters)
        {
            IList<T> list = new List<T>();

            using (DbConnection connection = _connection.NewConnection())
            {
                connection.Open();
                
                string query = Map.QueryText;
                if (Text.Occurrences(query, '?') > parameters.Length && parameters.Length == 1)
                {
                    query = query.Replace("?", _statement.FormatWriteValue(parameters[0], parameters[0].GetType()));
                }
                else
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        query = Text.Replace(query, "?", _statement.FormatWriteValue(parameters[i], parameters[i].GetType()));
                    }
                }

                DbCommand command = connection.CreateCommand();
                command.CommandText = query;

                using (IDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        T entity = (T)Activator.CreateInstance(Map.Type);

                        for (int i = 0; i < Map.Properties.Count; i++)
                        {
                            object dataValue = dataReader[Map.Properties[i].ColumnName];
                            if (dataValue != DBNull.Value)
                            {
                                Map.Properties[i].PropertyInfo.SetValue(entity, _statement.FormatReadValue(dataValue, Map.Properties[i].PropertyInfo.PropertyType), null);
                            }
                        }

                        list.Add(entity);
                    }
                }

                for (int i = 0; i < list.Count; i++)
                {
                    Select(list[i], Map, connection);
                }

                connection.Close();
            }

            return list;
        }

        public IList<T> ExecuteQuery(string query)
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
                        T entity = (T)Activator.CreateInstance(Map.Type);

                        for (int i = 0; i < Map.Properties.Count; i++)
                        {
                            object dataValue = dataReader[Map.Properties[i].ColumnName];
                            if (dataValue != DBNull.Value)
                            {
                                Map.Properties[i].PropertyInfo.SetValue(entity, _statement.FormatReadValue(dataValue, Map.Properties[i].PropertyInfo.PropertyType), null);
                            }
                        }

                        list.Add(entity);
                    }
                }

                for (int i = 0; i < list.Count; i++)
                {
                    Select(list[i], Map, connection);
                }

                connection.Close();
            }

            return list;
        }

        private void Select(object entity, IMap map, DbConnection connection)
        {
            for (int i = 0; i < map.Relations.Count; i++)
            {
                if (!map.Relations[i].IsCollection || map.Relations[i].IsLazy) { continue; }

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
            for (int i = 0; i < map.Properties.Count; i++)
            {
                object dataValue = dataReader[map.Alias + "_" + map.Properties[i].ColumnName];
                if (dataValue != DBNull.Value)
                {
                    map.Properties[i].PropertyInfo.SetValue(entity, _statement.FormatReadValue(dataValue, map.Properties[i].PropertyInfo.PropertyType), null);
                }
            }

            for (int i = 0; i < map.Relations.Count; i++)
            {
                if (map.Relations[i].IsCollection || map.Relations[i].IsLazy) { continue; }

                if (map.Relations[i].IsInheritance)
                {
                    LoadObject(entity, map.Relations[i].JoinMap, dataReader);
                }
                else
                {
                    //if (dataReader[map.Relations[i].Query.Alias + "_" + map.Joins[i].On[0].ForeignKey] == DBNull.Value) { continue; }
                    map.Relations[i].PropertyInfo.SetValue(entity, Load(map.Relations[i].JoinMap, dataReader));
                }
            }

            return entity;
        }

        private object LoadProxy(object entity, IMap map, IDataReader dataReader)
        {
            for (int i = 0; i < map.Properties.Count; i++)
            {
                object dataValue = dataReader[map.Alias + "_" + map.Properties[i].ColumnName];
                if (dataValue != DBNull.Value)
                {
                    map.Properties[i].PropertyInfo.SetValue(entity, _statement.FormatReadValue(dataValue, map.Properties[i].PropertyInfo.PropertyType), null);
                }
            }

            for (int i = 0; i < map.Relations.Count; i++)
            {
                if (map.Relations[i].IsCollection && !map.Relations[i].IsLazy) { continue; }

                if (map.Relations[i].IsInheritance)
                {
                    LoadProxy(entity, map.Relations[i].JoinMap, dataReader);
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
                    //if (dataReader[map.Relations[i].Query.Alias + "_" + map.Joins[i].On[0].ForeignKey] == DBNull.Value) { continue; }
                    map.Relations[i].PropertyInfo.SetValue(entity, Load(map.Relations[i].JoinMap, dataReader));
                }
            }

            return entity;
        }
    }
}
