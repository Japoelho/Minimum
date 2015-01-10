using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minimum.DataAccess.V08.Mapping;

namespace Minimum.DataAccess.V08.Statement
{
    internal class SQLStatement : IStatement
    {
        private Mappings _maps = null;
        private IList<Criteria> _criterias = null;
        private AliasMap _aliasMap = null;

        public SQLStatement(Mappings mapper)
        {
            _maps = mapper;
            _criterias = new List<Criteria>();
        }

        public string Select(Type type)
        {
            StringBuilder query = new StringBuilder();

            ClassMap classMap = _maps.Map(type);
            _aliasMap = new AliasMap(new AliasGenerator("T"), classMap, null);

            string where = SelectWhere(_aliasMap);
            string tables = SelectTables(_aliasMap);
            string columns = SelectColumns(_aliasMap);

            query.Append("SELECT ");
            query.Append(columns);
            query.Append(" FROM ");
            query.Append(tables);
            query.Append(where);

            return query.ToString();
        }

        public string Update(object element, Type type)
        {
            StringBuilder query = new StringBuilder();

            ClassMap map = _maps.Map(type);
            PropertyMap identity = map.Properties.FirstOrDefault(p => p.Identity != null);

            StringBuilder queryBuilder = new StringBuilder();

            query.Append("UPDATE ");
            query.Append(map.Table != null ? map.Table.Name : map.Type.Name);
            query.Append(" SET ");
            query.Append(UpdateColumnValues(map, element));
            query.Append(" WHERE ");
            query.Append(identity.Column != null ? identity.Column.Name : identity.Property.Name);
            query.Append(" = ");
            query.Append(FormatWriteValue(identity.Property.GetValue(element, null), identity.Property.PropertyType));

            return query.ToString();
        }

        public string Insert(object element, Type type)
        {
            StringBuilder query = new StringBuilder();

            ClassMap classMap = _maps.Map(type);
            PropertyMap identity = classMap.Properties.FirstOrDefault(p => p.Identity != null);

            query.Append("INSERT INTO ");
            query.Append(classMap.Table != null ? classMap.Table.Name : classMap.Type.Name);
            query.Append(" (");
            query.Append(InsertColumns(classMap, element));
            query.Append(") VALUES (");
            query.Append(InsertValues(classMap, element));
            query.Append(") ");
            query.Append("SELECT @@IDENTITY AS ID");

            return query.ToString();
        }

        public string Delete(object element, Type type)
        {
            StringBuilder query = new StringBuilder();

            ClassMap map = _maps.Map(type);
            PropertyMap identity = map.Properties.FirstOrDefault(p => p.Identity != null);

            query.Append("DELETE FROM ");
            query.Append(map.Table != null ? map.Table.Name : map.Type.Name);
            query.Append(" WHERE ");
            query.Append(identity.Column != null ? identity.Column.Name : identity.Property.Name);
            query.Append(" = ");
            query.Append(FormatWriteValue(identity.Property.GetValue(element, null), identity.Property.PropertyType));

            return query.ToString();
        }

