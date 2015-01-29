using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Minimum.DataAccess.Mapping
{
    // - The default mapper.
    public class Mapper : IMapper
    {
        // - Error messages.
        private const string InvalidProperty_NoJoin = "The type {0} has an invalid class property {1}, the attribute [Join] was not specified. Use [Ignore] to skip this property.";
        private const string InvalidJoin_NoKeys = "The type {0} has an invalid parameterless join specified with the type {1}. Parameterless [Join] assumes a [Key] attribute is present on both classes for relation, which wasn't found.";
        private const string InvalidJoin_LazyNoVirtual = "The type {0} has an invalid lazy join specified with the type {1}. Lazy joins requires the property to be marked as virtual for late-loading.";

        public QueryMap Map(Type type)
        {
            // - Checks in the QueryVault if there's a queryMap already mapped of the requested type.
            QueryMap map = QueryVault.GetInstance().LoadQuery(type);
            if (map != null) { return map; }

            // - If not, create a new of this type and save it to the QueryVault for future requests.
            map = new QueryMap(type);
            QueryVault.GetInstance().SaveQuery(map);

            // - Setting the table values from the [Table] attribute or type name.
            Table table = Attribute.GetCustomAttribute(type, typeof(Table)) as Table;
            if (table != null)
            {
                map.Database = table.Database;
                map.Schema = table.Schema;
                map.Table = table.Name;
            }
            else
            { map.Table = type.Name; }

            // - Looping through the properties to define the Column/Properties relations.
            PropertyInfo[] properties = type.GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                // - Base properties will be dealt as if they're another table.
                if (!properties[i].DeclaringType.Equals(type)) { continue; }
                
                // - If this property has the [Join] attribute, resolve it later (after to get the key values).
                if (Attribute.GetCustomAttribute(properties[i], typeof(Join)) != null || Attribute.GetCustomAttribute(properties[i], typeof(Ignore)) != null) { continue; }
                                
                // - Find the target type of the property, even if it's generic or a collection.
                Type resolvedType = properties[i].PropertyType.IsGenericType ? properties[i].PropertyType.GetGenericArguments()[0] : properties[i].PropertyType;

                // - If the target type is a class property without [Join], skip.
                if (resolvedType.IsClass && !resolvedType.Equals(typeof(System.String)) && !resolvedType.Equals(typeof(System.Object)))
                { continue; }// throw new ArgumentException(String.Format(InvalidProperty_NoJoin, type.Name, properties[i].Name)); }

                // - Setting the name from the [Column] attribute or property name.
                Column column = Attribute.GetCustomAttribute(properties[i], typeof(Column)) as Column;
                string columnName = column != null ? column.Name : properties[i].Name;

                // - Check if it has [Key], then it's an identity column.
                if (Attribute.GetCustomAttribute(properties[i], typeof(Key)) != null)
                { map.Identity(columnName, properties[i]); }
                else
                { map.Column(columnName, properties[i]); }
            }

            // - Regular columns resolved, working on [Join] now.
            for (int i = 0; i < properties.Length; i++)
            {
                // - Skip base properties.
                if (!properties[i].DeclaringType.Equals(type)) { continue; }
                
                // - If the property doesn't have a [Join], skip it.
                Join join = Attribute.GetCustomAttribute(properties[i], typeof(Join)) as Join;
                if (join == null) { continue; }
                
                // - Get the target type.
                Type resolvedType = properties[i].PropertyType.IsGenericType ? properties[i].PropertyType.GetGenericArguments()[0] : properties[i].PropertyType;
                
                bool isClass = resolvedType.IsClass && !resolvedType.Equals(typeof(System.String)) && !resolvedType.Equals(typeof(System.Object));
                bool isCollection = properties[i].PropertyType.IsGenericType && properties[i].PropertyType.GetGenericTypeDefinition() == typeof(IList<>) ? true : false;

                // - Gets the QueryMap of the target type.
                QueryMap joinMap = Map(resolvedType);

                // - Get the criteria for joining.
                On[] on = Attribute.GetCustomAttributes(properties[i], typeof(On)) as On[];

                // - Find the Identities of both types, if any.
                ColumnMap thisIdentity = map.Columns.FirstOrDefault(c => c.IsKey);
                ColumnMap joinIdentity = joinMap.Columns.FirstOrDefault(c => c.IsKey);

                // - No [On] attribute at all, applying defaults.
                if (on.Length == 0)
                {
                    // - No identity on one of the types, unable to figure the Join criteria.
                    if (thisIdentity == null || joinIdentity == null)
                    { throw new ArgumentException(String.Format(InvalidJoin_NoKeys, type.Name, properties[i].Name)); }

                    On defaultOn = new On(
                        // - If the target type is a not a collection, assume the target's ID also has a column in this type.
                        !isCollection ? joinIdentity.Name : thisIdentity.Name,
                        // - If the target type is a collection, assume this ID also has a column in the target type.
                        isCollection ? thisIdentity.Name : joinIdentity.Name
                    );

                    on = new On[] { defaultOn };
                }
                else
                {
                    for (int j = 0; j < on.Length; j++)
                    {
                        // - Primary Key should always be valid.
                        //if (on[j].PrimaryKey == null) { }

                        if (on[j].ForeignKey == null) 
                        {
                            on[j].ForeignKey = isCollection ? thisIdentity.Name : joinIdentity.Name;
                        }
                    }
                }

                // - If the property is a collection, Lazy is true, else, only if [Lazy] is set.
                bool isLazy = isCollection ? isCollection : Attribute.GetCustomAttribute(properties[i], typeof(Lazy)) != null;
                
                // - If the property is set to Lazy but is not virtual, can't override with proxy, throw error.
                if (isLazy && !(properties[i].GetGetMethod() ?? properties[i].GetSetMethod()).IsVirtual)
                { throw new ArgumentException(String.Format(InvalidJoin_LazyNoVirtual, type.Name, resolvedType.Name)); }
                
                if (isLazy) { map.Lazy(joinMap, properties[i], join.JoinType, on); }
                else { map.Join(joinMap, properties[i], join.JoinType, on); }
            }

            // - Done with properties' [Join], now doing the [Join] on base class, if any.
            if (type.BaseType != null && !type.BaseType.Equals(typeof(Object)))
            {
                Join join = Attribute.GetCustomAttribute(type, typeof(Join)) as Join;
                if (join == null) { join = new Join(JoinType.InnerJoin); }

                QueryMap joinMap = Map(type.BaseType);

                On[] on = Attribute.GetCustomAttributes(type, typeof(On)) as On[];

                ColumnMap thisIdentity = map.Columns.FirstOrDefault(c => c.IsKey);
                ColumnMap joinIdentity = joinMap.Columns.FirstOrDefault(c => c.IsKey);

                // - No [On] attribute at all, applying defaults.
                if (on.Length == 0)
                {
                    if (thisIdentity == null || joinIdentity == null)
                    { throw new ArgumentException(String.Format(InvalidJoin_NoKeys, type.Name, type.BaseType.Name)); }

                    On defaultOn = new On(thisIdentity.Name, joinIdentity.Name);
                    on = new On[] { defaultOn };
                }
                else
                {
                    for (int j = 0; j < on.Length; j++)
                    {
                        // - Primary Key should always be valid.
                        //if (on[j].PrimaryKey == null) { }

                        if (on[j].ForeignKey == null)
                        { on[j].ForeignKey = joinIdentity.Name; }
                    }
                }

                map.Base(joinMap, join.JoinType, on);
            }

            return map;
        }
    }
}