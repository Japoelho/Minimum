using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Data;
using Minimum.Proxy;
using Minimum.Connection;
using Minimum.Connection.Interfaces;
using Minimum.DataAccess.Mapping;
using Minimum.DataAccess.Statement;

namespace Minimum.DataAccess
{
    public class DataAccess
    {
        private Maps _maps;
        private Proxies _proxies;
        private string _connectionName;

        public DataAccess(string connectionName)
        {
            _maps = new Maps();
            _proxies = new Proxies();
            _connectionName = connectionName;
        }

        public string StrGet<T>(params Criteria[] criteria) where T : class, new()
        {
            using (IConnection connection = ConnectionFactory.GetConnection(_connectionName))
            {
                ICommand command = connection.GetCommand();
                IStatement statement = ConnectionFactory.GetStatement(_maps);
                command.CommandText = statement.AddCriteria(criteria).Select(typeof(T));

                return command.CommandText;
            }
        }

        public T GetByID<T>(int elementID) where T : class, new()
        {
            Column identity = _maps.Map(typeof(T)).Columns.FirstOrDefault(p => p.IsIdentity);
            T element = default(T);

            using (IConnection connection = ConnectionFactory.GetConnection(_connectionName))
            {
                ICommand command = connection.GetCommand();
                IStatement statement = ConnectionFactory.GetStatement(_maps);
                command.CommandText = statement.AddCriteria(Criteria.EqualTo(identity.Property.Name, elementID)).Select(typeof(T));
                //System.Diagnostics.Trace.WriteLine(command.CommandText);
                using (IDataReader dataReader = command.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        element = (T)Load(statement.Alias(), dataReader);
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
                        T element = (T)Load(statement.Alias(), dataReader);
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
                        T element = (T)Load(statement.Alias(), dataReader);
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
                        T element = (T)Load(statement.Alias(), dataReader);
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

                Column identity = _maps.Map(typeof(T)).Columns.FirstOrDefault(p => p.IsIdentity);
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

        private object Load(Alias alias, IDataReader dataReader)
        {
            bool hasCollection = alias.Table.Columns.Any(c => c.IsCollection && !c.IsIgnore);
            int totalClasses = alias.Table.Columns.Count(c => c.IsClass && !c.IsIgnore);
            int totalAliases = alias.Aliases.Count;

            Table joinTable = alias.Table.Base;
            Alias joinAlias = alias.Aliases.FirstOrDefault(a => a.Table == joinTable && a.Column == null);
            while (joinTable != null)
            {
                totalClasses++;

                if (hasCollection == false) { hasCollection = joinTable.Columns.Any(p => p.IsCollection && !p.IsIgnore); }

                totalClasses += joinTable.Columns.Count(c => c.IsClass && !c.IsIgnore);
                if (joinAlias != null) { totalAliases += joinAlias.Aliases.Count; }

                joinTable = joinTable.Base;
            }

            if (totalClasses > totalAliases || hasCollection == true)
            {
                IProxy proxy = _proxies.GetProxy(alias.Table.Type);
                return LoadProxy(proxy, alias, dataReader);
            }
            else
            {
                object element = Activator.CreateInstance(alias.Table.Type);
                return LoadObject(element, alias, dataReader);
            }
        }

        private object LoadProxy(object element, Alias alias, IDataReader dataReader)
        {
            IProxy proxy = element as IProxy;

            if (alias.Table.Base != null)
            {
                LoadProxy(element, alias.Aliases.First(a => a.Table == alias.Table.Base && a.Column == null), dataReader);
            }

            //IList<PropertyMap> properties = aliasMap.ClassMap.Properties.Where(p => p.Ignore == null && p.IsClass == false && p.IsCollection == false).ToList();
            for (int i = 0; i < alias.Table.Columns.Count; i++)
            {
                if (alias.Table.Columns[i].IsIgnore || alias.Table.Columns[i].IsClass || alias.Table.Columns[i].IsCollection) { continue; }

                //string columnName = properties[i].Column != null ? properties[i].Column.Name : properties[i].Property.Name;
                object dataValue = dataReader[alias.Name + alias.Table.Columns[i].Name];
                if (dataValue != DBNull.Value)
                {
                    alias.Table.Columns[i].Property.SetValue(element, FormatReadValue(dataValue, alias.Table.Columns[i].Property.PropertyType), null);
                }
            }

            //IList<PropertyMap> classes = aliasMap.ClassMap.Properties.Where(p => p.Ignore == null && (p.IsClass == true || p.IsCollection == true)).ToList();
            for (int i = 0; i < alias.Table.Columns.Count; i++)
            {
                if (alias.Table.Columns[i].IsIgnore || (!alias.Table.Columns[i].IsClass && !alias.Table.Columns[i].IsCollection)) { continue; }
                
                Join[] joins = alias.Table.Columns[i].Joins;
                if (joins.Length == 0) { continue; }

                Type type = alias.Table.Columns[i].IsCollection ? alias.Table.Columns[i].Property.PropertyType.GetGenericArguments()[0] : alias.Table.Columns[i].Property.PropertyType;
                Table joinTable = _maps.Map(type);

                IList<Criteria> criterias = new List<Criteria>();

                Column thisIdentity = alias.Table.Columns.FirstOrDefault(p => p.IsIdentity);
                Column joinIdentity = joinTable.Columns.FirstOrDefault(p => p.IsIdentity);
                for (int j = 0; j < joins.Length; j++)
                {
                    Column property = null;
                    string name = null;
                    string where = null;

                    if (alias.Table.Columns[i].IsClass)
                    {
                        name = joins[j].PrimaryKey ?? joinIdentity.Property.Name;
                        property = joinTable.Columns.FirstOrDefault(c => c.Name == name);
                        where = joins[j].ForeignKey ?? joinIdentity.Name;
                    }
                    else if (alias.Table.Columns[i].IsCollection)
                    {
                        name = joins[j].PrimaryKey ?? thisIdentity.Property.Name;
                        property = joinTable.Columns.FirstOrDefault(c => c.Name == name);
                        where = joins[j].ForeignKey ?? thisIdentity.Name;
                    }

                    if (property == null) { throw new ArgumentException(String.Format(Maps.CoultNotResolveProperty, name, alias.Table.Type.Name)); }

                    criterias.Add(Criteria.EqualTo(property.Name, dataReader[alias.Name + where]));
                }

                bool isCollection = alias.Table.Columns[i].IsCollection;
                object dalObject = this;
                MethodInfo dalMethod = typeof(DataAccess).GetMethod("GetBy", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(Criteria[]) }, null).MakeGenericMethod(new Type[] { type });
                MethodInfo setMethod = alias.Table.Columns[i].Property.GetSetMethod();
                proxy.Add(alias.Table.Columns[i].Property.GetGetMethod().Name, (object instance, object[] iargs) =>
                {
                    object property = dalMethod.Invoke(dalObject, new object[] { criterias.ToArray() });

                    if (isCollection == false) { property = (property as IList).Count > 0 ? (property as IList)[0] : null; }

                    setMethod.Invoke(instance, new object[] { property });

                    return property;
                });
            }

            return element;
        }

        private object LoadObject(object element, Alias alias, IDataReader dataReader)
        {
            if (alias.Table.Base != null)
            {
                LoadObject(element, alias.Aliases.FirstOrDefault(a => a.Table == alias.Table.Base && a.Column == null), dataReader);
            }

            for (int i = 0; i < alias.Table.Columns.Count; i++)
            {
                if (alias.Table.Columns[i].IsIgnore || alias.Table.Columns[i].IsCollection) { continue; }

                if (alias.Table.Columns[i].IsClass == true)
                {
                    object property = Load(alias.Aliases.First(a => a.Column == alias.Table.Columns[i]), dataReader);
                    alias.Table.Columns[i].Property.SetValue(element, property);
                    continue;
                }

                //string columnName = alias.ClassMap.Properties[i].Column != null ? alias.ClassMap.Properties[i].Column.Name : alias.ClassMap.Properties[i].Property.Name;
                object dataValue = dataReader[alias.Name + alias.Table.Columns[i].Name];
                if (dataValue != DBNull.Value)
                {
                    alias.Table.Columns[i].Property.SetValue(element, FormatReadValue(dataValue, alias.Table.Columns[i].Property.PropertyType), null);
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
