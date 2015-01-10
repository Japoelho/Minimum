using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Minimum.DataAccess.V08.Mapping
{
    internal class MappingErrors
    {
        public static string InvalidType { get { return "O tipo [{0}] é inválido para mapeamento."; } private set { } }
        public static string InvalidType_NoColumnID { get { return "O tipo [{0}] não possui uma coluna marcada como ID."; } private set { } }
        public static string CouldNotResolveProperty { get { return "Não foi possível encontrar a propriedade [{0}] no mapeamento da classe [{1}]."; } private set { } }
    }

    internal class Mappings
    {
        private IList<ClassMap> _maps;

        public Mappings()
        {
            _maps = new List<ClassMap>();
        }

        public ClassMap Map(Type type)
        {
            if (type.IsValueType || type.IsGenericType || type.Equals(typeof(System.String)) || type.Equals(typeof(System.Object)) || !type.IsClass)
            { throw new ArgumentException(String.Format(MappingErrors.InvalidType, type.Name)); }

            ClassMap map = _maps.FirstOrDefault(m => m.Type.Equals(type));
            if (map != null) { return map; }

            map = new ClassMap();
            map.Type = type;
            map.Table = Attribute.GetCustomAttribute(type, typeof(Table)) as Table;
            map.Joins = Attribute.GetCustomAttributes(type, typeof(Join)) as Join[];

            if (type.BaseType != null && !type.BaseType.Equals(typeof(System.Object)))
            {
                map.BaseClass = Map(type.BaseType);
            }

            PropertyInfo[] properties = type.GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                if (!properties[i].DeclaringType.Equals(type)) { continue; }

                PropertyMap property = new PropertyMap();
                property.Property = properties[i];
                property.Table = Attribute.GetCustomAttribute(properties[i], typeof(Table)) as Table;
                property.Column = Attribute.GetCustomAttribute(properties[i], typeof(Column)) as Column;
                property.Ignore = Attribute.GetCustomAttribute(properties[i], typeof(Ignore)) as Ignore;
                property.Identity = Attribute.GetCustomAttribute(properties[i], typeof(Identity)) as Identity;
                property.Joins = Attribute.GetCustomAttributes(properties[i], typeof(Join)) as Join[];

                if (properties[i].PropertyType.IsGenericType && properties[i].PropertyType.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    property.IsCollection = true;
                }
                else if (properties[i].PropertyType.IsClass && !properties[i].PropertyType.Equals(typeof(System.String)) && !properties[i].PropertyType.Equals(typeof(System.Object)) && !properties[i].PropertyType.Equals(map.Type))
                {
                    property.IsClass = true;
                    Map(properties[i].PropertyType);
                }

                map.Properties.Add(property);
            }

            if (map.Properties.FirstOrDefault(p => p.Identity != null) == null) { throw new ArgumentException(String.Format(MappingErrors.InvalidType_NoColumnID, type.Name)); }
            if (!_maps.Contains(map)) { _maps.Add(map); }
            return map;
        }
    }    
}
