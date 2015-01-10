using System;
using System.Linq;
using System.Linq.Expressions;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Minimum.Connection;
using Minimum.Connection.Interfaces;
using Minimum.DataAccess.V08.Mapping;
using Minimum.DataAccess.V08.Statement;
using Minimum.Proxy;

namespace Minimum.DataAccess.V08
{
    public class DataAccess
    {
        private Mappings _maps;
        private Proxies _proxies;
        private string _connectionName;

        public DataAccess(string connectionName)
        {
            _connectionName = connectionName;
            _maps = new Mappings();
            _proxies = new Proxies();
        }

        public T GetByID<T>(int elementID) where T : class, new()
        {
            PropertyMap property = _maps.Map(typeof(T)).Properties.FirstOrDefault(p => p.Identity != null);            
            T element = default(T);

            using (IConnection connection = ConnectionFactory.GetConnection(_connectionName))
            {
                ICommand command = connection.GetCommand();
                IStatement statement = ConnectionFactory.GetStatement(_maps);
                command.CommandText = statement.AddCriteria(Criteria.EqualTo(property.Property.Name, elementID)).Select(typeof(T));
                //System.Diagnostics.Trace.WriteLine(command.CommandText);
                using (IDataReader dataReader = command.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        element = (T)Load(statement.AliasMap(), dataReader);
                    }
                }
            }

            return element;
        }

        public IList<T> Get<T>() where T : class, new()
        {
            IList<T> list = new List<T>();
            
            using (IConnection connection = ConnectionFactory.GetConnection(_connectionName))
            {
                ICommand command = connection.GetCommand();
                IStatement statement = ConnectionFactory.GetStatement(_maps);
                command.CommandText = statement.Select(typeof(T));

                using (IDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        T element = (T)Load(statement.AliasMap(), dataReader);
                        list.Add(element);
                    }
                }
            }

