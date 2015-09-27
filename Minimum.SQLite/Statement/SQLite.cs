using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minimum.DataAccess.Statement
{
    internal class SQLiteStatement : IStatement
    {
        private const string UnableToResolveProperty = "The property {0} was not found in the mapping of the type {1}.";

        public string Select(IMap map, IList<Criteria> criterias)
        {
            StringBuilder select = new StringBuilder();
            select.Append("SELECT ");
            select.Append(SelectColumns(map));
            select.Append(" FROM ");
            select.Append(map.Name);
            select.Append(" AS ");
            select.Append(map.Alias);
            select.Append(SelectTables(map));
            select.Append(SelectWhere(map, criterias));
            select.Append(SelectOrder(map, criterias));
            select.Append(SelectTop(criterias));

            return select.ToString();
        }
        
        private string SelectColumns(IMap map)
        {
            StringBuilder columns = new StringBuilder();

            bool comma = false;
            for (int i = 0; i < map.Properties.Count; i++)
            {
                if (comma) { columns.Append(", "); }
                columns.Append(map.Alias);
                columns.Append(".");
                columns.Append(map.Properties[i].ColumnName);
                columns.Append(" AS ");
                columns.Append(map.Alias);
                columns.Append("_");
                columns.Append(map.Properties[i].ColumnName);

                comma = true;
            }

            for (int i = 0; i < map.Relations.Count; i++)
            {
                if (map.Relations[i].IsCollection || map.Relations[i].IsLazy)
                {
                    for (int j = 0; j < map.Relations[i].On.Length; j++)
                    {
                        if (map.Properties.Any(p => p.ColumnName == map.Relations[i].On[j].ForeignKey)) { continue; }

                        if (comma) { columns.Append(", "); }
                        columns.Append(map.Alias);
                        columns.Append(".");
                        columns.Append(map.Relations[i].On[j].ForeignKey);
                        columns.Append(" AS ");
                        columns.Append(map.Alias);
                        columns.Append("_");
                        columns.Append(map.Relations[i].On[j].ForeignKey);
                        comma = true;
                    }

                    continue;
                }

                if (comma) { columns.Append(", "); }
                columns.Append(SelectColumns(map.Relations[i].JoinMap));
                comma = true;
            }

            return columns.ToString();
        }

        private string SelectTables(IMap map)
        {
            StringBuilder tables = new StringBuilder();

            for (int i = 0; i < map.Relations.Count; i++)
            {
                if (map.Relations[i].IsCollection || map.Relations[i].IsLazy) { continue; }

                switch (map.Relations[i].JoinType)
                {
                    case JoinType.LeftJoin: { tables.Append(" LEFT JOIN "); break; }
                    case JoinType.RightJoin: { tables.Append(" RIGHT JOIN "); break; }
                    case JoinType.InnerJoin: { tables.Append(" INNER JOIN "); break; }
                }

                tables.Append(map.Relations[i].JoinMap.Name);
                tables.Append(" AS ");
                tables.Append(map.Relations[i].JoinMap.Alias);

                for (int j = 0; j < map.Relations[i].On.Length; j++)
                {
                    tables.Append(j == 0 ? " ON " : " AND ");
                    tables.Append(map.Alias);
                    tables.Append(".");
                    tables.Append(map.Relations[i].On[j].ForeignKey);
                    tables.Append(" = ");
                    tables.Append(map.Relations[i].JoinMap.Alias);
                    tables.Append(".");
                    tables.Append(map.Relations[i].On[j].PrimaryKey);
                }

                tables.Append(SelectTables(map.Relations[i].JoinMap));
            }

            return tables.ToString();
        }

        private string SelectTop(IList<Criteria> criterias)
        {
            StringBuilder top = new StringBuilder();
            for (int i = 0; i < criterias.Count; i++)
            {
                if (criterias[i].Type != CriteriaType.Limit) { continue; }

                top.Append(" LIMIT " + (criterias[i] as LimitCriteria).Value.ToString() + " ");
            }

            for (int i = 0; i < criterias.Count; i++)
            {
                if (criterias[i].Type != CriteriaType.Skip) { continue; }

                top.Append(" OFFSET " + (criterias[i] as SkipCriteria).Value.ToString() + " ");
            }

            return top.ToString();
        }

        private string SelectWhere(IMap map, IList<Criteria> criterias, bool useAlias = true)
        {
            StringBuilder queryString = new StringBuilder();

            queryString.Append(EvaluateCriteria(new AllCriteria() { Criterias = criterias.ToArray() }, map, true, useAlias));

            //bool isWhere = true;
            //for (int i = 0; i < criterias.Count; i++)
            //{
            //    if (criterias[i].Type == CriteriaType.Order || criterias[i].Type == CriteriaType.Limit || criterias[i].Type == CriteriaType.Skip) { continue; }

            //    queryString.Append(EvaluateCriteria(criterias[i], map, isWhere, useAlias));
            //    isWhere = false;
            //}

            return queryString.ToString();
        }

        private string SelectOrder(IMap map, IList<Criteria> criterias)
        {
            StringBuilder order = new StringBuilder();

            bool comma = false;
            for (int i = 0; i < criterias.Count; i++)
            {
                if (criterias[i].Type != CriteriaType.Order) { continue; }

                if (comma) { order.Append(", "); } else { order.Append(" ORDER BY "); }
                order.Append(EvaluateCriteria(criterias[i], map, false, true));                
                comma = true;
            }

            return order.ToString();
        }

        public string Update(IMap map, IList<Criteria> criterias, object element)
        {
            StringBuilder update = new StringBuilder();

            update.Append("UPDATE ");
            update.Append(map.Name);
            update.Append(" SET ");

            update.Append(UpdateColumns(map, element));
            update.Append(SelectWhere(map, criterias, false));

            return update.ToString();
        }

        private string UpdateColumns(IMap map, object element)
        {
            StringBuilder columns = new StringBuilder();

            bool comma = false;
            for (int i = 0; i < map.Properties.Count; i++)
            {
                if (map.Properties[i].IsIdentity) { continue; }

                if (comma) { columns.Append(", "); }
                columns.Append(map.Properties[i].ColumnName);
                columns.Append(" = ");
                columns.Append(FormatWriteValue(map.Properties[i].PropertyInfo.GetValue(element), map.Properties[i].Type));
                comma = true;
            }

            return columns.ToString();
        }

        public string Insert(IMap map, object element)
        {
            StringBuilder insert = new StringBuilder();

            insert.Append("INSERT INTO ");

            insert.Append(map.Name);

            insert.Append(" (");
            insert.Append(InsertColumns(map, element));
            insert.Append(") VALUES (");
            insert.Append(InsertValues(map, element));
            insert.Append(") ");

            return insert.ToString();
        }

        private string InsertColumns(IMap map, object element)
        {
            StringBuilder columns = new StringBuilder();

            bool comma = false;
            for (int i = 0; i < map.Properties.Count; i++)
            {
                if (map.Properties[i].IsIdentity && (int)map.Properties[i].PropertyInfo.GetValue(element) == 0) { continue; }

                if (comma) { columns.Append(", "); }
                columns.Append(map.Properties[i].ColumnName);
                comma = true;
            }

            return columns.ToString();
        }

        private string InsertValues(IMap map, object element)
        {
            StringBuilder values = new StringBuilder();

            bool comma = false;
            for (int i = 0; i < map.Properties.Count; i++)
            {
                if (map.Properties[i].IsIdentity && (int)map.Properties[i].PropertyInfo.GetValue(element) == 0) { continue; }

                if (comma) { values.Append(", "); }
                values.Append(FormatWriteValue(map.Properties[i].PropertyInfo.GetValue(element), map.Properties[i].Type));
                comma = true;
            }

            return values.ToString();
        }

        public string Delete(IMap map, IList<Criteria> criterias, object element)
        {
            StringBuilder delete = new StringBuilder();

            delete.Append("DELETE FROM ");
            delete.Append(map.Name);
            delete.Append(SelectWhere(map, criterias, false));

            return delete.ToString();
        }

        public string Count(IMap map, IList<Criteria> criterias)
        {
            StringBuilder count = new StringBuilder();

            count.Append("SELECT COUNT(*) AS Total FROM ");
            count.Append(map.Name);
            count.Append(" AS ");
            count.Append(map.Alias);
            count.Append(SelectTables(map));
            count.Append(SelectWhere(map, criterias));

            return count.ToString();
        }

        public string Create(IMap map)
        {
            StringBuilder create = new StringBuilder();

            create.Append("CREATE TABLE ");
            create.Append(map.Name);
            create.Append(" (");
            create.Append(CreateColumns(map));
            create.Append(")");

            return create.ToString();
        }

        private string CreateColumns(IMap map)
        {
            StringBuilder columns = new StringBuilder();

            bool comma = false;
            for (int i = 0; i < map.Properties.Count; i++)
            {
                if (comma) { columns.Append(", "); }
                columns.Append(map.Properties[i].ColumnName);
                columns.Append(" ");
                columns.Append(GetSQLType(map.Properties[i].Type));
                if (map.Properties[i].IsIdentity)
                { columns.Append(" PRIMARY KEY AUTOINCREMENT"); }
                else
                { columns.Append(" " + GetNullableType(map.Properties[i].Type)); }
                comma = true;
            }

            return columns.ToString();
        }

        public string Drop(IMap map)
        {
            StringBuilder drop = new StringBuilder();

            drop.Append("DROP TABLE ");
            drop.Append(map.Name);

            return drop.ToString();
        }

        public string Exists(IMap map)
        {
            StringBuilder exists = new StringBuilder();

            exists.Append("IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES ");
            //exists.Append(query.Map.Name); TODO!
            //WHERE TABLE_NAME = 'Alunos')  )
            exists.Append("SELECT 1 AS EXIST ELSE SELECT 0 AS EXIST");

            return exists.ToString();
        }

        public string GetInsertedID()
        {
            return "SELECT last_insert_rowid();";
        }

        private string GetSQLType(Type type)
        {
            switch (type.Name)
            {
                case "Int16":
                case "Int32":
                case "Int64":
                case "Boolean":                    
                    return "INTEGER";
                case "Single":                    
                case "Decimal":
                    return "REAL";                
                case "DateTime":                    
                case "String":
                case "Object":
                default:
                    return "TEXT";
            }
        }

        private string GetNullableType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            { return "NULL"; }

            switch (type.Name)
            {
                case "Int16":
                case "Int32":
                case "Int64":
                case "Boolean":
                    return "NOT NULL";
                case "Single":
                case "Decimal":
                    return "NULL";                
                case "DateTime":
                    return "NOT NULL";
                case "String":
                case "Object":
                default:
                    return "NULL";
            }
        }

        public object FormatReadValue(object value, Type valueType)
        {
            //Fix para Nullables
            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (valueType.GetGenericArguments()[0].IsEnum)
                { return Enum.Parse(valueType.GetGenericArguments()[0], value.ToString()); }

                if (valueType.GetGenericArguments()[0].IsValueType)
                { valueType = valueType.GetGenericArguments()[0]; }
            }

            if (valueType.IsEnum)
            {
                return Enum.Parse(valueType, value.ToString());
            }

            switch (valueType.Name)
            {
                case "Guid":
                    {
                        return Guid.Parse(value.ToString());
                    }
                case "Boolean":
                case "bool":                
                case "Int32":
                case "Int64":
                case "int":
                default:
                    {
                        return Convert.ChangeType(value, valueType);
                    }
            }
        }

        public string FormatWriteValue(object value, Type valueType)
        {
            if (value == null) { return "NULL"; }
            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                valueType = valueType.GetGenericArguments()[0];
            }
            
            if (valueType.IsEnum) { return Convert.ToInt32(value).ToString(); }

            switch (valueType.Name)
            {
                case "Byte[]": { return "0x" + BitConverter.ToString(value as byte[]).Replace("-", ""); }
                case "Boolean":
                case "bool": { return Convert.ToBoolean(value) ? "1" : "0"; }
                case "Single":
                case "Decimal": { return value.ToString().Replace(',', '.'); }
                case "Int64[]":
                case "Int32[]":
                case "Int16[]":
                    {
                        string array = null;
                        int[] values = value as int[];
                        for (int i = 0; i < values.Length; i++) { array += array == null ? values[i].ToString() : ", " + values[i].ToString(); }
                        return array;
                    }
                case "Int64":
                case "Int32":
                case "Int16":
                case "int": { return value.ToString(); }
                case "DateTime": { return "'" + Convert.ToDateTime(value).ToString("yyyy-MM-dd HH:mm:ss") + "'"; }
                default: { return "'" + value.ToString().Replace("'", "''") + "'"; }
            }
        }

        private string EvaluateCriteria(Criteria criteria, IMap map, bool firstCriteria, bool useAlias)
        {
            if (criteria == null) { return null; }

            StringBuilder condition = new StringBuilder();            

            switch (criteria.Type)
            {
                case CriteriaType.Value:
                    {
                        if ((criteria as ValueCriteria).UseBrackets) { condition.Append("("); }

                        //if ((criteria as ValueCriteria).Value.GetType().IsArray)
                        //{
                        //    IList list = (criteria as ValueCriteria).Value as IList;
                        //    for (int i = 0; i < list.Count; i++)
                        //    {
                        //        if (i != 0) { condition.Append(", "); }
                        //        condition.Append(FormatWriteValue(list[i], (criteria as ValueCriteria).ValueType.GetElementType()));                                
                        //    }
                        //}
                        //else
                        //{ 
                        //    condition.Append(FormatWriteValue((criteria as ValueCriteria).Value, (criteria as ValueCriteria).ValueType)); 
                        //}
                        condition.Append(FormatWriteValue((criteria as ValueCriteria).Value, (criteria as ValueCriteria).ValueType)); 

                        if ((criteria as ValueCriteria).UseBrackets) { condition.Append(")"); }

                        break;
                    }
                case CriteriaType.Member:
                    {
                        IMap currentMap = map;

                        Property property = null;
                        while ((criteria as MemberCriteria) != null)
                        {
                            property = null;
                            if ((criteria as MemberCriteria).Member == null)
                            {
                                property = currentMap.Properties.FirstOrDefault(c => c.PropertyInfo.Name == (criteria as MemberCriteria).Name);
                                if (property != null) { break; }

                                Relation join = currentMap.Relations.FirstOrDefault(j => j.IsInheritance);
                                if (join == null) { throw new ArgumentException(String.Format(UnableToResolveProperty, (criteria as MemberCriteria).Name, currentMap.Type.Name)); }
                                currentMap = join.JoinMap;
                                continue;
                            }
                            else
                            {
                                Relation join = currentMap.Relations.FirstOrDefault(j => j.IsInheritance == false && j.PropertyInfo.Name == (criteria as MemberCriteria).Name);
                                if (join != null) { currentMap = join.JoinMap; criteria = (criteria as MemberCriteria).Member; continue; }

                                join = currentMap.Relations.FirstOrDefault(j => j.IsInheritance);
                                if (join == null) { throw new ArgumentException(String.Format(UnableToResolveProperty, (criteria as MemberCriteria).Name, currentMap.Type.Name)); }

                                currentMap = join.JoinMap;
                            }

                            //criteria = (criteria as MemberCriteria).Member;
                        }

                        //if (property.IsAggregate) { condition.Append(property.Query.Alias + "_" + property.Name); }
                        ////condition.Append(currentTable.Alias + "." + property.Name);
                        //else { condition.Append(property.Query.Alias + "." + property.Name); }
                        condition.Append(useAlias ? currentMap.Alias + "." + property.ColumnName : property.ColumnName);
                        break;
                    }
                case CriteriaType.Binary:
                    {
                        if (firstCriteria) { condition.Append(" WHERE "); firstCriteria = false; }

                        if ((criteria as BinaryCriteria).UseBrackets) { condition.Append("("); }

                        condition.Append(EvaluateCriteria((criteria as BinaryCriteria).LeftValue, map, firstCriteria, useAlias));                        
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
                            case BinaryOperand.Like: { condition.Append(" LIKE "); break; }
                            case BinaryOperand.Is: { condition.Append(" IS "); break; }
                        }
                        condition.Append(EvaluateCriteria((criteria as BinaryCriteria).RightValue, map, firstCriteria, useAlias));

                        if ((criteria as BinaryCriteria).UseBrackets) { condition.Append(")"); }
                        break;
                    }
                case CriteriaType.Any:
                    {
                        if ((criteria as AnyCriteria).Criterias == null || (criteria as AnyCriteria).Criterias.Length == 0) { break; }
                        if ((criteria as AnyCriteria).UseBrackets) { condition.Append("("); }

                        for (int i = 0; i < (criteria as AnyCriteria).Criterias.Length; i++)
                        {
                            if ((criteria as AnyCriteria).Criterias[i].Type == CriteriaType.Order || (criteria as AnyCriteria).Criterias[i].Type == CriteriaType.Limit || (criteria as AnyCriteria).Criterias[i].Type == CriteriaType.Skip) { continue; }
                            if (firstCriteria) { condition.Append(" WHERE "); firstCriteria = false; }
                            else if (i > 0) { condition.Append(" OR "); }

                            condition.Append(EvaluateCriteria((criteria as AnyCriteria).Criterias[i], map, firstCriteria, useAlias));
                        }

                        if ((criteria as AnyCriteria).UseBrackets) { condition.Append(")"); }
                        break;
                    }
                case CriteriaType.All:
                    {
                        if ((criteria as AllCriteria).Criterias == null || (criteria as AllCriteria).Criterias.Length == 0) { break; }
                        if ((criteria as AllCriteria).UseBrackets) { condition.Append("("); }

                        for (int i = 0; i < (criteria as AllCriteria).Criterias.Length; i++)
                        {
                            if ((criteria as AllCriteria).Criterias[i].Type == CriteriaType.Order || (criteria as AllCriteria).Criterias[i].Type == CriteriaType.Limit || (criteria as AllCriteria).Criterias[i].Type == CriteriaType.Skip) { continue; }
                            if (firstCriteria) { condition.Append(" WHERE "); firstCriteria = false; }
                            else if (i > 0) { condition.Append(" AND "); }

                            condition.Append(EvaluateCriteria((criteria as AllCriteria).Criterias[i], map, firstCriteria, useAlias));
                        }

                        if ((criteria as AllCriteria).UseBrackets) { condition.Append(")"); }
                        break;
                    }
                case CriteriaType.Order:
                    {
                        condition.Append(EvaluateCriteria((criteria as OrderCriteria).Member, map, false, true));
                        condition.Append(" COLLATE NOCASE ");
                        condition.Append((criteria as OrderCriteria).Ascending ? "ASC" : "DESC");
                        break;
                    }
                case CriteriaType.Limit:
                    {
                        break;
                    }
                case CriteriaType.Skip:
                    {
                        break;
                    }
            }

            return condition.ToString();
        }
    }
}