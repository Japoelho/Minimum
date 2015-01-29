using System;
using System.Linq;
using System.Text;
using Minimum.DataAccess.Mapping;

namespace Minimum.DataAccess.Statement
{
    internal class SQLStatement : IStatement
    {
        private const string UnableToResolveProperty = "The property {0} was not found in the mapping of the type {1}.";

        private bool _hasAggregate;

        public string Select(Query query)
        {
            StringBuilder queryString = new StringBuilder();

            _hasAggregate = false;
            string where = SelectWhere(query);

            queryString.Append("SELECT ");

            queryString.Append(SelectTop(query));
            queryString.Append(SelectColumns(query));

            queryString.Append(" FROM ");
            queryString.Append(query.Table);
            queryString.Append(" AS ");
            queryString.Append(query.Alias);

            queryString.Append(SelectTables(query));

            queryString.Append(" WHERE 1 = 1");
            queryString.Append(where);

            if(_hasAggregate)
            {
                queryString.Append(" GROUP BY ");
                queryString.Append(SelectGroup(query));
            }

            queryString.Append(SelectOrder(query));

            return queryString.ToString();
        }

        private string SelectTop(Query query)
        {
            StringBuilder top = new StringBuilder();
            for (int i = 0; i < query.Criterias.Count; i++)
            {
                if (query.Criterias[i].Type != CriteriaType.Limit) { continue; }

                top.Append("TOP " + (query.Criterias[i] as LimitCriteria).Value.ToString() + " ");
            }

            return top.ToString();
        }

        private string SelectColumns(Query query)
        {
            StringBuilder columns = new StringBuilder();

            bool comma = false;
            for (int i = 0; i < query.Columns.Count; i++)
            {
                if (comma) { columns.Append(", "); }
                columns.Append(String.Format(query.Columns[i].Command, query.Columns[i].Query.Alias + "." + query.Columns[i].Name));
                columns.Append(" AS ");
                columns.Append(query.Columns[i].Query.Alias);
                columns.Append("_");
                columns.Append(query.Columns[i].Name);
                comma = true;

                if (query.Columns[i].IsAggregate) { _hasAggregate = true; }
            }

            for (int i = 0; i < query.Joins.Count; i++)
            {
                if (query.Joins[i].IsLazy) { continue; }

                string joinColumns = SelectColumns(query.Joins[i].Query);
                if (!String.IsNullOrEmpty(joinColumns))
                {
                    if (comma) { columns.Append(", "); }
                    columns.Append(joinColumns);
                    comma = true;
                }
            }

            return columns.ToString();
        }

        private string SelectTables(Query query)
        {
            StringBuilder tables = new StringBuilder();

            for (int i = 0; i < query.Joins.Count; i++)
            {
                if (query.Joins[i].IsLazy) { continue; }

                switch (query.Joins[i].JoinType)
                {
                    case JoinType.LeftJoin: { tables.Append(" LEFT JOIN "); break; }
                    case JoinType.RightJoin: { tables.Append(" RIGHT JOIN "); break; }
                    case JoinType.InnerJoin: { tables.Append(" INNER JOIN "); break; }
                }

                tables.Append(query.Joins[i].Query.Table);
                tables.Append(" AS ");
                tables.Append(query.Joins[i].Query.Alias);

                for (int j = 0; j < query.Joins[i].On.Length; j++)
                {
                    tables.Append(j == 0 ? " ON " : " AND ");
                    tables.Append(query.Joins[i].Query.Alias);
                    tables.Append(".");
                    tables.Append(query.Joins[i].On[j].ForeignKey);

                    tables.Append(" = ");

                    tables.Append(query.Alias);
                    tables.Append(".");
                    tables.Append(query.Joins[i].On[j].PrimaryKey);
                }

                tables.Append(SelectTables(query.Joins[i].Query));
            }

            return tables.ToString();
        }

        private string SelectWhere(Query query)
        {
            StringBuilder queryString = new StringBuilder();
            for (int i = 0; i < query.Criterias.Count; i++)
            {
                if (query.Criterias[i].Type == CriteriaType.Order) { continue; }

                queryString.Append(EvaluateCriteria(query.Criterias[i], query));
            }
            
            return queryString.ToString();
        }

