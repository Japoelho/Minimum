using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Minimum.DataAccess
{
    internal class AutoMapper : IMapper
    {
        private const string Identity_Not_Found = "No [Identity] property found on class {0}.";
        private const string Command_Text_Null = "The command text is null.";        

        public IMap Map(Type type)
        {
            IMap map = MapCache.GetInstance().Load(type);
            if (map != null) { return map; }

            map = Map(type, new AliasFactory());
            MapCache.GetInstance().Save(map);

            return map;
        }

        public IMap Map(Type type, params string[] properties)
        {
            if (properties == null || properties.Length == 0) { return Map(type); }

            return Map(type, new AliasFactory(), properties);
        }

        public IMap DynamicMap(string table)
        {
            Map<Object> map = new Map<object>();
            map.ToTable(table);
            map.HasAlias("T");
            map.IsDynamic = true;

            return map;
        }

        public IMap DynamicMap(string table, dynamic type)
        {
            Map<Object> map = new Map<object>();
            map.ToTable(table);
            map.HasAlias("T");
            map.IsDynamic = true;
            
            IDictionary<string, object> dictionary = type as IDictionary<string, object>;
            for (int i = 0; i < dictionary.Keys.Count; i++)
            {
                map.Properties.Add(new Property().ToColumn(dictionary.Keys.ElementAt(i)));             
            }

            return map;
        }

        private IMap Map(Type type, AliasFactory aliasFactory, IMap parent = null)
        {
            IMap map = (IMap)Activator.CreateInstance(typeof(Map<>).MakeGenericType(type));
            map.Parent = parent;

            Command command = Attribute.GetCustomAttribute(type, typeof(Command)) as Command;
            if (command != null)
            {
                if (String.IsNullOrEmpty(command.Text)) { throw new ArgumentException(Command_Text_Null); }
                map.Command(command.Text); 
            }

            Table table = Attribute.GetCustomAttribute(type, typeof(Table)) as Table;
            if (table != null)
            {
                map.ToDatabase(table.Database);
                map.ToSchema(table.Schema);
                map.ToTable(table.Name);
            }
            else
            {
                map.ToTable(type.Name);
            }

            map.HasAlias(aliasFactory.GenerateAlias(type));

            bool isPartial = Attribute.GetCustomAttribute(type, typeof(Join)) == null;
            if (type.BaseType != null && !type.BaseType.Equals(typeof(Object)) && !isPartial)
            {
                IMap joinMap = Map(type.BaseType, aliasFactory);

                Relation relation = map.Relation(type.BaseType);
                relation.JoinWith(joinMap);
                relation.JoinAs(Attribute.GetCustomAttribute(type, typeof(Join)) as Join);
                relation.JoinOn(Attribute.GetCustomAttributes(type, typeof(On)) as On[]);
                relation.Inherits();
            }

            PropertyInfo[] properties = type.GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                if (!properties[i].DeclaringType.Equals(type) && !isPartial) { continue; }
                if (Attribute.GetCustomAttribute(properties[i], typeof(Ignore)) != null) { continue; }

                Dynamic dynamicAttribute = Attribute.GetCustomAttribute(properties[i], typeof(Dynamic)) as Dynamic;

                if (// - Se é classe não array diferente de "String" e "Object"
                    (properties[i].PropertyType.IsClass && !properties[i].PropertyType.IsArray && !properties[i].PropertyType.Equals(typeof(System.String)) && !properties[i].PropertyType.Equals(typeof(System.Object))) || 
                    // - Se é IList
                    (properties[i].PropertyType.IsGenericType && (properties[i].PropertyType.GetGenericTypeDefinition() == typeof(IList<>) || typeof(IList).IsAssignableFrom(properties[i].PropertyType))) ||
                    // - Se é Dynamic
                    dynamicAttribute != null)
                {
                    Relation relation = map.Relation(properties[i].Name);
                    relation.Property(properties[i]);

                    if (dynamicAttribute != null)
                    {
                        Map<dynamic> dynamicMap = new Map<dynamic>();
                        dynamicMap.ToDatabase(dynamicAttribute.Database);
                        dynamicMap.ToSchema(dynamicAttribute.Schema);
                        dynamicMap.ToTable(dynamicAttribute.Table);
                        dynamicMap.HasAlias(aliasFactory.GenerateAlias(properties[i].PropertyType));
                        dynamicMap.IsDynamic = true;

                        relation.JoinWith(dynamicMap);
                    }
                    else
                    {
                        relation.JoinWith(Resolve(relation.Type, map) ?? Map(relation.Type, aliasFactory, map));
                    }

                    relation.JoinAs(Attribute.GetCustomAttribute(properties[i], typeof(Join)) as Join);
                    relation.JoinOn(Attribute.GetCustomAttributes(properties[i], typeof(On)) as On[]);
                    relation.OrderBy(Attribute.GetCustomAttributes(properties[i], typeof(Order)) as Order[]);
                    relation.Lazy(Attribute.GetCustomAttribute(properties[i], typeof(Lazy)) as Lazy);
                    relation.Cascade(Attribute.GetCustomAttribute(properties[i], typeof(Cascade)) != null);

                    continue;
                }
                
                Property property = map.Property(properties[i].Name);
                property.Identity(Attribute.GetCustomAttribute(properties[i], typeof(Identity)) != null);
                property.Cascade(Attribute.GetCustomAttribute(properties[i], typeof(Cascade)) != null);
                property.ToColumn(Attribute.GetCustomAttribute(properties[i], typeof(Column)) as Column);
            }

            // - Convenções
            Property identity = map.Properties.FirstOrDefault(p => p.IsIdentity);
            if (identity == null && command == null) { throw new ArgumentException(String.Format(Identity_Not_Found, type.Name)); }

            for (int i = 0; i < map.Relations.Count; i++)
            {
                bool isInheritance = map.Relations[i].IsInheritance;
                bool isCollection = isInheritance ? false : (map.Relations[i].PropertyInfo.PropertyType.IsGenericType && map.Relations[i].PropertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IList<>)) || typeof(IList).IsAssignableFrom(map.Relations[i].PropertyInfo.PropertyType);
                
                if (map.Relations[i].On.Length == 0)
                {
                    Property joinIdentity = map.Relations[i].JoinMap.Properties.FirstOrDefault(p => p.IsIdentity);
                    if (joinIdentity == null) { throw new ArgumentException(String.Format(Identity_Not_Found, map.Relations[i].Type.Name)); }

                    On[] on = new On[1];
                    on[0] = new On(isCollection || isInheritance ? identity.ColumnName : joinIdentity.ColumnName, isCollection ? identity.ColumnName : joinIdentity.ColumnName);                    
                    
                    map.Relations[i].JoinOn(on);
                }

                //if (map.Relations[i].PropertyInfo.PropertyType.IsGenericType && map.Relations[i].PropertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(LazyList<>))
                //{ map.Relations[i].Lazy(false); }
                if (map.Relations[i].PropertyInfo != null && Attribute.GetCustomAttribute(map.Relations[i].PropertyInfo, typeof(Lazy)) == null)
                { map.Relations[i].Lazy(isCollection); }                
            }

            return map;
        }

        private IMap Map(Type type, AliasFactory aliasFactory, string[] mapProperties)
        {
            IMap map = (IMap)Activator.CreateInstance(typeof(Map<>).MakeGenericType(type));
                        
            Table table = Attribute.GetCustomAttribute(type, typeof(Table)) as Table;
            if (table != null)
            {
                map.ToDatabase(table.Database);
                map.ToSchema(table.Schema);
                map.ToTable(table.Name);
            }
            else
            {
                map.ToTable(type.Name);
            }

            map.HasAlias(aliasFactory.GenerateAlias(type));

            bool isPartial = Attribute.GetCustomAttribute(type, typeof(Join)) == null;
            if (type.BaseType != null && !type.BaseType.Equals(typeof(Object)) && !isPartial)
            {
                IMap joinMap = Map(type.BaseType, aliasFactory, mapProperties);

                Relation relation = map.Relation(type.BaseType);
                relation.JoinWith(joinMap);
                relation.JoinAs(Attribute.GetCustomAttribute(type, typeof(Join)) as Join);
                relation.JoinOn(Attribute.GetCustomAttributes(type, typeof(On)) as On[]);
                relation.Inherits();
            }

            PropertyInfo[] properties = type.GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                if (!properties[i].DeclaringType.Equals(type) && !isPartial) { continue; }
                
                bool isIdentity = Attribute.GetCustomAttribute(properties[i], typeof(Identity)) != null;
                if (!isIdentity && !mapProperties.Any(p => p == properties[i].Name)) { continue; }

                Property property = map.Property(properties[i].Name);
                property.Identity(isIdentity);
                property.ToColumn(Attribute.GetCustomAttribute(properties[i], typeof(Column)) as Column);
            }

            // - Convenções
            Property identity = map.Properties.FirstOrDefault(p => p.IsIdentity);
            if (identity == null) { throw new ArgumentException(String.Format(Identity_Not_Found, type.Name)); }

            for (int i = 0; i < map.Relations.Count; i++)
            {
                bool isInheritance = map.Relations[i].IsInheritance;
                bool isCollection = isInheritance ? false : (map.Relations[i].PropertyInfo.PropertyType.IsGenericType && map.Relations[i].PropertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IList<>)) || typeof(IList).IsAssignableFrom(map.Relations[i].PropertyInfo.PropertyType);

                if (map.Relations[i].On.Length == 0)
                {
                    Property joinIdentity = map.Relations[i].JoinMap.Properties.FirstOrDefault(p => p.IsIdentity);
                    if (joinIdentity == null) { throw new ArgumentException(String.Format(Identity_Not_Found, map.Relations[i].Type.Name)); }

                    On[] on = new On[1];
                    on[0] = new On(isCollection || isInheritance ? identity.ColumnName : joinIdentity.ColumnName, isCollection ? identity.ColumnName : joinIdentity.ColumnName);

                    map.Relations[i].JoinOn(on);
                }

                if (map.Relations[i].PropertyInfo != null && Attribute.GetCustomAttribute(map.Relations[i].PropertyInfo, typeof(Lazy)) == null)
                { map.Relations[i].Lazy(isCollection); }
            }

            return map;
        }

        private IMap Resolve(Type type, IMap map)
        {
            if (map.Parent != null)
            {
                if (map.Parent.Type == type) { return map.Parent; }
                else { return Resolve(type, map.Parent); }
            }

            return null;
        }
    }
}