        private string SelectColumns(AliasMap aliasMap, ClassMap classMap = null, ClassMap parentMap = null, PropertyMap propertyMap = null)
        {
            StringBuilder columns = new StringBuilder();

            if (classMap == null) { classMap = aliasMap.ClassMap; }

            bool comma = false;
            for (int i = 0; i < classMap.Properties.Count; i++)
            {
                if (classMap.Properties[i].Ignore != null) { continue; }

                if (classMap.Properties[i].IsClass == false && classMap.Properties[i].IsCollection == false)
                {
                    if (comma == true) { columns.Append(", "); }

                    columns.Append(aliasMap.Alias);
                    columns.Append(".");
                    columns.Append(classMap.Properties[i].Column != null ? classMap.Properties[i].Column.Name : classMap.Properties[i].Property.Name);
                    columns.Append(" AS ");
                    columns.Append(aliasMap.Alias);
                    columns.Append(classMap.Properties[i].Column != null ? classMap.Properties[i].Column.Name : classMap.Properties[i].Property.Name);

                    comma = true;
                }
                else
                {
                    Join[] joins = classMap.Properties[i].Joins;
                    for (int j = 0; j < joins.Length; j++)
                    {
                        string joinReference = joins[j].ForeignKey;
                        if (String.IsNullOrEmpty(joinReference))
                        {
                            if (classMap.Properties[i].IsCollection == true) { continue; }

                            Type type = classMap.Properties[i].Property.PropertyType;
                            PropertyMap identity = _maps.Map(type).Properties.First(p => p.Identity != null);
                            joinReference = identity.Column != null ? identity.Column.Name : identity.Property.Name;
                        }

                        if (columns.ToString().Contains(aliasMap.Alias + joinReference)) { continue; }

                        if (!classMap.Properties.Any(p => p.Property.Name == joinReference || (p.Column != null && p.Column.Name == joinReference))) 
                        {
                            if (comma == true) { columns.Append(", "); }
                            
                            columns.Append(aliasMap.Alias);
                            columns.Append(".");
                            columns.Append(joinReference);
                            columns.Append(" AS ");
                            columns.Append(aliasMap.Alias);
                            columns.Append(joinReference);
                            
                            comma = true;
                        }
                    }

                    AliasMap joinAlias = aliasMap.Aliases.FirstOrDefault(a => a.PropertyMap == classMap.Properties[i]);
                    if (joinAlias != null)
                    {
                        if (comma == true) { columns.Append(", "); }

                        columns.Append(SelectColumns(joinAlias, _maps.Map(classMap.Properties[i].Property.PropertyType), classMap, classMap.Properties[i]));

                        comma = true;
                    }
                }
            }

            if (classMap.BaseClass != null)
            {
                AliasMap joinAlias = aliasMap.Aliases.FirstOrDefault(a => a.ClassMap == classMap.BaseClass && a.PropertyMap == null);
                if (joinAlias != null)
                {
                    if (comma == true) { columns.Append(", "); }
                    columns.Append(SelectColumns(joinAlias, classMap.BaseClass, classMap));
                }
            }

            return columns.ToString();
        }