        private string SelectGroup(Query query)
        {
            StringBuilder columns = new StringBuilder();

            bool comma = false;
            for (int i = 0; i < query.Columns.Count; i++)
            {
                if (query.Columns[i].IsAggregate) { continue; }

                if (comma) { columns.Append(", "); }
                columns.Append(query.Columns[i].Query.Alias);
                columns.Append(".");
                columns.Append(query.Columns[i].Name);
                comma = true;
            }

            for (int i = 0; i < query.Joins.Count; i++)
            {
                if (query.Joins[i].IsLazy) { continue; }

                string joinColumns = SelectGroup(query.Joins[i].Query);
                if (!String.IsNullOrEmpty(joinColumns))
                {
                    if (comma) { columns.Append(", "); }
                    columns.Append(joinColumns);
                    comma = true;
                }
            }

            return columns.ToString();
        }

        private string SelectOrder(Query query)
        {
            StringBuilder order = new StringBuilder();

            bool comma = false;
            for (int i = 0; i < query.Criterias.Count; i++)
            {
                if (query.Criterias[i].Type != CriteriaType.Order) { continue; }

                if (comma) { order.Append(", "); } else { order.Append(" ORDER BY "); }
                order.Append(EvaluateCriteria(query.Criterias[i], query));
                comma = true;
            }

            return order.ToString();
        }

        public string Update(Query query, object element)
        {
            StringBuilder queryString = new StringBuilder();

            queryString.Append("UPDATE ");
            queryString.Append(query.Alias);
            queryString.Append(" SET ");

            queryString.Append(UpdateColumns(query, element));

            queryString.Append(" FROM ");
            queryString.Append(query.Table);
            queryString.Append(" AS ");
            queryString.Append(query.Alias);

            queryString.Append(SelectTables(query));

            queryString.Append(" WHERE 1 = 1");
            queryString.Append(SelectWhere(query));

            return queryString.ToString();
        }

        private string UpdateColumns(Query query, object element)
        {
            StringBuilder columns = new StringBuilder();

            bool comma = false;
            for (int i = 0; i < query.Columns.Count; i++)
            {
                if (query.Columns[i].IsKey || !query.Columns[i].Query.Equals(query) || query.Columns[i].IsAggregate) { continue; }

                if (comma) { columns.Append(", "); }
                columns.Append(query.Columns[i].Name);
                columns.Append(" = ");
                columns.Append(FormatWriteValue(query.Columns[i].Property.GetValue(element), query.Columns[i].ResolvedType));
                comma = true;
            }

            return columns.ToString();
        }

        //private string UpdateTables(IQuery from, object element)
        //{
        //    StringBuilder query = new StringBuilder();

        //    for (int i = 0; i < from.Joins.Count; i++)
        //    {
        //        switch (from.Joins[i].JoinType)
        //        {
        //            case JoinType.LeftJoin: { query.Append(" LEFT JOIN "); break; }
        //            case JoinType.RightJoin: { query.Append(" RIGHT JOIN "); break; }
        //            case JoinType.InnerJoin: { query.Append(" INNER JOIN "); break; }
        //        }

        //        if (!String.IsNullOrEmpty(from.Joins[i].Class.Table.Database)) { query.Append(from.Joins[i].Class.Table.Database + "."); }
        //        if (!String.IsNullOrEmpty(from.Joins[i].Class.Table.Schema)) { query.Append(from.Joins[i].Class.Table.Schema + "."); }
        //        query.Append(from.Joins[i].Class.Table.Name);
        //        query.Append(" AS ");
        //        query.Append(from.Joins[i].Alias);

        //        for (int j = 0; j < from.Joins[i].Criterias.Length; j++)
        //        {
        //            query.Append(j == 0 ? " ON " : " AND ");
        //            query.Append(from.Joins[i].Alias);
        //            query.Append(".");
        //            query.Append(from.Joins[i].Criterias[j].ForeignKey);

        //            query.Append(" = ");

        //            query.Append(from.Alias);
        //            query.Append(".");
        //            query.Append(from.Joins[i].Criterias[j].PrimaryKey);
        //        }