            return list;
        }
        
        public IList<T> GetBy<T>(params Criteria[] criterias) where T : class, new()
        {
            IList<T> list = new List<T>();

            using (IConnection connection = ConnectionFactory.GetConnection(_connectionName))
            {
                ICommand command = connection.GetCommand();
                IStatement statement = ConnectionFactory.GetStatement(_maps);
                command.CommandText = statement.AddCriteria(criterias).Select(typeof(T));

                using (IDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        T element = (T)Load(statement.AliasMap(), dataReader);
                        list.Add(element);
                    }
                }
            }

            return list;
        }

        public IList<T> GetBy<T>(Expression<Func<T, bool>> expression) where T : class, new()
        { 
            IList<T> list = new List<T>();

            using (IConnection connection = ConnectionFactory.GetConnection(_connectionName))
            {
                ICommand command = connection.GetCommand();
                IStatement statement = ConnectionFactory.GetStatement(_maps);
                command.CommandText = statement.AddCriteria(ExpressionParser.GetCriteria(expression)).Select(typeof(T));

                using (IDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        T element = (T)Load(statement.AliasMap(), dataReader);
                        list.Add(element);
                    }
                }
            }

            return list;
        }

        public void Set<T>(T element) where T : class, new()
        {
            using (IConnection connection = ConnectionFactory.GetConnection(_connectionName))
            {
                Type type = (element as IProxy) != null ? (element as IProxy).Original : element.GetType();

                ICommand command = connection.GetCommand();
                IStatement statement = ConnectionFactory.GetStatement(_maps);
                command.CommandText = statement.Update(element, type);

                command.ExecuteNonQuery();
            }
        }

        public void Add<T>(T element) where T : class, new()
        {
            using (IConnection connection = ConnectionFactory.GetConnection(_connectionName))
            {
                Type type = (element as IProxy) != null ? (element as IProxy).Original : element.GetType();

                ICommand command = connection.GetCommand();
                IStatement statement = ConnectionFactory.GetStatement(_maps);
                command.CommandText = statement.Insert(element, type);

                object valueID = command.ExecuteScalar();

                PropertyMap identity = _maps.Map(typeof(T)).Properties.FirstOrDefault(p => p.Identity != null);
                identity.Property.SetValue(element, Convert.ChangeType(valueID, identity.Property.PropertyType));
            }
        }

        public void Del<T>(T element) where T : class, new()
        {
            using (IConnection connection = ConnectionFactory.GetConnection(_connectionName))
            {
                Type type = (element as IProxy) != null ? (element as IProxy).Original : element.GetType();

                ICommand command = connection.GetCommand();
                IStatement statement = ConnectionFactory.GetStatement(_maps);
                command.CommandText = statement.Delete(element, type);

                command.ExecuteNonQuery();
            }
        }
        
        private object Load(AliasMap aliasMap, IDataReader dataReader)
        {
            bool hasCollection = aliasMap.ClassMap.Properties.Any(p => p.IsCollection == true && p.Ignore == null);
            int totalClasses = aliasMap.ClassMap.Properties.Count(p => p.IsClass == true && p.Ignore == null);
            int totalAliases = aliasMap.Aliases.Count;

            ClassMap baseClass = aliasMap.ClassMap.BaseClass;
            AliasMap baseAlias = aliasMap.Aliases.FirstOrDefault(a => a.ClassMap == baseClass && a.PropertyMap == null);
            while (baseClass != null)
            {
                totalClasses++;

                if (hasCollection == false) { hasCollection = baseClass.Properties.Any(p => p.IsCollection == true && p.Ignore == null); }
                
                totalClasses += baseClass.Properties.Count(p => p.IsClass == true && p.Ignore == null);
                if (baseAlias != null) { totalAliases += baseAlias.Aliases.Count; }

                baseClass = baseClass.BaseClass;
            }

            if (totalClasses > totalAliases || hasCollection == true)
            {
                IProxy proxy = _proxies.GetProxy(aliasMap.ClassMap.Type);
                return LoadProxy(proxy, aliasMap, dataReader);
            }
            else
            {
                object element = Activator.CreateInstance(aliasMap.ClassMap.Type);
                return LoadObject(element, aliasMap, dataReader);
            }
        }

        private object LoadProxy(object element, AliasMap aliasMap, IDataReader dataReader)
        {
            IProxy proxy = element as IProxy;

            if (aliasMap.ClassMap.BaseClass != null)
            {
                LoadProxy(element, aliasMap.Aliases.First(a => a.ClassMap == aliasMap.ClassMap.BaseClass && a.PropertyMap == null), dataReader);
            }

            IList<PropertyMap> properties = aliasMap.ClassMap.Properties.Where(p => p.Ignore == null && p.IsClass == false && p.IsCollection == false).ToList();
            for (int i = 0; i < properties.Count; i++)
            {
                string columnName = properties[i].Column != null ? properties[i].Column.Name : properties[i].Property.Name;
                object dataValue = dataReader[aliasMap.Alias + columnName];
                if (dataValue != DBNull.Value)
                {
                    properties[i].Property.SetValue(element, FormatReadValue(dataValue, properties[i].Property.PropertyType), null);
                }
            }

            IList<PropertyMap> classes = aliasMap.ClassMap.Properties.Where(p => p.Ignore == null && (p.IsClass == true || p.IsCollection == true)).ToList();
            for (int i = 0; i < classes.Count; i++)
            {
                Join[] joins = classes[i].Joins;
                if (joins.Length == 0) { continue; }

                Type type = classes[i].IsCollection ? classes[i].Property.PropertyType.GetGenericArguments()[0] : classes[i].Property.PropertyType;
                ClassMap joinMap = _maps.Map(type);

                IList<Criteria> criterias = new List<Criteria>();

                PropertyMap thisIdentity = aliasMap.ClassMap.Properties.FirstOrDefault(p => p.Identity != null);
                PropertyMap joinIdentity = joinMap.Properties.FirstOrDefault(p => p.Identity != null);
                for (int j = 0; j < joins.Length; j++)
                {
                    string property = null;
                    string where = null;

                    if (classes[i].IsClass)
                    {
                        property = joins[j].PrimaryKey ?? joinIdentity.Property.Name;
                        where = joins[j].ForeignKey ?? (joinIdentity.Column != null ? joinIdentity.Column.Name : joinIdentity.Property.Name);
                    }
                    else if (classes[i].IsCollection)
                    {
                        property = joins[j].PrimaryKey ?? thisIdentity.Property.Name;
                        where = joins[j].ForeignKey ?? (thisIdentity.Column != null ? thisIdentity.Column.Name : thisIdentity.Property.Name);
                    }

                    criterias.Add(Criteria.EqualTo(property, dataReader[aliasMap.Alias + where]));                    
                }

                bool isCollection = classes[i].IsCollection;
                object dalObject = this;
                MethodInfo dalMethod = typeof(DataAccess).GetMethod("GetBy", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(Criteria[]) }, null).MakeGenericMethod(new Type[] { type });
                MethodInfo setMethod = classes[i].Property.GetSetMethod();
                proxy.Add(classes[i].Property.GetGetMethod().Name, (object instance, object[] iargs) =>
                {
                    object property = dalMethod.Invoke(dalObject, new object[] { criterias.ToArray() });
                    
                    if (isCollection == false) { property = (property as IList).Count > 0 ? (property as IList)[0] : null; }
                    
                    setMethod.Invoke(instance, new object[] { property });

                    return property;
                });
            }

            return element;
        }

        private object LoadObject(object element, AliasMap aliasMap, IDataReader dataReader)
        {
            if (aliasMap.ClassMap.BaseClass != null)
            {
                LoadObject(element, aliasMap.Aliases.FirstOrDefault(a => a.ClassMap == aliasMap.ClassMap.BaseClass && a.PropertyMap == null), dataReader);
            }

            for (int i = 0; i < aliasMap.ClassMap.Properties.Count; i++)
            {
                if (aliasMap.ClassMap.Properties[i].Ignore != null || aliasMap.ClassMap.Properties[i].IsCollection) { continue; }

                if (aliasMap.ClassMap.Properties[i].IsClass == true)
                {
                    object property = Load(aliasMap.Aliases.First(a => a.PropertyMap == aliasMap.ClassMap.Properties[i]), dataReader);
                    aliasMap.ClassMap.Properties[i].Property.SetValue(element, property);
                    continue;
                }

                string columnName = aliasMap.ClassMap.Properties[i].Column != null ? aliasMap.ClassMap.Properties[i].Column.Name : aliasMap.ClassMap.Properties[i].Property.Name;
                object dataValue = dataReader[aliasMap.Alias + columnName];
                if (dataValue != DBNull.Value)
                {
                    aliasMap.ClassMap.Properties[i].Property.SetValue(element, FormatReadValue(dataValue, aliasMap.ClassMap.Properties[i].Property.PropertyType), null);
                }
            }

            return element;
        }

        private object FormatReadValue(object value, Type valueType)
        {
            //Fix para Nullables
            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (valueType.GetGenericArguments()[0].IsEnum)
                { return Enum.Parse(valueType.GetGenericArguments()[0], value.ToString()); }

                if (valueType.GetGenericArguments()[0].IsValueType)
                { valueType = valueType.GetGenericArguments()[0]; }
            }

            switch (valueType.Name)
            {
                case "Boolean":
                case "bool":
                case "Int64":
                case "Int32":
                case "int":
                    {
                        return Convert.ChangeType(value, valueType);
                    }
                default:
                    {
                        return value;
                    }
            }
        }
    }
}