using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Minimum.DataAccess
{
    internal class AutoMapper : IMapper
    {
        private const string Identity_Not_Found = "No [Identity] property found on class {0}.";
        private const string Command_Text_Null = "The command text is null.";

        /// <summary>
        /// The only method an AutoMapper must implement.
        /// This should return an IMap representing the class' database mappings.
        /// </summary>
        public IMap Map(Type type)
        {
            // - MapCache is for saving mappings so they don't go through this process more than once, and only 
            // happens the first time it is called. Further calls in this function with the same type will return
            // the map saved in the MapCache for faster requests.
            IMap map = MapCache.GetInstance().Load(type);
            if (map != null) { return map; }

            // - This is the function you should change should you implement your own.
            map = Map(type, new AliasFactory());

            // - Saving to the MapCache.
            MapCache.GetInstance().Save(map);

            return map;
        }

        private IMap Map(Type type, AliasFactory aliasFactory, IMap parent = null)
        {
            // - Instancing the new Map<Type>.
            IMap map = (IMap)Activator.CreateInstance(typeof(Map<>).MakeGenericType(type));
            map.Parent = parent;

            // - [Command] is an attribute that will override the auto-generated query, if present.
            Command command = Attribute.GetCustomAttribute(type, typeof(Command)) as Command;
            if (command != null) 
            {
                if (String.IsNullOrEmpty(command.Text)) { throw new ArgumentException(Command_Text_Null); }
                map.Command(command.Text); 
            }

            // - [Table] is an attribute that sets the Table, Schema and Database.
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

            // - Sets the Alias of this table-class.
            map.HasAlias(aliasFactory.GenerateAlias(type));

            // - Checks it's inherited classes and includes them in the mapping.
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

            // - Adds the properties as columns or joins if they're classes.
            PropertyInfo[] properties = type.GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                if (!properties[i].DeclaringType.Equals(type) && !isPartial) { continue; }
                if (Attribute.GetCustomAttribute(properties[i], typeof(Ignore)) != null) { continue; }

                // - If it's a class or collection, configure the join.
                if (properties[i].PropertyType.IsClass && !properties[i].PropertyType.Equals(typeof(System.String)) && !properties[i].PropertyType.Equals(typeof(System.Object)) || properties[i].PropertyType.IsGenericType && properties[i].PropertyType.GetGenericTypeDefinition() == typeof(IList<>) || typeof(IList).IsAssignableFrom(properties[i].PropertyType))
                {
                    Relation relation = map.Relation(properties[i].Name);
                    relation.Property(properties[i]);

                    IMap joinMap = Resolve(relation.Type, map) ?? Map(relation.Type, aliasFactory, map);
                    
                    relation.JoinWith(joinMap);
                    relation.JoinAs(Attribute.GetCustomAttribute(properties[i], typeof(Join)) as Join);
                    relation.JoinOn(Attribute.GetCustomAttributes(properties[i], typeof(On)) as On[]);
                    relation.Lazy(Attribute.GetCustomAttribute(properties[i], typeof(Lazy)) as Lazy);
                    
                    Table joinTable = Attribute.GetCustomAttribute(properties[i], typeof(Table)) as Table;
                    if (joinTable != null)
                    {
                        joinMap.ToTable(joinTable.Name);
                        joinMap.ToSchema(joinTable.Schema);
                        joinMap.ToDatabase(joinTable.Database);
                    }
                    
                    continue;
                }
                
                // - If it's not a class or collection, configure the column.
                Property property = map.Property(properties[i].Name);
                property.Identity(Attribute.GetCustomAttribute(properties[i], typeof(Identity)) != null);
                property.ToColumn(Attribute.GetCustomAttribute(properties[i], typeof(Column)) as Column);
            }

            // - Conventions
            Property identity = map.Properties.FirstOrDefault(p => p.IsIdentity);
            if (identity == null && command == null) { throw new ArgumentException(String.Format(Identity_Not_Found, type.Name)); }

            // - Relations represent the joins this class has.
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