        //        query.Append(SelectTables(from.Joins[i]));
        //    }

        //    return query.ToString();
        //}

        public string Insert(Query query, object element)
        {
            StringBuilder queryString = new StringBuilder();

            queryString.Append("INSERT INTO ");

            queryString.Append(query.Table);

            queryString.Append(" (");
            queryString.Append(InsertColumns(query, element));
            queryString.Append(") VALUES (");
            queryString.Append(InsertValues(query, element));
            queryString.Append(") ");
            queryString.Append("SELECT @@IDENTITY AS ID");

            return queryString.ToString();
        }

        private string InsertColumns(Query query, object element)
        {
            StringBuilder columns = new StringBuilder();

            bool comma = false;
            for (int i = 0; i < query.Columns.Count; i++)
            {
                if (query.Columns[i].IsKey || !query.Columns[i].Query.Equals(query) || query.Columns[i].IsAggregate) { continue; }

                if (comma) { columns.Append(", "); }
                columns.Append(query.Columns[i].Name);
                comma = true;
            }

            return columns.ToString();
        }

        private string InsertValues(Query query, object element)
        {
            StringBuilder queryString = new StringBuilder();

            bool comma = false;
            for (int i = 0; i < query.Columns.Count; i++)
            {
                if (query.Columns[i].IsKey || !query.Columns[i].Query.Equals(query) || query.Columns[i].IsAggregate) { continue; }

                if (comma) { queryString.Append(", "); }
                queryString.Append(FormatWriteValue(query.Columns[i].Property.GetValue(element), query.Columns[i].ResolvedType));
                comma = true;
            }

            return queryString.ToString();
        }
        
        public string Delete(Query query, object element)
        {
            StringBuilder queryString = new StringBuilder();

            queryString.Append("DELETE FROM ");
            queryString.Append(query.Table);
            queryString.Append(" AS ");
            queryString.Append(query.Alias);
            queryString.Append(" WHERE 1 = 1");
            queryString.Append(SelectWhere(query));            

            return queryString.ToString();
        }

        private string EvaluateCriteria(Criteria criteria, Query query)
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
                        Query currentTable = query;

                        Column property = null;
                        while ((criteria as MemberCriteria) != null)
                        {
                            property = null;
                            if ((criteria as MemberCriteria).Member == null)
                            {
                                property = currentTable.Columns.FirstOrDefault(c => c.Property.Name == (criteria as MemberCriteria).Name);
                                if (property != null) { break; }

                                Join join = currentTable.Joins.FirstOrDefault(j => j.IsBase);
                                if (join == null) { throw new ArgumentException(String.Format(UnableToResolveProperty, (criteria as MemberCriteria).Name, currentTable.Type.Name)); }
                                currentTable = join.Query;
                                continue;
                            }
                            else
                            {
                                Join join = currentTable.Joins.FirstOrDefault(j => j.Property.Name == (criteria as MemberCriteria).Name);
                                if (join == null) { throw new ArgumentException(String.Format(UnableToResolveProperty, (criteria as MemberCriteria).Name, currentTable.Type.Name)); }
                                
                                join.IsLazy = false;
                                currentTable = join.Query;
                            }

                            criteria = (criteria as MemberCriteria).Member;
                        }

                        if (property.IsAggregate) { condition.Append(property.Query.Alias + "_" + property.Name); }
                        //condition.Append(currentTable.Alias + "." + property.Name);
                        else { condition.Append(property.Query.Alias + "." + property.Name); }
                        break;
                    }
                case CriteriaType.Binary:
                    {
                        if ((criteria as BinaryCriteria).UseBrackets) { condition.Append("("); }

                        condition.Append(EvaluateCriteria((criteria as BinaryCriteria).LeftValue, query));
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
                        condition.Append(EvaluateCriteria((criteria as BinaryCriteria).RightValue, query));

                        if ((criteria as BinaryCriteria).UseBrackets) { condition.Append(")"); }
                        break;
                    }
                case CriteriaType.Order:
                    {
                        condition.Append(EvaluateCriteria((criteria as OrderCriteria).Member, query));
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

        public string FormatWriteValue(object value, Type valueType)
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
    }
}