        private string SelectTables(AliasMap aliasMap, ClassMap classMap = null, ClassMap parentMap = null, PropertyMap propertyMap = null)
        {
            StringBuilder tables = new StringBuilder();

            if (classMap == null)
            {
                classMap = aliasMap.ClassMap;
                tables.Append(aliasMap.ClassMap.Table != null ? aliasMap.ClassMap.Table.Name : aliasMap.ClassMap.Type.Name);
                tables.Append(" AS ");
                tables.Append(aliasMap.Alias);
            }

            if (classMap.BaseClass != null)
            {
                ClassMap joinMap = classMap.BaseClass;
                Join[] joins = classMap.Joins;

                if (joins.Length > 0)
                {
                    tables.Append(" INNER JOIN ");

                    AliasMap joinAlias = aliasMap.Aliases.FirstOrDefault(a => a.ClassMap == classMap.BaseClass && a.PropertyMap == null) ?? aliasMap.MapAlias(classMap.BaseClass, null);

                    Table table = joinMap.Table;

                    tables.Append(table != null ? table.Name : joinMap.Type.Name);
                    tables.Append(" AS ");
                    tables.Append(joinAlias.Alias);

                    PropertyMap thisIdentity = classMap.Properties.FirstOrDefault(p => p.Identity != null);
                    PropertyMap joinIdentity = joinMap.Properties.FirstOrDefault(p => p.Identity != null);
                    for (int j = 0; j < joins.Length; j++)
                    {
                        tables.Append(j == 0 ? " ON " : " AND ");
                        tables.Append(joinAlias.Alias);
                        tables.Append(".");

                        tables.Append(!String.IsNullOrEmpty(joins[j].PrimaryKey) ? joins[j].PrimaryKey : joinIdentity.Column != null ? joinIdentity.Column.Name : joinIdentity.Property.Name);

                        tables.Append(" = ");
                        tables.Append(aliasMap.Alias);
                        tables.Append(".");

                        tables.Append(!String.IsNullOrEmpty(joins[j].ForeignKey) ? joins[j].ForeignKey : thisIdentity.Column != null ? thisIdentity.Column.Name : thisIdentity.Property.Name);
                    }

                    tables.Append(SelectTables(joinAlias, classMap.BaseClass, classMap));
                }
            }

            for (int i = 0; i < classMap.Properties.Count; i++)
            {
                if (classMap.Properties[i].Ignore != null || classMap.Properties[i].IsClass == false || classMap.Properties[i].IsCollection == true) { continue; }

                ClassMap joinMap = _maps.Map(classMap.Properties[i].Property.PropertyType);
                Join[] joins = classMap.Properties[i].Joins;

                if (joins.Length == 0) { continue; }

                AliasMap joinAlias = null;
                switch (joins.Last().JoinType)
                {
                    case JoinType.InnerJoin:
                        {
                            joinAlias = aliasMap.Aliases.FirstOrDefault(a => a.PropertyMap == classMap.Properties[i]) ?? aliasMap.MapAlias(joinMap, classMap.Properties[i]);
                            tables.Append(" INNER JOIN ");
                            break;
                        }
                    case JoinType.LeftJoin:
                        {
                            joinAlias = aliasMap.Aliases.FirstOrDefault(a => a.PropertyMap == classMap.Properties[i]) ?? aliasMap.MapAlias(joinMap, classMap.Properties[i]);
                            tables.Append(" LEFT JOIN ");
                            break;
                        }
                    case JoinType.LazyJoin:
                        {
                            joinAlias = aliasMap.Aliases.FirstOrDefault(a => a.PropertyMap == classMap.Properties[i]);
                            if (joinAlias == null) { return tables.ToString(); }
                            tables.Append(" LEFT JOIN ");
                            break;
                        }
                }

                Table table = classMap.Properties[i].Table ?? joinMap.Table;

                tables.Append(table != null ? table.Name : joinMap.Type.Name);
                tables.Append(" AS ");
                tables.Append(joinAlias.Alias);

                PropertyMap thisIdentity = classMap.Properties.FirstOrDefault(p => p.Identity != null);
                PropertyMap joinIdentity = joinMap.Properties.FirstOrDefault(p => p.Identity != null);
                for (int j = 0; j < joins.Length; j++)
                {
                    if (j == 0) { tables.Append(" ON "); }
                    else { tables.Append(" AND "); }

                    tables.Append(joinAlias.Alias);
                    tables.Append(".");

                    tables.Append(!String.IsNullOrEmpty(joins[j].PrimaryKey) ? joins[j].PrimaryKey : joinIdentity.Column != null ? joinIdentity.Column.Name : joinIdentity.Property.Name);

                    tables.Append(" = ");
                    tables.Append(aliasMap.Alias);
                    tables.Append(".");

                    //Quando é classe dentro de classe, se não for especificado o ID assumir o mesmo Identity da subclasse.
                    //tables.Append(!String.IsNullOrEmpty(joins[j].ForeignKey) ? joins[j].ForeignKey : thisIdentity.Column != null ? thisIdentity.Column.Name : thisIdentity.Property.Name);
                    tables.Append(!String.IsNullOrEmpty(joins[j].ForeignKey) ? joins[j].ForeignKey : joinIdentity.Column != null ? joinIdentity.Column.Name : joinIdentity.Property.Name);
                }

                tables.Append(SelectTables(joinAlias, joinMap, classMap, classMap.Properties[i]));
            }

            return tables.ToString();
        }

        private string SelectWhere(AliasMap aliasMap)
        {
            StringBuilder where = new StringBuilder();
            where.Append(" WHERE 1 = 1");

            for (int i = 0; i < _criterias.Count; i++)
            {
                where.Append(EvaluateCriteria(_criterias[i], aliasMap));
            }

            return where.ToString();
        }

