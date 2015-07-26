﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minimum.DataAccess.Statement
{
    internal class SQL2012Statement : IStatement
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
            //select.Append(SelectTop(criterias)); //SQL 2012 ou maior

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
                if (map.Relations[i].IsLazy)
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
                if (map.Relations[i].IsLazy) { continue; }

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

            Criteria offset = criterias.FirstOrDefault(c => c.Type == CriteriaType.Skip);
            Criteria rows = criterias.FirstOrDefault(c => c.Type == CriteriaType.Limit);
            Criteria order = criterias.FirstOrDefault(c => c.Type == CriteriaType.Order);

            if (rows != null)
            {                
                top.Append(" OFFSET " + (offset != null ? (offset as SkipCriteria).Value.ToString() : "0") + " ROWS");
                top.Append(" FETCH NEXT " + (rows as LimitCriteria).Value.ToString() + " ROWS ONLY");
            }

            //for (int i = 0; i < criterias.Count; i++)
            //{
            //    if (criterias[i].Type != CriteriaType.Limit) { continue; }

            //    top.Append("TOP " + (criterias[i] as LimitCriteria).Value.ToString() + " ");
            //}

            return top.ToString();
        }

        private string SelectWhere(IMap map, IList<Criteria> criterias, bool useAlias = true)
        {
            StringBuilder queryString = new StringBuilder();

            bool isWhere = true;
            for (int i = 0; i < criterias.Count; i++)
            {
                if (criterias[i].Type == CriteriaType.Order || criterias[i].Type == CriteriaType.Limit) { continue; }

                queryString.Append(EvaluateCriteria(criterias[i], map, isWhere, useAlias));
                isWhere = false;
            }

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

            Criteria offset = criterias.FirstOrDefault(c => c.Type == CriteriaType.Skip);
            Criteria rows = criterias.FirstOrDefault(c => c.Type == CriteriaType.Limit);            

            if (rows != null)
            {
                Property identity = map.Properties.FirstOrDefault(p => p.IsIdentity);
                if (comma == false) { order.Append(" ORDER BY " + (identity != null ? map.Alias + "." + identity.ColumnName : "1")); }
                
                order.Append(" OFFSET " + (offset != null ? (offset as SkipCriteria).Value.ToString() : "0") + " ROWS");
                order.Append(" FETCH NEXT " + (rows as LimitCriteria).Value.ToString() + " ROWS ONLY");
            }

            return order.ToString();
        }
        
        public string Update(IMap map, IList<Criteria> criterias, object element)
        {
            StringBuilder update = new StringBuilder();

            if (map.Properties.Count(p => p.IsIdentity == false) > 0)
            {
                update.Append("UPDATE ");
                update.Append(map.Alias);
                update.Append(" SET ");

                update.Append(UpdateColumns(map, element));

                update.Append(" FROM ");
                update.Append(map.Name);
                update.Append(" AS ");
                update.Append(map.Alias);
                update.Append(SelectTables(map));
                update.Append(SelectWhere(map, criterias));
            }

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

            delete.Append("DELETE ");
            delete.Append(map.Alias);
            delete.Append(" FROM ");
            delete.Append(map.Name);
            delete.Append(" AS ");
            delete.Append(map.Alias);
            delete.Append(SelectWhere(map, criterias));

            return delete.ToString();
        }

        public string Count(IMap map, IList<Criteria> criterias)
        {
            StringBuilder count = new StringBuilder();

            count.Append("SELECT COUNT(*) FROM ");
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
                columns.Append(GetTypeSize(map.Properties[i].Type));
                if (map.Properties[i].IsIdentity)
                { columns.Append(" IDENTITY PRIMARY KEY"); }
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
            return "SELECT @@IDENTITY AS ID";
        }

        private string GetSQLType(Type type)
        {
            switch (type.Name)
            {
                case "Int16":
                case "Int32":
                case "Int64":
                    return "INT";
                case "Single":
                    return "FLOAT";
                case "Decimal":
                    return "DECIMAL";
                case "Boolean":
                    return "BIT";
                case "DateTime":
                    return "SMALLDATETIME";
                case "String":
                case "Object":
                default:
                    return "NVARCHAR";
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
                    return "NOT NULL";
                case "Single":
                    return "NULL";
                case "Decimal":
                    return "NULL";
                case "Boolean":
                    return "NOT NULL";
                case "DateTime":
                    return "NOT NULL";
                case "String":
                case "Object":
                default:
                    return "NULL";
            }        
        }

        private string GetTypeSize(Type type)
        {
            switch (type.Name)
            {
                case "String":
                    return "(200)";
                default: 
                    return null;
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
                case "Int64":
                case "Int32":
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

        private string EvaluateCriteria(Criteria criteria, IMap map, bool firstCriteria, bool useAlias)
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
                        }

                        //if (property.IsAggregate) { condition.Append(property.Query.Alias + "_" + property.Name); }
                        ////condition.Append(currentTable.Alias + "." + property.Name);
                        //else { condition.Append(property.Query.Alias + "." + property.Name); }
                        condition.Append(currentMap.Alias + "." + property.ColumnName);
                        break;
                    }
                case CriteriaType.Binary:
                    {
                        if ((criteria as BinaryCriteria).UseBrackets) { condition.Append("("); }

                        condition.Append(EvaluateCriteria((criteria as BinaryCriteria).LeftValue, map, firstCriteria, useAlias));
                        if (firstCriteria) { condition.Append(" WHERE "); firstCriteria = false; }
                        else
                        {
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
                            }
                        }
                        condition.Append(EvaluateCriteria((criteria as BinaryCriteria).RightValue, map, firstCriteria, useAlias));

                        if ((criteria as BinaryCriteria).UseBrackets) { condition.Append(")"); }
                        break;
                    }
                case CriteriaType.Order:
                    {
                        condition.Append(EvaluateCriteria((criteria as OrderCriteria).Member, map, false, true));
                        condition.Append((criteria as OrderCriteria).Ascending ? " ASC" : " DESC");
                        break;
                    }
                case CriteriaType.Limit:
                    {
                        break;
                    }
            }

            return condition.ToString();
        }
    }
}