        private string EvaluateCriteria(Criteria criteria, AliasMap aliasMap)
        {
            if (criteria == null) { return null; }

            StringBuilder condition = new StringBuilder();

            switch (criteria.Type)
            {
                case CriteriaType.Value:
                    {
                        if ((criteria as ValueCriteria).UseBrackets) { condition.Append("("); }

                        condition.Append(FormatWriteValue((criteria as ValueCriteria).Value, (criteria as ValueCriteria).ValueType));

                        if ((criteria as ValueCriteria).UseBrackets) { condition.Append(")"); }

                        break;
                    }
                case CriteriaType.Member:
                    {
                        ClassMap currentMap = aliasMap.ClassMap;
                        AliasMap currentAlias = aliasMap;

                        while ((criteria as MemberCriteria).Member != null)
                        {
                            PropertyMap propertyMap = null;
                            while (propertyMap == null)
                            {
                                propertyMap = currentMap.Properties.FirstOrDefault(p => p.Property.Name == (criteria as MemberCriteria).Name);                                
                                if ((propertyMap == null && currentMap.BaseClass == null) || (propertyMap == null && currentMap.BaseClass != null && (currentMap.Type.GetCustomAttributes(typeof(Join), false) as Join[]).Length == 0) || (propertyMap != null && propertyMap.Joins.Length == 0))
                                {
                                    throw new ArgumentException(String.Format(MappingErrors.CouldNotResolveProperty, (criteria as MemberCriteria).Name, currentMap.Type.Name));
                                }
                                else if (propertyMap == null && currentMap.BaseClass != null)
                                {
                                    currentMap = currentMap.BaseClass;
                                    currentAlias = currentAlias.Aliases.FirstOrDefault(a => a.ClassMap == currentMap && a.PropertyMap == null) ?? currentAlias.MapAlias(currentMap, null);
                                }
                                else
                                {
                                    currentMap = _maps.Map(propertyMap.Property.PropertyType);
                                    currentAlias = currentAlias.Aliases.FirstOrDefault(a => a.ClassMap == currentMap && a.PropertyMap == propertyMap) ?? currentAlias.MapAlias(currentMap, propertyMap);
                                }
                            }

                            criteria = (criteria as MemberCriteria).Member;
                        }

                        PropertyMap currentProperty = null;
                        while (currentProperty == null)
                        {
                            currentProperty = currentMap.Properties.FirstOrDefault(p => p.Property.Name == (criteria as MemberCriteria).Name);
                            if (currentProperty == null && currentMap.BaseClass == null)
                            {
                                //TODO: Mantenho isto para Lazy Loading?
                                //Caso único de houver um JOIN em de uma classeA para classeB onde a classeB não possui a propriedade explícita na classe, mas possui no banco.
                                condition.Append(currentAlias.Alias + "." + (criteria as MemberCriteria).Name);
                                return condition.ToString();
                                //throw new ArgumentException(String.Format(MappingErrors.CouldNotResolveProperty, (criteria as MemberCriteria).Name, currentMap.Type.Name));
                            }
                            else if (currentProperty == null && currentMap.BaseClass != null)
                            {
                                currentMap = currentMap.BaseClass;
                                currentAlias = currentAlias.Aliases.FirstOrDefault(a => a.ClassMap == currentMap && a.PropertyMap == null) ?? currentAlias.MapAlias(currentMap, null);
                            }
                        }

                        condition.Append(currentAlias.Alias + "." + (currentProperty.Column != null ? currentProperty.Column.Name : currentProperty.Property.Name));
                        break;
                    }
                case CriteriaType.Binary:
                    {
                        if ((criteria as BinaryCriteria).UseBrackets) { condition.Append("("); }

                        condition.Append(EvaluateCriteria((criteria as BinaryCriteria).LeftValue, aliasMap));
                        switch ((criteria as BinaryCriteria).Operand)
                        {
                            case BinaryOperand.Equal: { condition.Append(" = "); break; }
                            case BinaryOperand.NotEqual: { condition.Append(" <> "); break; }
                            case BinaryOperand.GreaterThan: { condition.Append(" > "); break; }
                            case BinaryOperand.GreaterEqualThan: { condition.Append(" >= "); break; }
                            case BinaryOperand.LowerThan: { condition.Append(" < "); break; }
                            case BinaryOperand.LowerEqualThan: { condition.Append(" <= "); break; }
                            case BinaryOperand.And: { condition.Append(" AND "); break; }
                            case BinaryOperand.Or: { condition.Append(" OR "); break; }
                            case BinaryOperand.In: { condition.Append(" IN "); break; }
                            case BinaryOperand.Between: { condition.Append(" BETWEEN "); break; }
                        }
                        condition.Append(EvaluateCriteria((criteria as BinaryCriteria).RightValue, aliasMap));

                        if ((criteria as BinaryCriteria).UseBrackets) { condition.Append(")"); }
                        break;
                    }
            }

            return condition.ToString();
        }

        private string UpdateColumnValues(ClassMap classMap, object element)
        {
            StringBuilder columns = new StringBuilder();

            bool comma = false;
            for (int i = 0; i < classMap.Properties.Count; i++)
            {
                if (classMap.Properties[i].Identity != null || classMap.Properties[i].Ignore != null || classMap.Properties[i].IsClass == true || classMap.Properties[i].IsCollection == true) { continue; }

                object value = classMap.Type.GetProperty(classMap.Properties[i].Property.Name).GetValue(element, null);

                if (comma == true) { columns.Append(", "); }
                columns.Append(classMap.Properties[i].Column != null ? classMap.Properties[i].Column.Name : classMap.Properties[i].Property.Name);
                columns.Append(" = ");
                columns.Append(FormatWriteValue(value, classMap.Properties[i].Property.PropertyType));
                comma = true;
            }

            return columns.ToString();
        }

        private string InsertColumns(ClassMap classMap, object element)
        {
            StringBuilder columns = new StringBuilder();

            bool comma = false;
            for (int i = 0; i < classMap.Properties.Count; i++)
            {
                if (classMap.Properties[i].Identity != null || classMap.Properties[i].Ignore != null || classMap.Properties[i].IsClass == true || classMap.Properties[i].IsCollection == true) { continue; }

                if (comma == true) { columns.Append(", "); }
                columns.Append(classMap.Properties[i].Column != null ? classMap.Properties[i].Column.Name : classMap.Properties[i].Property.Name);
                comma = true;
            }

            return columns.ToString();
        }

        private string InsertValues(ClassMap classMap, object element)
        {
            StringBuilder values = new StringBuilder();

            bool comma = false;
            for (int i = 0; i < classMap.Properties.Count; i++)
            {
                if (classMap.Properties[i].Identity != null || classMap.Properties[i].Ignore != null || classMap.Properties[i].IsClass == true || classMap.Properties[i].IsCollection == true) { continue; }

                if (comma == true) { values.Append(", "); }
                values.Append(FormatWriteValue(classMap.Properties[i].Property.GetValue(element, null), classMap.Properties[i].Property.PropertyType));
                comma = true;
            }

            return values.ToString();
        }

        private string FormatWriteValue(object value, Type valueType)
        {
            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                valueType = valueType.GetGenericArguments()[0];
            }

            if (value == null) { return "NULL"; }
            if (valueType.IsEnum) { return Convert.ToInt32(value).ToString(); }

            switch (valueType.Name)
            {
                case "Boolean":
                case "bool": { return Convert.ToBoolean(value) ? "1" : "0"; }
                case "Single":
                case "Decimal": { return value.ToString().Replace(',', '.'); }
                case "Int64":
                case "Int32":
                case "Int16":
                case "int": { return value.ToString(); }
                case "DateTime": { return "CONVERT(DATETIME, '" + Convert.ToDateTime(value).ToString("dd/MM/yyyy HH:mm:ss") + "', 103)"; }
                default: { return "'" + value.ToString().Replace("'", "''") + "'"; }
            }
        }

        public IStatement AddCriteria(params Criteria[] criterias)
        {
            for (int i = 0; i < criterias.Length; i++)
            {
                _criterias.Add(criterias[i]);
            }

            return this;
        }

        public AliasMap AliasMap()
        {
            return _aliasMap;
        }
    }
}
